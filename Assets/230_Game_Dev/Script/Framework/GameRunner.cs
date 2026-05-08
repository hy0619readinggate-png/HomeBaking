using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.UI;
using System;
using UnityEngine.SceneManagement;

namespace DoDoEng.Game.Framework
{
    public class GameRunner : SceneRunnerBase
    {
        // Properties
        public GameBase Game => game;



        // Fields : caching
        private GameBase game_;
        private GameBase game => game_ ??= GetComponent<GameBase>();

        // Functions
        private async UniTask<int?> saveLMS(GameResult gameResult)
        {
            try
            {
                var gameIDX = RunnerParam.SelectedIDX as GameIndex;
                if (gameIDX.GameMode == GameMode.Review)
                {
                    var learningIndex = RunnerParam.LearningIndex;
                    var learningTime = gameResult.PlayingTime;
                    return await LMS.One.CompleteReviewGame(learningIndex, learningTime);
                }
                else // GameMode.Playground
                {
                    var curriculumId = gameIDX.CurriculumId;
                    var learningTime = gameResult.PlayingTime;
                    var star = gameResult.EarnStar;
                    return await LMS.One.CompletePlaygroundGame(curriculumId, learningTime, star);
                }
            }
            catch (Exception ex)
            {
                LOG.Info($"{ex.Message}", this);

                // devBOX(swon) - 저장중 오류 팝업으로 변경해야함
                SystemUI.One.ShowPopupProblem();

                return null;
            }
        }

        // Event Handlers
        private async void game_OnComplete(GameResult gameResult)
        {
            LOG.Info($"game_OnComplete() | {gameResult}", this);

            // LMS 완료처리
            var reward = await saveLMS(gameResult);
            if (reward != null)
            {
                // 결과 팝업
                var gameIDX = RunnerParam.SelectedIDX as GameIndex;
                if (gameIDX.GameMode == GameMode.Playground)
                {
                    if (gameResult.EarnStar > 0)
                    {
                        var slot = RunnerParam.PlaygroundSlotNum;
                        var canNext = RunnerParam.CanNextPlayground();
                        var result = await UIGameCommon.One.PlaygroundSuccessPopup.ShowPopup(slot, gameResult, canNext, reward.Value);
                        var finishAction = result switch
                        {
                            GamePopupResult.Next => FinishAction.Next,
                            GamePopupResult.Retry => FinishAction.Retry,
                            GamePopupResult.Back => FinishAction.Return,
                            _ => FinishAction.Return
                        };
                        finish(finishAction);
                    }
                    else
                    {
                        var slot = RunnerParam.PlaygroundSlotNum;
                        var result = await UIGameCommon.One.PlaygroundFailPopup.ShowPopup(slot, gameResult, reward.Value);
                        var finishAction = result switch
                        {
                            GamePopupResult.Retry => FinishAction.Retry,
                            GamePopupResult.Back => FinishAction.Return,
                            _ => FinishAction.Return
                        };
                        finish(finishAction);
                    }
                }
                else
                {
                    var result = await UIGameCommon.One.ReviewCompletePU.ShowPopup(reward.Value);
                    var finishAction = result switch
                    {
                        GamePopupResult.Retry => FinishAction.Retry,
                        GamePopupResult.Back => FinishAction.Return,
                        _ => FinishAction.Return
                    };
                    finish(finishAction);
                }
            }
            else finish(FinishAction.Return);
        }
        private async void SystemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Back:
                    var result = await UIGameCommon.One.ConfirmExitPopup.ShowPopup();
                    if (result == SimplePopupResult.Yes)
                        finish();
                    break;

                case SystemButtonType.Pause:
                    var idx = RunnerParam.SelectedIDX as GameIndex;
                    await UIGameCommon.One.PausePopup.ShowPopup(idx);
                    break;


                case SystemButtonType.Resume:
                    UIGameCommon.One.PausePopup.CloseWithResult();
                    break;
            }
        }

        // Overrides
        protected override async UniTask onPrepare()
        {
            LOG.Assert(RunnerParam.SelectedIDX is GameIndex, "SelectedIDX must be GameIndex", this);

            await game.Prepare(RunnerParam.SelectedIDX as GameIndex);
            game.OnGameComplete += game_OnComplete;
        }
        protected override void onRun()
        {
            game.StartGame();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            game.OnGameComplete -= game_OnComplete;
            game.FinishGame();

            LOG.Important($"Finish with [{finishAction}]", this);
            switch (finishAction)
            {
                case FinishAction.Return:
                    LOG.Important($"RunnerParam.ReturnScene | {RunnerParam.ReturnScene}", this);
                    if (!string.IsNullOrEmpty(RunnerParam.ReturnScene))
                        SceneLoader.One.LoadScene(RunnerParam.ReturnScene);
                    else LOG.Error($"No ReturnScene", this);
                    break;

                case FinishAction.Retry:
                    SceneLoader.One.LoadScene(SceneManager.GetActiveScene().name);
                    break;

                case FinishAction.Next:
                    if (RunnerParam.CanNextPlayground())
                    {
                        var nextIDX = RunnerParam.NextPlayground();
                        RunnerParam.SelectedIDX = nextIDX;
                        SceneLoader.One.LoadScene(nextIDX.SceneName);
                    }
                    else LOG.Error($"No Next Playground index", this);
                    break;

            }
        }



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {

        }
        private void OnEnable()
        {
            SystemEventManager.OnSystemButtonClicked += SystemEventManager_OnSystemButtonClicked;
        }
        private void OnDisable()
        {
            SystemEventManager.OnSystemButtonClicked -= SystemEventManager_OnSystemButtonClicked;
        }
    }
}