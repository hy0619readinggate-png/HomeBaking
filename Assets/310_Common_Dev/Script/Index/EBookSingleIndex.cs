
namespace DoDoEng.Common
{
    public abstract class EBookSingleIndex : IndexBase
    {
        // Properties
        public string Index => index;


        // Properties
        public int MainCategory => mainCategory;
        public int SubCategory => subCategory;
        public int EBookNum => ebookNum;

        // Methods : ctor.
        public EBookSingleIndex(
            int mainCategory, int subCategory, int num,
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false)
        {
            this.mainCategory = mainCategory;
            this.subCategory = subCategory;
            ebookNum = num;
            IsComplete = isComplete;
            IsRead = isRead;
            IsRecorded = isRecorded;
            IsQuizDone = isQuizDone;

            // ABBCCDDD
            // A : 학습구분 (2:이북)
            // B : 메인 카테고리 
            // C : 서브 카테고리
            // D : 콘텐츠
            index = $"2{MainCategory:d02}{SubCategory:d02}{EBookNum:d03}";
        }
        public EBookSingleIndex(
            string index,
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false)
        {
            index = index.Trim();

            LOG.Assert(index.Length == 8, "index string length must be 8", this);
            LOG.Assert(index[0] == '2', "EBookIndex[0] must be 2", this);

            this.index = index;

            mainCategory = int.Parse(index.Substring(1, 2));
            subCategory = int.Parse(index.Substring(3, 2));
            ebookNum = int.Parse(index.Substring(5, 3));
            IsComplete = isComplete;
            IsRead = isRead;
            IsRecorded = isRecorded;
            IsQuizDone = isQuizDone;
        }



        // Fields
        private string index;
        private int mainCategory;
        private int subCategory;
        private int ebookNum;


        // Overrides
        protected override string onGetAddressablePath()
        {
            return $"{Index}";
        }
        protected override string onGetThumbnailPath()
        {
            return $"eBook/Thumbnail/Category{MainCategory}/{Index}.png";
        }
        protected override string onGetDownloadDataPath()
        {
            return $"{AddressablePath}/{Index}_pages.xlsx";
        }
        public override string ToString()
        {
            return $"<b><color=white>EBookIDX ({MainCategory}, {SubCategory}, {EBookNum} | {Index} | {IsComplete} | {IsRead} | {IsRecorded} | {IsQuizDone})</color></b>";
        }
    }
}