using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.EBook.UI;
using UnityEngine.SceneManagement;

namespace DoDoEng.EBook.Framework
{
    public class EBookPlayAllRunner : SceneRunnerBase
    {
        // Fields : caching
        private EBookPlayAllBase ebook_;
        private EBookPlayAllBase eBook => ebook_ ??= GetComponent<EBookPlayAllBase>();

        // Event Handlers
        private void eBook_OnComplete()
        {
            LOG.Info($"eBook_OnComplete()", this);

            finish(FinishAction.Return);
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
            LOG.Assert(RunnerParam.SelectedIDX is EBookPlayAllIndex, "SelectedIDX must be EBookPlayAllIndex", this);

            AutoQuitMGR.One.PauseMonitor();

            await eBook.Prepare(RunnerParam.SelectedIDX as EBookPlayAllIndex);
            eBook.OnEBookComplete += eBook_OnComplete;
        }
        protected override void onRun()
        {
            eBook.StartEBook();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            AutoQuitMGR.One.ResumeMonitor();

            eBook.OnEBookComplete -= eBook_OnComplete;
            eBook.FinishEBook();

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