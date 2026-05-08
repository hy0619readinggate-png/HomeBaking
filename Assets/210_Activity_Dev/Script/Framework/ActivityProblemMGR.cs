using DoDoEng.Common;

namespace DoDoEng.Activity.Framework
{
    public abstract class ActivityProblemMGR<TActivityData, TProblemData> :
        ProblemMGR<ActivityList, TActivityData, TProblemData>
    {
        // Properties
        public int BlanksCount { get; protected set; }



        // Properties : for concrete
        protected ActivityIndex ActIDX => IDX as ActivityIndex;
    }
}