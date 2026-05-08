using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using UnityEngine.SceneManagement;

namespace DoDoEng.Library.Framework
{
    public class LibraryRunner : SceneRunnerBase
    {
        // Properties
        public LibraryBase Library => library;



        // Fields : caching
        private LibraryBase library_;
        private LibraryBase library => library_ ??= GetComponent<LibraryBase>();

        // Event Handlers
        private void SystemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Back:
                    if (library.Back())
                        finish();
                    break;
            }
        }

        // Overrides
        protected override async UniTask onPrepare()
        {
            await library.Prepare();
            await UniTask.Delay(1);
        }
        protected override void onRun()
        {
            library.StartLibrary();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            library.FinishLibrary();

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