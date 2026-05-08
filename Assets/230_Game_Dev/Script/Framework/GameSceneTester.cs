using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using NaughtyAttributes;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace DoDoEng.Game.Framework
{
    public class GameSceneTester : MonoBehaviour
    {
        // Fields : caching
        private GameRunner runner_;
        private GameRunner runner => runner_ ??= GetComponent<GameRunner>();

        // Functions
        private string[] getOptions()
        {
            var stateType = runner?.Game?.GetStateType();
            if (stateType != null)
                return UtilEnum.GetValues(stateType);
            else return new string[] { string.Empty };
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private int gameNum = 1;
        [SerializeField] private GameMode gameMode = GameMode.Review;
        [Header("★ Dev")]
        [SerializeField] private bool enableSkip = false;
        [Dropdown(nameof(getOptions))]
        [SerializeField] private string skipStateTo = string.Empty;


        // Unity Messages
        private void Awake()
        {
            if (RunnerParam.SelectedIDX == null)
            {
                SystemUI.One.Fader.FadeOutNow();

                var game = GetComponent<GameBase>();

                var gameIDX = new GameIndex(game.GameID, gameNum, gameMode);

                RunnerParam.SelectedIDX = gameIDX;
                RunnerParam.SkipStateTo = enableSkip ? skipStateTo : string.Empty;

                LOG.Info($"Start SceneTest | {gameIDX}", this);
                StartCoroutine(coRunActivity());
            }
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coRunActivity()
        {
            yield return runner.Prepare().ToCoroutine();
            yield return SystemUI.One.Fader.FadeIn();

            runner.Run();
            yield return null;
        }
    }
}