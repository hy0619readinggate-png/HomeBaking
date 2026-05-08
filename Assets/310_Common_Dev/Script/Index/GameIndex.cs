
using DoDoEng.Launcher;

namespace DoDoEng.Common
{
    public enum GameMode { Review = 5, Playground = 6 }

    public class GameIndex : IndexBase
    {
        // Properties
        public GameID GameID => gameID;
        public int GameNum => gameNum;
        public GameMode GameMode => gameMode;

        // Properties
        public string Index => index;
        public int Course => (int)GameID / 100;
        public int GameType => (int)GameID % 100;

        // Properties for LMS
        public int CurriculumId => curriculumId;

        // Methods : ctor.
        public GameIndex(GameID id, int num, GameMode mode = GameMode.Playground)
        {
            this.gameID = id;
            this.gameNum = num;
            this.gameMode = mode;

            // ABCDDEEE
            // A : 학습구분 (5:리뷰게임, 6:플레이그라운드)
            // B : 코스 
            // C : 분류 (1:list)
            // D : 타입
            // E : 콘텐츠
            this.index = $"{(int)GameMode:d1}{Course:d1}1{GameType:d02}{GameNum:d03}";
        }
        public GameIndex(string index, int curriculumId = 0)
        {
            index = index.Trim();

            LOG.Assert(index.Length == 8, "index string length must be 8", this);
            LOG.Assert(index[0] == '5' || index[0] == '6', "GameIndex[0] must be 5 or 6", this);
            LOG.Assert(index[2] == '1', "GameIndex[2] must be 1", this);

            this.index = index;
            this.curriculumId = curriculumId;

            var gameIDstring = index.Substring(1, 1) + index.Substring(3, 2);
            this.gameID = (GameID)int.Parse(gameIDstring);
            this.gameNum = int.Parse(index.Substring(5, 3));
            this.gameMode = (GameMode)int.Parse(index.Substring(0, 1));
        }



        // Fields
        private string index;
        private GameID gameID;
        private GameMode gameMode;
        private int gameNum;
        private int curriculumId = 0;

        // Overrides
        protected override string onGetSceneName()
        {
            return $"{GameID}";
        }
        protected override string onGetAddressablePath()
        {
            return gameID.ToString().Substring(0, 6);
        }
        protected override string onGetThumbnailPath()
        {
            return $"ReviewGame/Thumbnail/Course{Course}/{Index}.png";
        }
        protected override string onGetDownloadDataPath()
        {
            return $"{AddressablePath}/DataDefinitionSO.asset";
        }
        public override string ToString()
        {
            return $"<b><color=white>GameIDX ({GameID},{GameNum:d03},{GameMode} | {Index})</color></b>";
        }
        protected override string onGetHowToVideoPath()
        {
            return $"{API.One.MediaHost}/service/C{Course:d1}_G{GameType:d02}_HowTo.mp4";
        }
    }
}