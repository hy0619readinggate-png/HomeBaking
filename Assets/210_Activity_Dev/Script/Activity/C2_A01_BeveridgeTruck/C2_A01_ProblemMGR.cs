using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A01;

namespace DoDoEng.Activity.C2_A01
{
    public class C2_A01_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);

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
                    WordSTR = p.WordSTR,
                    WordSPR = await loadSprite(p.Image),
                    WordCLIP = await loadSound(p.SoundWord)
                });
            }

            await UniTask.Yield();

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Word;
        public string WordSTR;
        public Sprite WordSPR;
        public AudioClip WordCLIP;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Word}]" +
                $"</color>";
        }
    }
}