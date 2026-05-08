using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C4_A09;

namespace DoDoEng.Activity.C2_A06
{
    public class C4_A09_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Constants
        private const int EXAMPLE_PER_PROBLEM_COUNT = 2;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();

            // 문제 풀 생성
            var queue = fill(problemsPool.Length, curriculum.ProblemCount * EXAMPLE_PER_PROBLEM_COUNT);
            queue.ForEach(q => LOG.Info($"{q}", this));
            var yogaposes = UtilArray.Sequential(1, yogaPoseCount);
            var problemYogaPoses = UtilArray.Extract(yogaposes, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            for (var i = 0; i < queue.Length; i = i + EXAMPLE_PER_PROBLEM_COUNT)
            {
                var sequences = new int[] { queue[i], queue[i + 1] };
                var examples = await buildExamples(problemsPool, sequences);

                problemList.Add(new ProblemData
                {
                    YogaPose = problemYogaPoses[(i / EXAMPLE_PER_PROBLEM_COUNT) % EXAMPLE_PER_PROBLEM_COUNT],
                    Examples = examples,
                    CharacterExamples = UtilArray.Shuffled(examples)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count * 2;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] problemPool, int[] sequences)
        {
            // 보기 추가

            var examList = new List<ExampleData>();

            foreach (var seq in sequences)
            {

                LOG.Info($"{sequences}, {seq}", this);
                var ex = problemPool[seq - 1];

                examList.Add(new ExampleData
                {
                    Word = ex.Word,
                    Image = await loadSprite(ex.ImageWord),
                    WordCLIP = await loadSound(ex.SoundWord)
                });
            }

            return examList.ToArray();
        }

        // Functions
        private int[] fill(int poolCount, int examCount)
        {
            var queue = new Queue<int>();
            fillQueue(queue, poolCount);

            var list = new List<int>();
            for (var i = 0; i < examCount; i++)
            {
                if (queue.Count() == 0)
                    fillQueue(queue, poolCount);

                var exist = false;
                do
                {
                    var v = queue.Peek();
                    exist = list.TakeLast(3).Contains(v);
                    if (exist)
                        queue.Enqueue(queue.Dequeue());
                }
                while (exist);

                list.Add(queue.Dequeue());
            }

            return list.ToArray();
        }
        private void fillQueue(Queue<int> queue, int poolCount)
        {
            var arr = UtilArray.Random(1, poolCount);
            arr.ForEach(a => queue.Enqueue(a));
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private int yogaPoseCount = 2;
    }
}