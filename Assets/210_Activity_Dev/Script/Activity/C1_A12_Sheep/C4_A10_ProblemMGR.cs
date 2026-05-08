using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C4_A10;

namespace DoDoEng.Activity.C1_A12
{
    public class C4_A10_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definition
        const int MIN_SHEEP = 5;
        const int MAX_SHEEP = 6;

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
                var sheepCount = UtilArray.RandomOne(MIN_SHEEP, MAX_SHEEP);
                var sheepTypes = UtilArray.Random(1, 3, sheepCount);

                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Text,
                    WordCLIP = await loadSound(p.SoundWord),
                    Examples = await buildExamples(tables, p),
                    SheepTypes = sheepTypes
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Select(p => p.SheepTypes.Length).Sum();

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ActivityData>();

            // 정답 추가
            list.Add(problem);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Text != problem.Text).ToArray();
            var wrong = UtilArray.Extract(wrongPool, 1);
            list.AddRange(wrong);

            var examList = new List<ExampleData>();
            foreach (var ex in list)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Text = ex.Text,
                    WordSPR = await loadSprite(ex.ImageWord),
                    WordCLIP = await loadSound(ex.SoundWord)
                });
            }

            return UtilArray.Shuffle(examList.ToArray());
        }
    }
}