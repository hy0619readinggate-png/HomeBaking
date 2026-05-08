using beyondi.Behaviour;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.EBook.Framework
{
    public class EBookQuizProgress : BYDSingleton<EBookQuizProgress>
    {
        // Properties
        public EBookQuizResult Result => result;

        // Methods
        public void Setup(int problemCount)
        {
            LOG.Info($"{nameof(Setup)}() | {problemCount}", this);

            result.Init(problemCount);
        }
        public void StartMeasureOfPlayingTime()
        {
            LOG.Info($"{nameof(StartMeasureOfPlayingTime)}()", this);
            startTime = Time.time;
        }
        public void FinishMeasureOfPlayingTime()
        {
            LOG.Info($"{nameof(FinishMeasureOfPlayingTime)}()", this);
            result.PlayingTime = Mathf.RoundToInt(Time.time - startTime);
        }
        public void Correct(int pNO, bool correct)
        {
            LOG.Info($"{nameof(Correct)}() | {pNO}, {correct}", this);

            result.Correct(pNO, correct);
        }
        public void ScreenShot(int pNO, Sprite sprite)
        {
            LOG.Info($"{nameof(ScreenShot)}() | {pNO}", this);

            result.ScreenShot(pNO, sprite);
        }



        // Fields
        private EBookQuizResult result = new EBookQuizResult();
        private float startTime = 0;
    }

    public class EBookQuizResult
    {
        // Constants
        // 80% 이상 : Awesome
        // 60% 이상 : GreatJob
        // 60% 이하 : GreatJob
        private static float[] emblemForCorrectCounts = new float[] { 60, 80 };

        // Properties
        public int PlayingTime { get; set; }  // Seconds
        public int ProblemCount { get; private set; }
        public Sprite[] CapturedImages { get; set; } // 퀴즈 결과 캡쳐 이미지
        public bool[] Corrections { get; private set; } // 정오답 결과

        // Properties
        public int CorrectRate => Mathf.RoundToInt(Corrections.Count(c => c) / (float)ProblemCount * 100);
        public int GetEmblemIdx()
        {
            return emblemForCorrectCounts.TakeWhile(r => r <= CorrectRate).Count();
        }




        // Methods
        public void Init(int problemCount)
        {
            ProblemCount = problemCount;
            CapturedImages = new Sprite[ProblemCount];
            Corrections = new bool[ProblemCount];
        }
        public void Correct(int pNO, bool correct)
        {
            Corrections[pNO - 1] = correct;
        }
        public void ScreenShot(int pNO, Sprite sprite)
        {
            CapturedImages[pNO - 1] = sprite;
        }



        // Overrides
        public override string ToString()
        {
            return $"<b><color=red>EBookResult[" +
                $"ProblemCount:{ProblemCount}" +
                $"PlayingTime:{PlayingTime}s" +
                $"Corrections:{string.Join(",", Corrections.Select(c => c))}" +
                $"PlayingTime:{CorrectRate}" +
                $"]</color></b>";
        }
    }
}