using System.Linq;

namespace DoDoEng.Game.Framework
{
    public static class GameReward
    {
        // Properties
        public static float[] StarRatio => starRatio;

        // Methods
        public static int GetStarCountFor(int correct, int total)
        {
            var ratio = correct / (float)total;
            return starRatio.TakeWhile(r => r <= ratio).Count();
        }



        // 20% : 별 1개
        // 60% : 별 2개
        // 80% : 별 3개
        private static float[] starRatio = new float[] { 0.2f, 0.5f, 0.8f };
    }
}