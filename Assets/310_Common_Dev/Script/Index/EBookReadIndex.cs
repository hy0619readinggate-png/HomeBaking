namespace DoDoEng.Common
{
    public enum EBookReadMode { Native = 1, MyVoice = 2 }

    public class EBookReadIndex : EBookSingleIndex
    {
        // Properties
        public EBookReadMode EBookMode => eBookMode;

        // Methods : ctor.
        public EBookReadIndex(
            int mainCategory, int subCategory, int num, EBookReadMode mode = EBookReadMode.Native,
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false) : 
            base(mainCategory, subCategory, num, isComplete, isRead, isRecorded, isQuizDone)
        {
            this.eBookMode = mode;
        }
        public EBookReadIndex(
            string index, EBookReadMode mode = EBookReadMode.Native,
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false) : 
            base(index, isComplete, isRead, isRecorded, isQuizDone)
        {
            this.eBookMode = mode;
        }



        // Fields
        private EBookReadMode eBookMode;

        // Overrides
        protected override string onGetSceneName()
        {
            return $"EBook_Read";
        }
        public override string ToString()
        {
            return $"<b><color=white>EBookReadIndex ({MainCategory}, {SubCategory}, {EBookNum}, {eBookMode} | {Index} | {IsComplete} | {IsRead} | {IsRecorded} | {IsQuizDone})</color></b>";
        }
    }
}