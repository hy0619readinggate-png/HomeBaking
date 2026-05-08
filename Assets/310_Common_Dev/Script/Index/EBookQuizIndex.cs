namespace DoDoEng.Common
{
    public class EBookQuizIndex : EBookSingleIndex
    {
        // Methods : ctor.
        public EBookQuizIndex(
            int mainCategory, int subCategory, int num, 
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false) : 
            base(mainCategory, subCategory, num, isComplete, isRead, isRecorded, isQuizDone)
        {
        }
        public EBookQuizIndex(
            string index, EBookReadMode mode = EBookReadMode.Native,
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false) : 
            base(index, isComplete, isRead, isRecorded, isQuizDone)
        {
        }



        // Overrides
        protected override string onGetSceneName()
        {
            return $"EBook_Quiz";
        }
        public override string ToString()
        {
            return $"<b><color=white>EBookQuizIndex ({MainCategory}, {SubCategory}, {EBookNum} | {Index} | {IsComplete} | {IsRead} | {IsRecorded} | {IsQuizDone})</color></b>";
        }
    }
}