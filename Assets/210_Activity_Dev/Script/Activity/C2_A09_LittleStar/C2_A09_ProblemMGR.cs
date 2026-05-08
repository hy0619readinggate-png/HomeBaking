using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A09;

namespace DoDoEng.Activity.C2_A09
{
    public class C2_A09_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int WRONG_EXAMPLE_COUNT = 4;
        private const int PROBLEM_COUNT_IN_CONSTELLATION = 3;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 6 ||
                curriculum.ProblemCount == 4,
                "curriculum.ProblemCount must be 6 or 4", this);

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
                    WordSPR = await loadSprite(p.ImageWord),
                    PhonicsCLIP = await loadSound(p.SoundPhonetic),
                    WordCLIP = await loadSound(p.SoundWord),
                    Seq = i % PROBLEM_COUNT_IN_CONSTELLATION + 1,
                    FirstStar = i % PROBLEM_COUNT_IN_CONSTELLATION == 0,
                    Examples = await buildExamples(tables, p)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            // 오답 보기
            var wrongPool = tables
                .Where(t => t.Phonics != problem.Phonics)
                .GroupBy(t => t.Phonics)
                .Select(g => UtilArray.ExtractOne(g.ToArray()))
                .ToArray();
            var wrong = UtilArray.Extract(wrongPool, WRONG_EXAMPLE_COUNT);

            var examList = new List<ExampleData>();
            foreach (var w in wrong)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = false,
                    Phonics = w.Phonics,
                    PhoneticCLIP = await loadSound(w.SoundPhonetic)
                });
            }

            // 정답 보기
            examList.Add(new ExampleData
            {
                IsAnswer = true,
                Phonics = problem.Phonics,
                PhoneticCLIP = await loadSound(problem.SoundPhonetic)
            });

            var exams = examList.ToArray();
            return UtilArray.Shuffle(exams);
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Word;
        public Sprite WordSPR;
        public AudioClip PhonicsCLIP;
        public AudioClip WordCLIP;
        public int Seq;
        public bool FirstStar;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Word} | {FirstStar} | {string.Join(",", Examples.Select(ex => ex.Phonics))}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Phonics;
        public AudioClip PhoneticCLIP;

        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";
            return $"<color=red>ExampleData[{Phonics} {PhoneticCLIP} {ox}]</color>";
        }
    }
}