using Cysharp.Threading.Tasks;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System;
using UnityEngine.SceneManagement;

namespace DoDoEng.Activity.Framework
{
    public class ActivityRunner : SceneRunnerBase
    {
        // Properties
        public ActivityBase Activity => activity;



        // Fields : caching
        private ActivityBase activity_;
        private ActivityBase activity => activity_ ??= GetComponent<ActivityBase>();

        // Functions
        private async UniTask<int?> saveLMS(ActivityResult activityResult)
        {
            try
            {
                var launchedFrom = RunnerParam.LaunchedFrom;
                var learningIndex = RunnerParam.LearningIndex;
                var learningTime = activityResult.PlayingTime;
                var wrongRate = activityResult.WrongRate;
                return await LMS.One.CompleteActivity(launchedFrom, learningIndex, learningTime, wrongRate);
            }
            catch (Exception ex)
            {
                LOG.Info($"{ex.Message}", this);

                SystemUI.One.ShowPopupProblem();

                return null;
            }
        }

        // Event Handlers
        private async void activity_OnComplete(ActivityResult activityResult)
        {
            LOG.Info($"activity_OnComplete() | {activityResult}", this);

            // LMS 완료처리
            var reward = await saveLMS(activityResult);
            if (reward != null)
            {
                // 결과 팝업
                var result = await UIActivityCommon.One.CompletePopup.ShowPopup(reward.Value);
                var finishAction = result switch
                {
                    ActivityPopupResult.Retry => FinishAction.Retry,
                    ActivityPopupResult.Back => FinishAction.Return,
                    _ => FinishAction.Return
                };
                finish(finishAction);
            }
            else finish(FinishAction.Return);
        }
        private void activity_OnError() // 마이크 권한이 허용되지 않은 경우만 호출되고 있음
        {
            LOG.Info($"activity_OnError()", this);

            finish(FinishAction.Return);
        }
        private async void SystemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Back:
                    var result = await UIActivityCommon.One.ConfirmExitPopup.ShowPopup();
                    if (result == SimplePopupResult.Yes)
                    {
                        SystemManager.Pause();
                        finish();
                    }
                    break;

                case SystemButtonType.Pause:
                    UIActivityCommon.One.VisiblePauseButton = false;
                    await UIActivityCommon.One.PausePopup.ShowPopup();
                    break;

                case SystemButtonType.Resume:
                    UIActivityCommon.One.PausePopup.CloseWithResult();
                    UIActivityCommon.One.VisiblePauseButton = true;
                    break;
            }
        }

        // Overrides
        protected override async UniTask onPrepare()
        {
            LOG.Assert(RunnerParam.SelectedIDX is ActivityIndex, "SelectedIDX must be ActivityIndex", this);

            await activity.Prepare(RunnerParam.SelectedIDX as ActivityIndex);
            activity.OnActivityComplete += activity_OnComplete;
            activity.OnActivityError += activity_OnError;

        }
        protected override void onRun()
        {
            activity.StartActivity();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            activity.OnActivityComplete -= activity_OnComplete;
            activity.OnActivityError -= activity_OnError;
            activity.FinishActivity();

            switch (finishAction)
            {
                case FinishAction.Next:
                    LOG.VeryImportant($"Next must be implemented.", this);
                    goto case FinishAction.Return;

                case FinishAction.Return:
                    LOG.Important($"RunnerParam.ReturnScene | {RunnerParam.ReturnScene}", this);
                    if (!string.IsNullOrEmpty(RunnerParam.ReturnScene))
                        SceneLoader.One.LoadScene(RunnerParam.ReturnScene);
                    break;

                case FinishAction.Retry:
                    LOG.Important($"Finish with Retry", this);
                    SceneLoader.One.LoadScene(SceneManager.GetActiveScene().name);
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