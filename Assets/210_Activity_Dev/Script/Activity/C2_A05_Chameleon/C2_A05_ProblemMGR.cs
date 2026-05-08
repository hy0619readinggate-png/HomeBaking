using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A05;

namespace DoDoEng.Activity.C2_A05
{
    public class C2_A05_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int WRONG_EXAMPLE_COUNT = 4;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 4, "curriculum.ProblemCount must be 4", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();

            // 음가별 문제 수 생성
            var phonetics = problemsPool.Select(p => p.Phonics).Distinct().ToArray();
            var problemCounts = UtilArray.Distribute(phonetics.Length, curriculum.ProblemCount);

            // 음가별 문제 추출
            var problems = new List<ActivityData>();
            for (var i = 0; i < phonetics.Length; i++)
            {
                var phoneticProblems = problemsPool.Where(p => p.Phonics == phonetics[i]).Distinct().ToArray();
                var extrected = UtilArray.Extract(phoneticProblems, problemCounts[i]);
                extrected.ForEach(pp => problems.Add(pp));
            }

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {

                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Phonics,
                    Word = p.Word,
                    BlankWord = p.BlankWord,
                    ImageWord = await loadSprite(p.ImageWord),
                    WordCLIP = await loadSound(p.SoundWord),
                    Examples = buildExamples(p, p.ExampleTexts)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count;

            return problemList.ToArray();
        }

        // Functions
        private ExampleData[] buildExamples(ActivityData problem, string[] wrongExampleTexts)
        {
            var list = new List<string>();

            // 정답 벌 추가
            list.Add(problem.Phonics);

            // 오답 벌 추가
            var wrong = UtilArray.Extract(wrongExampleTexts, WRONG_EXAMPLE_COUNT);
            list.AddRange(wrong);

            var examData = list.Select(ex => new ExampleData
            {
                IsAnswer = ex == problem.Phonics,
                Text = ex
            }).ToArray();

            UtilArray.Shuffle(examData);

            return examData;

        }
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public string Word;
        public string BlankWord;
        public Sprite ImageWord;
        public AudioClip WordCLIP;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Text} | {string.Join(",", Examples.Select(ex => ex.Text))}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Text;
    }
}