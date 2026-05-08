using DoDoEng.Common;

namespace DoDoEng
{
    public abstract class IndexBase
    {
        // Properties
        public string SceneName => onGetSceneName();
        public string AddressablePath => onGetAddressablePath();
        public string ThumbnailPath => onGetThumbnailPath();
        public string DownloadDataPath => onGetDownloadDataPath();
        public string HowToVideoPath => onGetHowToVideoPath();
        public bool IsComplete { get; set; }
        public bool IsRead { get; set; }
        public bool IsRecorded { get; set; }
        public bool IsQuizDone { get; set; }

        // Methods
        public static IndexBase FromIndex(
            int contentIndex, int curriculumId = 0,
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false)
        {
            var index = contentIndex.ToString();
            LOG.Assert<IndexBase>(index.Length == 8, "index string length must be 8");

            var study = index.Substring(0, 1);
            return study switch
            {
                "1" => new ActivityIndex(index),
                "4" => new MovieSingleIndex(index, isComplete, isRead, isRecorded),
                "5" => new GameIndex(index),
                "6" => new GameIndex(index, curriculumId),
                "7" => new AIStudioIndex(index),
                _ => null
            };
        }



        // Virtual
        protected abstract string onGetSceneName();
        protected abstract string onGetAddressablePath();
        protected abstract string onGetThumbnailPath();
        protected abstract string onGetDownloadDataPath();
        protected virtual string onGetHowToVideoPath() { return string.Empty; }
    }
}