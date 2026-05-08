using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;

using ActivityData = DoDoEng.Common.ActivityData_C3_A01;

namespace DoDoEng.Activity.C2_A01
{
    public class C3_A01_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 8, "curriculum.ProblemCount must be 8", this);

            // 문제 데이터 추출
            // #722 [C3A1] 문항 추가에 따른 테이블 관련 문의
            // 이미지 없는 문제 4개 + 이미지 있는 문제 4개
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var xImagePool = problemsPool.Where(t => string.IsNullOrEmpty(t.ImageWord)).ToArray();
            var oImagePool = problemsPool.Where(t => !string.IsNullOrEmpty(t.ImageWord)).ToArray();

            var xProblems = UtilArray.Extract(xImagePool, curriculum.ProblemCount / 2);
            var oProblems = UtilArray.Extract(oImagePool, curriculum.ProblemCount / 2);

            var problems = Enumerable.Concat(xProblems, oProblems).ToArray();

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Word = p.Word,
                    WordSTR = p.Word,
                    WordSPR = await loadSprite(p.ImageWord),
                    WordCLIP = await loadSound(p.SoundWord)
                });
            }

            await UniTask.Yield();

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }
    }
}