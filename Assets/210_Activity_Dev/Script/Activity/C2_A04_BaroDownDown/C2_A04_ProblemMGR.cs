using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A04;

namespace DoDoEng.Activity.C2_A04
{
    public class C2_A04_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int WRONG_EXAMPLE_COUNT = 2;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 6, "curriculum.ProblemCount must be 6", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Word = p.Word,
                    WordSPR = await loadSprite(p.ImageWord),
                    WordCLIP = await loadSound(p.SoundWord),
                    Examples = await buildExamples(tables, p)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ActivityData>();

            // 정답 추가
            list.Add(problem);

            // 오답 추가
            var wrongPool = tables
                .Where(t => t.Group == problem.Group && t.Word != problem.Word)
                .ToArray();
            var wrong = UtilArray.Extract(wrongPool, WRONG_EXAMPLE_COUNT);
            list.AddRange(wrong);

            var examList = new List<ExampleData>();
            foreach (var ex in list)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Word = ex.Word,
                    WordCLIP = await loadSound(ex.SoundWord),
                });
            }

            return UtilArray.Shuffle(examList.ToArray());
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Word;
        public Sprite WordSPR;
        public AudioClip WordCLIP;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Word} | {string.Join(",", Examples.Select(ex => ex.Word))}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Word;
        public AudioClip WordCLIP;
    }
}