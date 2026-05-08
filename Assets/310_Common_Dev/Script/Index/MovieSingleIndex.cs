
namespace DoDoEng.Common
{
    public class MovieSingleIndex : IndexBase
    {
        // Properties
        public string Index { get; private set; }

        // Properties
        public int MainCategory { get; private set; }
        public int SubCategory { get; private set; }
        public int MovieNum { get; private set; }

        // Methods : ctor.
        public MovieSingleIndex(
            int mainCategory, int subCategory, int num, 
            bool isComplete = false, bool isRead = false, bool isRecorded = false)
        {
            MainCategory = mainCategory;
            SubCategory = subCategory;
            MovieNum = num;
            IsComplete = isComplete;
            IsRead = isRead;
            IsRecorded = isRecorded;

            // ABBCCDDD
            // A : 학습구분 (4:영상)
            // B : 메인 카테고리 
            // C : 서브 카테고리
            // D : 콘텐츠
            Index = $"4{MainCategory:d02}{SubCategory:d02}{MovieNum:d03}";
        }
        public MovieSingleIndex(
            string index,
            bool isComplete = false, bool isRead = false, bool isRecorded = false)
        {
            index = index.Trim();

            LOG.Assert(index.Length == 8, "index string length must be 8", this);
            LOG.Assert(index[0] == '4', "MovieIndex[0] must be 4", this);

            MainCategory = int.Parse(index.Substring(1, 2));
            SubCategory = int.Parse(index.Substring(3, 2));
            MovieNum = int.Parse(index.Substring(5, 3));
            IsComplete = isComplete;
            IsRead = isRead;
            IsRecorded = isRecorded;

            Index = index;
        }



        // Overrides
        protected override string onGetSceneName()
        {
            return $"Movie";
        }
        protected override string onGetAddressablePath()
        {
            return $"{Index}";
        }
        protected override string onGetThumbnailPath()
        {
            return $"Movie/Thumbnail/Category{MainCategory}/{Index}.png";
        }
        protected override string onGetDownloadDataPath()
        {
            return $"{AddressablePath}/{Index}.xlsx";
        }
        public override string ToString()
        {
            return $"<b><color=white>MovieIDX ({MainCategory}, {SubCategory}, {MovieNum} | {Index} | {IsComplete} | {IsRead} | {IsRecorded})</color></b>";
        }
    }
}