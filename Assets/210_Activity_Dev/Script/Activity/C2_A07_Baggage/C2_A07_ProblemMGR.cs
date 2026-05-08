using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A07;

namespace DoDoEng.Activity.C2_A07
{
    public class C2_A07_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int WRONG_EXAMPLE_COUNT = 2;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 6, "curriculum.ProblemCount must be 6", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();

            // 음가별 문제 수 생성
            var phonetics = problemsPool.Select(p => p.PhonicsForProblem).Distinct().ToArray();
            var problemCounts = UtilArray.Distribute(phonetics.Length, curriculum.ProblemCount);
            var cartCount = curriculum.ProblemCount / phonetics.Length;

            UtilArray.Shuffle(phonetics);

            // 음가별 문제 추출
            var problemList = new List<ProblemData>();
            for (var i = 0; i < phonetics.Length; i++)
            {
                var phoneticProblems = problemsPool.Where(p => p.PhonicsForProblem == phonetics[i]).ToArray();
                var extrected = UtilArray.Extract(phoneticProblems, problemCounts[i]);
                foreach (var p in extrected)
                {
                    problemList.Add(new ProblemData
                    {
                        Index = p.Index,
                        Text = p.Phonics,
                        CartCount = cartCount,
                        LastCart = extrected.Last() == p,
                        PhoneticCLIP = await loadSound(p.SoundPhonetic),
                        Examples = await buildExamples(tables, p)
                    });
                }
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Select(p => p.CartCount).Sum();

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ActivityData>();

            // 정답 추가
            list.Add(problem);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Phonics != problem.Phonics).ToArray();
            var wrong = UtilArray.Extract(wrongPool, WRONG_EXAMPLE_COUNT);
            list.AddRange(wrong);

            var examList = new List<ExampleData>();
            foreach (var ex in list)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Word = ex.Word,
                    WordSPR = await loadSprite(ex.ImageWord),
                    WordCLIP = await loadSound(ex.SoundWord)
                });
            }

            return UtilArray.Shuffle(examList.ToArray());
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public int CartCount;
        public bool LastCart;
        public AudioClip PhoneticCLIP;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Text} | {CartCount} | {LastCart} | {string.Join(",", Examples.Select(ex => ex.Word))}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Word;
        public Sprite WordSPR;
        public AudioClip WordCLIP;

        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";
            return $"<color=red>ExampleData[{Word} {ox}]</color>";
        }
    }
}