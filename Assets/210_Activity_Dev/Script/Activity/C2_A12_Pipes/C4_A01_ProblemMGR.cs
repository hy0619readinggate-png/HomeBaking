using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;

using ActivityData = DoDoEng.Common.ActivityData_C4_A01;

namespace DoDoEng.Activity.C2_A12
{
    public class C4_A01_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int WRONG_EXAMPLE_COUNT = 6;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 6, "curriculum.ProblemCount must be 6", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var subjects = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            for (var i = 0; i < subjects.Length; i += 2)
            {
                var p1 = subjects[i];
                var p2 = subjects[i + 1];
                var subject1 = new SubjectData
                {
                    Index = p1.Index,
                    Word = p1.Word,
                    WordCLIP = await loadSound(p1.SoundWord),

                };
                var subject2 = new SubjectData
                {
                    Index = p2.Index,
                    Word = p2.Word,
                    WordCLIP = await loadSound(p2.SoundWord),
                };

                problemList.Add(new ProblemData
                {
                    Subjects = new SubjectData[] { subject1, subject2 },
                    Examples = await buildExamples(tables, p1, p2)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Select(p => p.Subjects.Length).Sum();

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem1, ActivityData problem2)
        {
            var list = new List<ActivityData>();

            // 정답 추가
            list.Add(problem1);
            list.Add(problem2);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Word != problem1.Word && t.Word != problem2.Word).ToArray();
            var wrong = UtilArray.Extract(wrongPool, WRONG_EXAMPLE_COUNT);
            list.AddRange(wrong);

            var examList = new List<ExampleData>();
            foreach (var ex in list)
            {
                examList.Add(new ExampleData
                {
                    Word = ex.Word,
                    WordCLIP = await loadSound(ex.SoundWord),
                    WordSPR = await loadSprite(ex.ImageWord)
                });
            }

            var examData = examList.ToArray();
            UtilArray.Shuffle(examData);

            return examData;
        }
    }
}