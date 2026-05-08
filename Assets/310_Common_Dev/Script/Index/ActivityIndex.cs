
namespace DoDoEng.Common
{
    public class ActivityIndex : IndexBase
    {
        // Properties
        public ActivityID ActivityID => activityID;
        public int ActivityNum => activityNum;

        // Properties
        public string Index => index;
        public int Course => (int)ActivityID / 100;
        public int ActivityType => (int)ActivityID % 100;

        // Methods : ctor.
        public ActivityIndex(ActivityID id, int num)
        {
            this.activityID = id;
            this.activityNum = num;

            // ABCDDEEE
            // A : 학습구분 (1:액티비티)
            // B : 코스 
            // C : 분류 (1:list)
            // D : 타입
            // E : 콘텐츠
            this.index = $"1{Course:d1}1{ActivityType:d02}{ActivityNum:d03}";
        }
        public ActivityIndex(string index)
        {
            index = index.Trim();

            LOG.Assert(index.Length == 8, "index string length must be 8", this);
            LOG.Assert(index[0] == '1', "ActivityIndex[0] must be 1", this);
            LOG.Assert(index[2] == '1', "ActivityIndex[2] must be 1", this);

            this.index = index;

            var activityIDstring = index.Substring(1, 1) + index.Substring(3, 2);
            this.activityID = (ActivityID)int.Parse(activityIDstring);
            this.activityNum = int.Parse(index.Substring(5, 3));
        }



        // Fields
        private string index;
        private ActivityID activityID;
        private int activityNum;

        // Overrides
        protected override string onGetSceneName()
        {
            return $"{ActivityID}";
        }
        protected override string onGetAddressablePath()
        {
            return activityID.ToString().Substring(0, 6);
        }
        protected override string onGetThumbnailPath()
        {
            return $"Activity/Thumbnail/Course{Course}/{Index}.png";
        }
        protected override string onGetDownloadDataPath()
        {
            return $"{AddressablePath}/DataDefinitionSO.asset";
        }
        public override string ToString()
        {
            return $"<b><color=white>ActIDX ({ActivityID},{ActivityNum:d03} | {Index})</color></b>";
        }
    }
}