using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.EBook.Framework
{
    public class EBookSingleProgress : BYDSingleton<EBookSingleProgress>
    {
        // Properties
        public EBookSingleResult Result => result;

        // Methods
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



        // Fields
        private EBookSingleResult result = new EBookSingleResult();
        private float startTime = 0;
    }

    public class EBookSingleResult
    {
        // Properties
        public int PlayingTime { get; set; }  // Seconds



        // Overrides
        public override string ToString()
        {
            return $"<b><color=red>EBookSingleResult[" +
               $"PlayingTime:{PlayingTime}s" +
               $"]</color></b>";
        }
    }
}