using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C4_A05;

namespace DoDoEng.Activity.C4_A05
{
    public class C4_A05_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Properties
        public bool IsIntroDataExist => introDatas.Length > 1;
        public IntroData[] IntroDatas => introDatas;

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
                    Texts = texts(p)
                });
            }

            // 인트로 데이터 생성
            var words = problemsPool
                        .Where(p => !string.IsNullOrEmpty(p.Word))
                        .Select(p => p.Word)
                        .Distinct()
                        .ToArray();
            var introList = new List<IntroData>();
            foreach (var word in words)
            {
                var introWords = problemsPool.Where(p => p.Word == word).ToArray();
                introList.Add(new IntroData
                {
                    Word = introWords[0].Word,
                    WordCLIP = await loadSound(introWords[0].SoundWord),
                    WordSPR = await loadSprite(introWords[0].ImageWord),
                    Sentence1 = introWords[0].Sentence,
                    Sentence2 = introWords[1].Sentence,
                    SentenceCLIP1 = await loadSound(introWords[0].SoundSentence),
                    SentenceCLIP2 = await loadSound(introWords[1].SoundSentence)
                });

            }
            introDatas = introList.ToArray();

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count;

            return problemList.ToArray();
        }



        // Fields
        private IntroData[] introDatas = null;

        // Functions
        private string[] texts(ActivityData problem)
        {
            var texts = new string[] { problem.chunk1, problem.chunk2, problem.chunk3, problem.chunk4, problem.chunk5 };
            return texts.Where(t => !string.IsNullOrEmpty(t)).ToArray();
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Sentence;
        public AudioClip SentenceCLIP;
        public string[] Texts;

        // Properties
        public int TextsCount => Texts.Length;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Sentence} | {string.Join(",", Texts)}]" +
                $"</color>";
        }
    }

    public class IntroData
    {
        public string Word;
        public AudioClip WordCLIP;
        public Sprite WordSPR;
        public string Sentence1;
        public string Sentence2;
        public AudioClip SentenceCLIP1;
        public AudioClip SentenceCLIP2;

        public override string ToString()
        {
            return $"<color=red>IntroData" +
                $"[{Word} | {Sentence1} | {Sentence2}]" +
                $"</color>";
        }
    }
}