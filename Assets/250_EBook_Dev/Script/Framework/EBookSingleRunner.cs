using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.EBook.UI;
using System;
using UnityEngine.SceneManagement;

namespace DoDoEng.EBook.Framework
{
    public class EBookSingleRunner : SceneRunnerBase
    {
        // Fields : caching
        private EBookSingleBase eBook_;
        private EBookSingleBase eBook => eBook_ ??= GetComponent<EBookSingleBase>();

        // Functions
        private async UniTask<int?> saveLMS(EBookSingleResult ebookResult)
        {
            try
            {
                var launchedFrom = RunnerParam.LaunchedFrom;
                var learningIndex = RunnerParam.LearningIndex;
                var learningTime = ebookResult.PlayingTime;

                if (RunnerParam.SelectedIDX is EBookReadIndex)
                {
                    var ebReadIDX = RunnerParam.SelectedIDX as EBookReadIndex;
                    if (ebReadIDX.EBookMode == EBookReadMode.Native)
                    {
                        RunnerParam.SelectedIDX.IsRead = true;
                        return await LMS.One.CompleteEBookRead(launchedFrom, learningIndex, learningTime);
                    }
                    else
                        return 0;
                }
                else // EBookRecordIndex
                {
                    RunnerParam.SelectedIDX.IsRecorded = true;
                    return await LMS.One.CompleteEBookRecord(launchedFrom, learningIndex, learningTime);
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
        private async void eBook_OnComplete(EBookSingleResult ebookResult)
        {
            LOG.Info($"eBook_OnComplete() | {ebookResult}", this);

            // LMS 완료처리
            if (!UserData.One.IsReportContents)
            {
                var reward = await saveLMS(ebookResult);
                if (reward != null)
                {
                    var thumbnail = await RunnerParam.SelectedIDX.LoadThumbnail();
                    var ebReadIDX = RunnerParam.SelectedIDX as EBookReadIndex;
                    var isRead = RunnerParam.SelectedIDX.IsRead;
                    var isRecorded = RunnerParam.SelectedIDX.IsRecorded;
                    var isQuizDone = RunnerParam.SelectedIDX.IsQuizDone;
                    if (ebReadIDX != null && ebReadIDX.EBookMode == EBookReadMode.Native)
                    {
                        var result = await UIEBookCommon.One.CompleteReadPopup.ShowPopup(thumbnail, reward.Value, isRead, isRecorded, isQuizDone);
                        var finishAction = result switch
                        {
                            EBookPopupResult.Back => FinishAction.Return,
                            EBookPopupResult.Retry => FinishAction.Retry,
                            EBookPopupResult.Read => FinishAction.EBook_Read,
                            EBookPopupResult.Record => FinishAction.EBook_Record,
                            EBookPopupResult.Quiz => FinishAction.EBook_Quiz,
                            EBookPopupResult.MyEBook => FinishAction.EBook_MyEBook,
                            _ => FinishAction.Return
                        };
                        finish(finishAction);
                    }
                    else // EBRecord이거나, MyVoice인 경우
                    {
                        SystemUI.One.StandbyPU.HideNow(); // #2106 저장 대기 화면을 끔 

                        var result = await UIEBookCommon.One.CompleteRecordPopup.ShowPopup(thumbnail, reward.Value, isRead, isRecorded, isQuizDone);
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
                }
                else finish(FinishAction.Return);
            }
            else finish(FinishAction.Return);
        }
        private void eBook_OnError() // 마이크 권한이 허용되지 않은 경우만 호출되고 있음
        {
            LOG.Info($"eBook_OnError()", this);

            finish(FinishAction.Return);
        }
        private async void SystemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Back:
                    if (UserData.One.IsReportContents)
                    {
                        finish();
                    }
                    else
                    {
                        var result = await UIEBookCommon.One.ConfirmExitPopup.ShowPopup();
                        if (result == SimplePopupResult.Yes)
                        {
                            finish();
                        }
                    }

                    break;
            }
        }

        // Overrides
        protected override async UniTask onPrepare()
        {
            LOG.Assert(RunnerParam.SelectedIDX is EBookSingleIndex, "SelectedIDX must be EBookIndex", this);

            await eBook.Prepare(RunnerParam.SelectedIDX as EBookSingleIndex);
            eBook.OnEBookComplete += eBook_OnComplete;
            eBook.OnEBookError += eBook_OnError;
        }
        protected override void onRun()
        {
            eBook.StartEBook();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            eBook.OnEBookComplete -= eBook_OnComplete;
            eBook.OnEBookError -= eBook_OnError;
            eBook.FinishEBook();

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