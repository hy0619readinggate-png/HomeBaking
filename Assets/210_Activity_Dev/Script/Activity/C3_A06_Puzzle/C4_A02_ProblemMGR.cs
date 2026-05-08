using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;

using ActivityData = DoDoEng.Common.ActivityData_C4_A02;

namespace DoDoEng.Activity.C3_A06
{
    public class C4_A02_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int PEACE_COUNT = 4;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 4, "curriculum.ProblemCount must be 4", this);

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
                    Sentence = p.Sentence,
                    SentenceCLIP = await loadSound(p.SoundSentence),
                    SentenceSPR = await loadSprite(p.ImageSentence),
                    ExampleIDs = UtilArray.Random(1, PEACE_COUNT)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Select(p => p.ExampleIDs.Length).Sum();

            return problemList.ToArray();
        }
    }
}