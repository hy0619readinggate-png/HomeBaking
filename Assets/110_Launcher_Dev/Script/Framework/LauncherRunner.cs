using Cysharp.Threading.Tasks;
using DoDoEng.Common;

namespace DoDoEng.Launcher.Framework
{
    public class LauncherRunner : SceneRunnerBase
    {
        // Fields : caching
        private Launcher launcher_;
        private Launcher launcher => launcher_ ??= GetComponent<Launcher>();

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
            await launcher.Prepare();
            await UniTask.Delay(1);
        }
        protected override void onRun()
        {
            launcher.StartLauncher();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            launcher.FinishLauncher();

            // TODO: ¾Û Á¾·á
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