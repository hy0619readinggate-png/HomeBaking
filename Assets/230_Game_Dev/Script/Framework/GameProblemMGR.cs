using DoDoEng.Common;

namespace DoDoEng.Game.Framework
{
    public abstract class GameProblemMGR<TGameData, TProblemData> :
        ProblemMGR<GameList, TGameData, TProblemData>
    {
        protected GameIndex GameIDX => IDX as GameIndex;
    }
}