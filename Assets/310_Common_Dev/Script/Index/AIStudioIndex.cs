// using DoDoEng.AIStudio;

namespace DoDoEng.Common
{
    public class AIStudioIndex : IndexBase
    {
        // Properties
        public string Index => index;
        public int MainCategory => mainCategory;
        public int SubCategory => subCategory;
        public int AIStudioNum => listNum;
        public string Title;

        const char A = '7';                       // A : 학습구분 (7:AIStudio)
        // Methods : ctor.
        public AIStudioIndex(int mainCategory, int subCategory, int num)
        {
            this.mainCategory = mainCategory;
            this.subCategory = subCategory;
            this.listNum = num;

            string BB = $"{MainCategory:d02}";  // B : 메인 카테고리 
            string CC = $"{SubCategory:d02}";   // C : 서브 카테고리
            string DDD = $"{AIStudioNum:d03}";  // D : 콘텐츠
            this.index = $"{A}{BB}{CC}{DDD}";   // ABBCCDDD
        }
        public AIStudioIndex(string index)
        {
            index = index.Trim();

            LOG.Assert(index.Length == 8, "index string length must be 8", this);
            LOG.Assert(index[0] == A, $"AIStudioIndex[0] must be {A}", this);

            this.index = index;
            this.mainCategory = int.Parse(index.Substring(1, 2));
            this.subCategory = int.Parse(index.Substring(3, 2));
            this.listNum = int.Parse(index.Substring(5, 3));
        }
        // public AIStudioIndex(AIStudioSlot slot)
        // {
        //     index = slot.Index;
        //     mainCategory = int.Parse(index.Substring(1, 2));
        //     subCategory = int.Parse(index.Substring(3, 2));
        //     listNum = int.Parse(index.Substring(5, 3));
        //     Title = slot.Title;
        // }

        // Fields
        string index;
        int mainCategory;
        int subCategory;
        int listNum;

        // Overrides
        protected override string onGetSceneName()
        {
            return $"AiStudio";
        }
        protected override string onGetAddressablePath()
        {
            return string.Empty;
        }
        protected override string onGetThumbnailPath()
        {
            return $"AIStudio/Thumbnail/{Index}.png";
        }
        protected override string onGetDownloadDataPath()
        {
            return string.Empty;
        }
        public override string ToString()
        {
            return $"<b><color=white>AIStudioIDX ({MainCategory}, {SubCategory}, {AIStudioNum} | {Index})</color></b>";
        }
    }
}