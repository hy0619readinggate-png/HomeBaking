using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.Framework
{
    public class GameProgress : BYDSingleton<GameProgress>
    {
        // Properties
        public GameResult Result => result;

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
        public void Correct(int count = 1)
        {
            LOG.Info($"{nameof(Correct)}() | {count}", this);

            result.Correct(count);
        }



        // Fields
        private GameResult result = new GameResult();
        private float startTime = 0;
    }

    public class GameResult
    {
        // Properties
        public int PlayingTime { get; set; }  // Seconds
        public int ProblemCount { get; private set; }
        public int CorrectCount { get; private set; }

        // Properties
        public int CorrectRate => Mathf.RoundToInt(CorrectCount / (float)ProblemCount * 100);
        public int EarnStar => GameReward.GetStarCountFor(CorrectCount, ProblemCount);

        // Methods
        public void Init(int problemCount)
        {
            ProblemCount = problemCount;
            CorrectCount = 0;
        }
        public void Correct(int count = 1)
        {
            CorrectCount += count;
        }



        // Overrides
        public override string ToString()
        {
            return $"<b><color=red>GameResult[" +
                $"PlayingTime:{PlayingTime}s | " +
                $"Correct:{CorrectCount}/{ProblemCount} {CorrectRate}% | " +
                $"Star:{EarnStar}" +
                $"]</color></b>";
        }
    }
}