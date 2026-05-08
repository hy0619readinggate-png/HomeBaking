using System.Linq;

namespace DoDoEng.Common
{
    public static class RunnerParam
    {
        public enum LaunchType
        {
            Day,
            Others
        }
        public enum MovieModeType
        {
            Watch,
            RecordStart,
            RecordPlay
        }

        // Properties
        public static LaunchType LaunchedFrom { get; set; } = LaunchType.Others;
        public static MovieModeType MovieMode { get; set; } = MovieModeType.Watch;
        public static string ReturnScene { get; set; }
        public static IndexBase SelectedIDX { get; set; }

        // Properties : LMS
        public static int LearningIndex { get; set; } = 0;

        // Properties : Playground
        public static int PlaygroundSlotNum { get; set; } = 1;
        public static GameIndex[] PlaygroundNexts { get; set; }

        // Properties : TodaysStudy
        public static int TodaysStage = 1;
        public static int TodaysDay = 1;
        public static int TodaysOrder = 2;
        public static bool TodaysCompleted = true;

        // Properties : debug
        public static string SkipStateTo { get; set; } = string.Empty;



        // Methods
        public static bool CanNextPlayground()
        {
            return PlaygroundNexts?.Length > 0;
        }
        public static IndexBase NextPlayground()
        {
            var idx = PlaygroundNexts[0];
            PlaygroundSlotNum++;
            PlaygroundNexts = PlaygroundNexts.Skip(1).ToArray();

            return idx;
        }
    }
}