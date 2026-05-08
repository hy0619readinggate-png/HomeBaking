using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A03;

namespace DoDoEng.Activity.C1_A03
{
    public class C1_A03_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Properties
        public int ExampleCount => 3;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"Build() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 3, "problem count must be 3", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var problem in problems)
            {
                problemList.Add(new ProblemData
                {
                    Index = problem.Index,
                    Text = problem.Text,
                    PhoneticCLIP = await loadSound(problem.SoundPhonetic),
                    Examples = await buildExamples(tables, problem)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var problems = UtilArray.Extract(tables, 3);

            if (!problems.Contain(problem))
            {
                var idx = Random.Range(0, problems.Length);
                problems[idx] = problem;
            }

            var examList = new List<ExampleData>();
            foreach (var p in problems)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = p.Index == problem.Index,
                    Text = p.Text,
                    PhoneticCLIP = await loadSound(p.SoundPhonetic)
                });
            }

            return examList.ToArray();
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public AudioClip PhoneticCLIP;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Text} | " +
                $"{Examples[0].Text},{Examples[0].IsAnswer} | " +
                $"{Examples[1].Text},{Examples[1].IsAnswer} | " +
                $"{Examples[2].Text},{Examples[2].IsAnswer}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Text;
        public AudioClip PhoneticCLIP;
    }
}