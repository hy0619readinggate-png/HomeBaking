using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(Text))]
    public class VersionDisp : MonoBehaviour
    {
        // Fields : caching
        private Text txt_ = null;
        private Text txt => txt_ ??= GetComponent<Text>();



        // Unity Messages
        private void Awake()
        {
            gameObject.SetActive(false);

            if (Debug.isDebugBuild)
            {
                gameObject.SetActive(true);
                StartCoroutine(coLoadVersion());
            }
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coLoadVersion()
        {
#if UNITY_EDITOR
            var url = $"file://{Application.streamingAssetsPath}/BuildNumber.txt";

#elif UNITY_IOS
            var url = $"file://{Application.dataPath}/Raw/BuildNumber.txt";
#else
            var url = $"{Application.streamingAssetsPath}/BuildNumber.txt";
#endif
            var www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                var buildVersion = www.downloadHandler.text;
                txt.text = $"v{Application.version}-{buildVersion}";
            }
            else
            {
                LOG.Error($"error | {www.error}", this);

                txt.text = $"v{Application.version}-(error)";
            }

            yield return null;
        }
    }
}