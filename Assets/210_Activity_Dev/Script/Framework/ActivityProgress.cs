using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.Framework
{
    public class ActivityProgress : BYDSingleton<ActivityProgress>
    {
        // Properties
        public ActivityResult Result => result;

        // Methods
        public void Setup(int blanksCount)
        {
            LOG.Info($"{nameof(Setup)}() | {blanksCount}", this);

            result.Init(blanksCount);
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
        public void Wrong(int count = 1)
        {
            LOG.Info($"{nameof(Wrong)}() | {count}", this);

            result.Wrong(count);
        }



        // Fields
        private ActivityResult result = new ActivityResult();
        private float startTime = 0;
    }

    public class ActivityResult
    {
        // Properties
        public int PlayingTime { get; set; }  // Seconds
        public int BlanksCount { get; private set; } = 10;
        public int WrongCount { get; private set; }

        // Properties
        public int WrongRate => Mathf.RoundToInt(WrongCount / ((float)BlanksCount + WrongCount) * 100);

        // Methods
        public void Init(int blanksCount)
        {
            BlanksCount = blanksCount;
            WrongCount = 0;
        }
        public void Wrong(int count = 1)
        {
            WrongCount += count;
        }



        // Overrides
        public override string ToString()
        {
            return $"<b><color=red>ActivityResult[" +
               $"PlayingTime:{PlayingTime}s | " +
               $"BlanksCount:{BlanksCount} " +
               $"WrongCount:{WrongCount} " +
               $"WrongRate:{WrongCount}/{BlanksCount + WrongCount} {WrongRate}%" +
               $"]</color></b>";
        }
    }
}