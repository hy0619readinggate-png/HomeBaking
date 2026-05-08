using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A02;

namespace DoDoEng.Activity.C2_A02
{
    public class C2_A02_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Constants
        private const int EXAMPLE_COUNT = 3;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 6, "curriculum.ProblemCount must be 6", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();

            // 음가별 문제 수 생성
            var phonetics = problemsPool.Select(p => p.Phonics).Distinct().ToArray();
            var problemCounts = UtilArray.Distribute(phonetics.Length, curriculum.ProblemCount);

            UtilArray.Shuffle(phonetics);

            // 음가별 문제 생성
            var problemList = new List<ProblemData>();
            for (var i = 0; i < phonetics.Length; i++)
            {
                var phoneticProblems = problemsPool.Where(p => p.Phonics == phonetics[i]).ToArray();
                var extrected = UtilArray.Extract(phoneticProblems, problemCounts[i]);
                foreach (var p in extrected)
                {
                    problemList.Add(new ProblemData
                    {
                        Index = p.Index,
                        Word = p.Word,
                        WordCLIP = await loadSound(p.SoundWord),
                        WordSPR = await loadSprite(p.ImageWord),
                        Subjects = buildSubjects(p),
                        Examples = await buildExamples(tables, p)
                    });
                }
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList
                            .Sum(p => p.Subjects.Count(s => !s.IsFix));

            return problemList.ToArray();
        }

        // Functions
        private SubjectData[] buildSubjects(ActivityData problem)
        {
            return problem.Texts.Select(t => new SubjectData
            {
                Text = t.Text,
                IsFix = !t.IsPhonics && problem.IsFix   // 고정문제이고, 음가가 아닌 것들만 IsFix=true
            }).ToArray();
        }
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ExampleData>();

            // 문제의 보기
            foreach (var t in problem.Texts)
            {
                if (t.IsPhonics || !problem.IsFix)
                    list.Add(new ExampleData
                    {
                        Text = t.Text,
                        PhoniceCLIP = await loadSound(t.Sound)
                    });
            }

            // 추가 보기
            var lackCount = EXAMPLE_COUNT - list.Count;
            if (lackCount > 0)
            {
                var poneticList = tables
                    .Where(p => p.Group == problem.Group)
                    .Select(p => p.PhoneticText)
                    .Distinct();

                var wrongPoneticList = new List<ActivityData>();
                foreach (var p in poneticList)
                {
                    var contain = list.Select(p => p.Text).Contains(p);
                    if (!contain)
                    {
                        var t = tables.Where(t => t.PhoneticText == p).ToArray();
                        wrongPoneticList.Add(UtilArray.ExtractOne(t));
                    }
                }
                var wrongPool = wrongPoneticList.ToArray();

                var wrongList = new List<ExampleData>();
                var extractedWrong = UtilArray.Extract(wrongPool, lackCount);
                foreach (var t in extractedWrong)
                {
                    wrongList.Add(new ExampleData
                    {
                        Text = t.PhoneticText,
                        PhoniceCLIP = await loadSound(t.PhoneticSound)
                    });
                }

                list.AddRange(wrongList);
            }

            return UtilArray.Shuffled(list.ToArray());
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Word;
        public AudioClip WordCLIP;
        public Sprite WordSPR;
        public SubjectData[] Subjects;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Word} | {string.Join(",", Subjects.Select(s => s.Text))} | {string.Join(",", Examples.Select(e => e.Text))}]" +
                $"</color>";
        }
    }
    public class SubjectData
    {
        public string Text;
        public bool IsFix;
    }
    public class ExampleData
    {
        public string Text;
        public AudioClip PhoniceCLIP;
    }
}