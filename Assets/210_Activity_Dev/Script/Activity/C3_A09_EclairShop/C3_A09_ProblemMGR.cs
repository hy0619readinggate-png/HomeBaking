using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C3_A09;

namespace DoDoEng.Activity.C3_A09
{
    public class C3_A09_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Constants
        private const int EXAMPLE_COUNT = 6;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 4, "curriculum.ProblemCount must be 4", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var customerIDs = UtilArray.Random(1, curriculum.ProblemCount);
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    CustomerID = customerIDs[i],
                    Sentence = p.Sentence,
                    SentenceCLIP = await loadSound(p.SoundSentence),
                    SentenceSPR = await loadSprite(p.ImageSentence),
                    Subjects = buildSubjects(p),
                    Examples = buildExamples(tables, p)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList
                            .Sum(p => p.Subjects.Count());

            return problemList.ToArray();
        }

        // Functions
        private SubjectData[] buildSubjects(ActivityData problem)
        {
            return problem.Texts.Select((t, i) => new SubjectData
            {
                ID = i + 1,
                Text = t
            }).ToArray();
        }
        private ExampleData[] buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ExampleData>();

            // 문제의 보기
            foreach (var (t, i) in problem.Texts.Select((t, i) => (t, i)))
            {
                list.Add(new ExampleData
                {
                    ID = i + 1,
                    Text = t,
                });
            }

            // 오답 추가
            var wrongList = new List<string>();
            var wrongPool = tables.Where(t => t.Index != problem.Index).Select(t => t.Texts);

            foreach (var texts in wrongPool)
            {
                foreach (var text in texts)
                {
                    if (!problem.Texts.Exists(t => t == text))
                        wrongList.Add(text);
                }
            }

            var wrongArr = wrongList.Distinct().ToArray();
            var extractCount = EXAMPLE_COUNT - problem.Texts.Length;
            var wrongTexts = UtilArray.Extract(wrongArr, extractCount);

            foreach (var t in wrongTexts)
            {
                list.Add(new ExampleData
                {
                    ID = -1,
                    Text = t
                });
            }
            var randomIDs = UtilArray.Random(1, EXAMPLE_COUNT);
            foreach (var (exam, i) in list.Select((exam, i) => (exam, i)))
            {
                exam.SkinID = randomIDs[i];
            }
            return UtilArray.Shuffled(list.ToArray());
        }
    }

    public class ProblemData
    {
        public int Index;
        public int CustomerID;
        public string Sentence;
        public AudioClip SentenceCLIP;
        public Sprite SentenceSPR;
        public SubjectData[] Subjects;
        public ExampleData[] Examples;

        // Properties
        public int SubjectCount => Subjects.Length;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Sentence} | {string.Join(",", Examples.Select(ex => ex.Text))}]" +
                $"</color>";
        }
    }
    public class SubjectData
    {
        public int ID;
        public string Text;
    }
    public class ExampleData
    {
        public int ID;
        public int SkinID;
        public string Text;
    }
}