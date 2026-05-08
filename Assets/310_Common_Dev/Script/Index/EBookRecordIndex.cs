namespace DoDoEng.Common
{
    public class EBookRecordIndex : EBookSingleIndex
    {
        // Methods : ctor.
        public EBookRecordIndex(
            int mainCategory, int subCategory, int num,
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false) : 
            base(mainCategory, subCategory, num, isComplete, isRead, isRecorded, isQuizDone)
        {
        }
        public EBookRecordIndex(
            string index,
            bool isComplete = false, bool isRead = false, bool isRecorded = false, bool isQuizDone = false) : 
            base(index, isComplete, isRead, isRecorded, isQuizDone)
        {
        }



        // Overrides
        protected override string onGetSceneName()
        {
            return $"EBook_Record";
        }
        public override string ToString()
        {
            return $"<b><color=white>EBookRecordIndex ({MainCategory}, {SubCategory}, {EBookNum} | {Index} | {IsComplete} | {IsRead} | {IsRecorded} | {IsQuizDone})</color></b>";
        }
    }
}