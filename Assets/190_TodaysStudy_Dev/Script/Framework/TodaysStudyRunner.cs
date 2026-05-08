using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using UnityEngine.SceneManagement;

namespace DoDoEng.TodaysStudy.Framework
{
    public class TodaysStudyRunner : SceneRunnerBase
    {
        // Fields : caching
        private TodaysStudy todaysStudy_;
        private TodaysStudy todaysStudy => todaysStudy_ ??= GetComponent<TodaysStudy>();

        // Event Handlers
        private void SystemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Back:
                    finish();
                    break;
            }
        }

        // Overrides
        protected override async UniTask onPrepare()
        {
            await todaysStudy.Prepare();
            await UniTask.Delay(1);
        }
        protected override void onRun()
        {
            todaysStudy.StartTodaysStudy();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            LOG.Function(this, $"| finishAction={finishAction}");
            todaysStudy.FinishTodaysStudy();

            RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
            SceneLoader.One.LoadScene("Launcher");
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