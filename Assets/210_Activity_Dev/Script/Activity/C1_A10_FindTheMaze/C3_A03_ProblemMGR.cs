using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C3_A03;

namespace DoDoEng.Activity.C1_A10
{
    public class C3_A03_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 3, "curriculum.ProblemCount must be 4", this);

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
                    ProblemCLIP = await loadSound(p.SoundWord),
                    Examples = await buildExamples(tables, p)
                });
            }

            // 2번째는 2번째보기가 정답이 되지 않도록함 #488
            var excepted = problemList[1].Examples[1].IsAnswer;
            if (excepted)
            {
                var examples = problemList[1].Examples;
                var changeIdx = UtilRandom.RandomSuccess(0.5f) ? 0 : 2;
                var temp = examples[changeIdx];
                examples[changeIdx] = examples[1];
                examples[1] = temp;
            }

            await UniTask.Yield();

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var exams = new List<ActivityData>();

            // 정답 추가
            exams.Add(problem);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Text != problem.Text).ToArray();
            var wrong = UtilArray.Extract(wrongPool, 2);
            exams.AddRange(wrong);

            // 보기 만들기
            var examList = new List<ExampleData>();
            foreach (var ex in exams)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Text = ex.Text,
                    PhoneticCLIP = await loadSound(ex.SoundWord)
                });
            }
            var examData = examList.ToArray();
            UtilArray.Shuffle(examData);

            return examData;

        }
    }
}