using System.Linq;

namespace DoDoEng.Common
{
    public class EBookPlayAllIndex : IndexBase
    {
        // Properties
        public int Count => ebooks.Length;
        public EBookSingleIndex[] EBooks => ebooks;
        public EBookReadMode EBookMode => eBookMode;

        // Methods : ctor.
        public EBookPlayAllIndex(string[] ebCodes, EBookReadMode mode = EBookReadMode.Native)
        {
            this.eBookMode = mode;
            ebooks = ebCodes.Select(idx => new EBookReadIndex(idx, mode))
                              .ToArray();
        }

        // Methods 
        public EBookSingleIndex GetEBookIndex(int i) => ebooks[i];



        // Fields
        private EBookSingleIndex[] ebooks = null;
        private EBookReadMode eBookMode;

        // Overrides
        protected override string onGetSceneName()
        {
            return $"EBook_PlayAll";
        }
        protected override string onGetAddressablePath()
        {
            return string.Empty;
        }
        protected override string onGetThumbnailPath()
        {
            return string.Empty;
        }
        protected override string onGetDownloadDataPath()
        {
            return string.Empty;
        }
        public override string ToString()
        {
            return $"<b><color=white>EBookPlayAllIndex (Count:{Count})</color></b>";
        }
    }
}