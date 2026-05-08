using beyondi.Util;
using FlexFramework.Excel;

namespace DoDoEng.Common
{
    public sealed class ActivityIDConverter : CustomConverter<ActivityID>
    {
        public override ActivityID Convert(string input)
        {
            return UtilEnum.FromString<ActivityID>(input);
        }
    }

    public sealed class GameIDConverter : CustomConverter<GameID>
    {
        public override GameID Convert(string input)
        {
            return UtilEnum.FromString<GameID>(input);
        }
    }

}