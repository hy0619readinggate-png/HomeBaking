using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Tester
{
    public class TestError : MonoBehaviour
    {
        // Event Handlers
        private void stringFormatBTN_OnClick()
        {
            LOG.Function(this);

            var txt = string.Format("'{NickName}'는 지금 어디까지 배웠을까요?", "NickName");
            LOG.Info($"{txt}", this);
        }
        private void stringFormatAsyncBTN_OnClick()
        {
            LOG.Function(this);

            testUniTask().Forget();
        }
        private async UniTask testUniTask()
        {
            LOG.Function(this);

            await UniTask.Delay(1000);

            var txt = string.Format("'{NickName}'는 지금 어디까지 배웠을까요?", "NickName");
            LOG.Info($"{txt}", this);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button stringFormatBTN = null;
        [SerializeField] private Button stringFormatAsyncBTN = null;
        [SerializeField] private Button crashlyticsTesterBTN = null;
        [SerializeField] private GameObject crashlyticsTesterGO = null;

        // Unity Messages
        private void Awake()
        {
            crashlyticsTesterGO.SetActive(false);

            stringFormatBTN.onClick.AddListener(stringFormatBTN_OnClick);
            stringFormatAsyncBTN.onClick.AddListener(stringFormatAsyncBTN_OnClick);
            crashlyticsTesterBTN.onClick.AddListener(() => crashlyticsTesterGO.SetActive(!crashlyticsTesterGO.activeSelf));
        }
        private void Start()
        {
        }
    }
}