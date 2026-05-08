using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections.Generic;
using UnityEngine;

using GameData = DoDoEng.Common.GameData_C1_G00;

// devBOX - 삭제요망
#pragma warning disable 0414

namespace DoDoEng.Game.C1_G00
{
    public class C1_G00_ProblemMGR : GameProblemMGR<GameData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                await UniTask.Delay(100);

                return null;
            }
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO config = null;
    }

    public class ProblemData
    {
        public int Round;
    }
}