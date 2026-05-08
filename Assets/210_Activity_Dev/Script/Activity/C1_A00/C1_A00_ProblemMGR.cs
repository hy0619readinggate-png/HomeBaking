using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A00;

namespace DoDoEng.Activity.C1_A00
{
    public class C1_A00_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            //LOG.Assert(curriculum.ProblemCount == 2, "curriculum.ProblemCount must be 2", this);
            //LOG.Assert(config.ProblemConfigs.Length == 3, "config.ProblemConfigs.Length must be 3", this);

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
                    Text = p.Text,
                    //Examples = buildExamples(tables, p, config.ProblemConfigs[i])
                });
            }

            await UniTask.Yield();

            return problemList.ToArray();
        }

        // Functions
        private ExampleData[] buildExamples(ActivityData[] tables, ActivityData problem, ProblemConfig config)
        {
            return null;

        }



        // Unity Inspectors
        //[Header("★ Configs")]
        //[SerializeField] private ConfigSO config = null;
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
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