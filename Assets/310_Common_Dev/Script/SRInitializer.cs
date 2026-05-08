using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng
{
    [RequireComponent(typeof(Button))]
    public class SRInitializer : MonoBehaviour
    {
        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields 
        private bool isSRTriggerEnabled = false;
        private float resetTimer = 0;
        private int clickCount = 0;

        // Event Handlers
        private void onClick()
        {
            clickCount++;
            resetTimer = 1f; // 1초

            if (clickCount > 5)
            {
                btn.interactable = false;

                isSRTriggerEnabled = true;
                SRDebug.Instance.IsTriggerEnabled = true;

                SystemUI.One.DeveloperModeMarker = true;
                SystemUI.One.ShowToastMessage($"개발자 모드가 활성화되었습니다.");
            }
        }



        // Unity Messages
        private void Awake()
        {
            btn.interactable = !SRDebug.Instance.IsTriggerEnabled;
            btn.onClick.AddListener(onClick);
        }
        private void Start()
        {
            StartCoroutine(coResetCount());
        }

        // Unity Coroutine
        IEnumerator coResetCount()
        {
            while (!isSRTriggerEnabled)
            {
                if (clickCount > 0)
                {
                    if (resetTimer > 0)
                        resetTimer -= Time.deltaTime;
                    else clickCount = 0;
                }

                yield return null;
            }
        }
    }
}