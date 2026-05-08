using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Playground.UI;
using UnityEngine.SceneManagement;

namespace DoDoEng.Playground.Framework
{
    public class PlaygroundRunner : SceneRunnerBase
    {
        // Properties
        public Playground PlayGround => playground;



        // Fields : caching
        private Playground playground_;
        private Playground playground => playground_ ??= GetComponent<Playground>();

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
            await playground.Prepare();
            await UniTask.Delay(1);
        }
        protected override void onRun()
        {
            playground.StartPlayground();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            playground.FinishPlayground();

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