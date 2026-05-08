using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.EBook.UI;
using System;
using UnityEngine.SceneManagement;

namespace DoDoEng.EBook.Framework
{
    public class EBookQuizRunner : SceneRunnerBase
    {
        // Fields : caching
        private EBookQuizBase eBookQuiz_;
        private EBookQuizBase eBookQuiz => eBookQuiz_ ??= GetComponent<EBookQuizBase>();

        // Functions
        private async UniTask<int?> saveLMS(EBookQuizResult ebookResult)
        {
            try
            {
                var launchedFrom = RunnerParam.LaunchedFrom;
                var learningIndex = RunnerParam.LearningIndex;
                //var learningTime = ebookResult.PlayingTime;

                RunnerParam.SelectedIDX.IsQuizDone = true;
                return await LMS.One.CompleteEBookQuiz(launchedFrom, learningIndex);
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
        private async void eBook_OnComplete(EBookQuizResult ebookResult)
        {
            LOG.Info($"eBook_OnComplete() | {ebookResult}", this);

            // LMS 완료처리
            var reward = await saveLMS(ebookResult);

            // 결과 팝업
            if (reward != null)
            {
                var ebReadIDX = RunnerParam.SelectedIDX as EBookQuizIndex;
                var isRead = ebReadIDX.IsRead;
                var isRecorded = ebReadIDX.IsRecorded;
                var isQuizDone = true;
                var result = await UIEBookCommon.One.CompleteQuizPopup.ShowPopup(ebookResult, reward.Value, isRead, isRecorded, isQuizDone);

                var finishAction = result switch
                {
                    EBookPopupResult.Back => FinishAction.Return,
                    EBookPopupResult.Retry => FinishAction.EBook_Record,
                    EBookPopupResult.Read => FinishAction.EBook_Read,
                    EBookPopupResult.Record => FinishAction.EBook_Record,
                    EBookPopupResult.Quiz => FinishAction.EBook_Quiz,
                    EBookPopupResult.MyEBook => FinishAction.EBook_MyEBook,
                    _ => FinishAction.Return
                };
                finish(finishAction);
            }
            else finish(FinishAction.Return);
        }
        private async void SystemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Back:
                    var result = await UIEBookCommon.One.ConfirmExitPopup.ShowPopup();
                    if (result == SimplePopupResult.Yes)
                    {
                        finish();
                    }

                    break;
            }
        }

        // Overrides
        protected override async UniTask onPrepare()
        {
            LOG.Assert(RunnerParam.SelectedIDX is EBookSingleIndex, "SelectedIDX must be EBookSingleIndex", this);

            await eBookQuiz.Prepare(RunnerParam.SelectedIDX as EBookQuizIndex);
            eBookQuiz.OnEBookQuizComplete += eBook_OnComplete;
        }
        protected override void onRun()
        {
            eBookQuiz.StartEBookQuiz();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            eBookQuiz.OnEBookQuizComplete -= eBook_OnComplete;
            eBookQuiz.FinishEBookQuiz();

            LOG.Important($"Finish with [{finishAction}]", this);
            switch (finishAction)
            {
                case FinishAction.Return:
                    LOG.Important($"RunnerParam.ReturnScene | {RunnerParam.ReturnScene}", this);
                    if (!string.IsNullOrEmpty(RunnerParam.ReturnScene))
                        SceneLoader.One.LoadScene(RunnerParam.ReturnScene);
                    else LOG.Error($"No ReturnScene", this);
                    break;

                case FinishAction.EBook_Read:
                    {
                        var currentIDX = RunnerParam.SelectedIDX as EBookSingleIndex;
                        var nextIDX = new EBookReadIndex(currentIDX.Index, EBookReadMode.Native, currentIDX.IsComplete, currentIDX.IsRead, currentIDX.IsRecorded, currentIDX.IsQuizDone);

                        RunnerParam.SelectedIDX = nextIDX;
                        SceneLoader.One.LoadScene(nextIDX.SceneName);
                    }
                    break;

                case FinishAction.EBook_Record:
                    {
                        var currentIDX = RunnerParam.SelectedIDX as EBookSingleIndex;
                        var nextIDX = new EBookRecordIndex(currentIDX.Index, currentIDX.IsComplete, currentIDX.IsRead, currentIDX.IsRecorded, currentIDX.IsQuizDone);

                        RunnerParam.SelectedIDX = nextIDX;
                        SceneLoader.One.LoadScene(nextIDX.SceneName);
                    }
                    break;

                case FinishAction.EBook_MyEBook:
                    {
                        var currentIDX = RunnerParam.SelectedIDX as EBookSingleIndex;
                        var nextIDX = new EBookReadIndex(currentIDX.Index, EBookReadMode.MyVoice, currentIDX.IsComplete, currentIDX.IsRead, currentIDX.IsRecorded, currentIDX.IsQuizDone);

                        RunnerParam.SelectedIDX = nextIDX;
                        SceneLoader.One.LoadScene(nextIDX.SceneName);
                    }
                    break;

                case FinishAction.EBook_Quiz:
                    {
                        var currentIDX = RunnerParam.SelectedIDX as EBookSingleIndex;
                        var nextIDX = new EBookQuizIndex(currentIDX.Index, EBookReadMode.Native, currentIDX.IsComplete, currentIDX.IsRead, currentIDX.IsRecorded, currentIDX.IsQuizDone);

                        RunnerParam.SelectedIDX = nextIDX;
                        SceneLoader.One.LoadScene(nextIDX.SceneName);
                    }
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