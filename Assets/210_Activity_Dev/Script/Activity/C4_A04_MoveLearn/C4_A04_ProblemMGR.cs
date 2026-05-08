using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ActivityData = DoDoEng.Common.ActivityData_C4_A04;

namespace DoDoEng.Activity.C4_A04
{
    public class C4_A04_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 2, "curriculum.ProblemCount must be 2", this);

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
                    Word = p.Words,
                    Movie = p.Movie,
                    WordCLIP = await loadSound(p.SoundWord)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count;

            return problemList.ToArray();
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Word;
        public string Movie;
        public AudioClip WordCLIP;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Word} | {Movie}]" +
                $"</color>";
        }
    }
}