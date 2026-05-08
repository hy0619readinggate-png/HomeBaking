using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using UnityEngine.SceneManagement;

namespace DoDoEng.MyRoom.Framework
{
    public class MyRoomRunner : SceneRunnerBase
    {
        // Fields : caching
        private MyRoom myRoom_;
        private MyRoom myRoom => myRoom_ ??= GetComponent<MyRoom>();

        // Event Handlers
        private void SystemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Back:
                    if (myRoom.Back())
                        finish();
                    break;
            }
        }

        // Overrides
        protected override async UniTask onPrepare()
        {
            await myRoom.Prepare();
            await UniTask.Delay(1);
        }
        protected override void onRun()
        {
            myRoom.StartMyRoom();
        }
        protected override void onFinish(FinishAction finishAction)
        {
            myRoom.FinishMyRoom();

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