using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using NaughtyAttributes;
using System;

namespace DoDoEng.Activity.Framework
{
    public class ActivitySceneTester : MonoBehaviour
    {
        // Fields : caching
        private ActivityRunner runner_;
        private ActivityRunner runner => runner_ ??= GetComponent<ActivityRunner>();

        // Functions
        private string[] getOptions()
        {
            var stateType = runner?.Activity?.GetStateType();
            if (stateType != null)
                return UtilEnum.GetValues(stateType);
            else return new string[] { string.Empty };
        }



        // Unity Inspectors
        [Header("�� Config")]
        [SerializeField] private int activityNum = 1;
        [Header("�� Dev")]
        [SerializeField] private bool enableSkip = false;
        [Dropdown(nameof(getOptions))]
        [SerializeField] private string skipStateTo = string.Empty;

        // Unity Messages
        // Unity Messages
        private void Awake()
        {
            if (RunnerParam.SelectedIDX == null)
            {
                string url = Application.absoluteURL;
#if UNITY_EDITOR
                url = $"http://localhost:8000/?activityNum={activityNum}";
#endif
                Debug.Log("URL: " + url);
                if (!string.IsNullOrEmpty(url))
                {
                    var param = ParseQuery(url);

                    if (param.TryGetValue("activityNum", out var aN))
                    {
                        if (int.TryParse(aN, out var num))
                            activityNum = num;

                    }
                }
                Debug.Log("activityNum: " + activityNum);


                SystemUI.One.Fader.FadeOutNow();

                var act = GetComponent<ActivityBase>();
                var actIDX = new ActivityIndex(act.ActivityID, activityNum);
                RunnerParam.SelectedIDX = actIDX;
                RunnerParam.SkipStateTo = enableSkip ? skipStateTo : string.Empty;

                LOG.Info($"Start SceneTest | {actIDX}", this);
                StartCoroutine(coRunActivity());
            }
        }


        System.Collections.Generic.Dictionary<string, string> ParseQuery(string url)
        {
            var dict = new System.Collections.Generic.Dictionary<string, string>();

            var uri = new Uri(url);
            var query = uri.Query.TrimStart('?').Split('&');

            foreach (var pair in query)
            {
                if (string.IsNullOrEmpty(pair)) continue;

                var kv = pair.Split('=');
                if (kv.Length == 2)
                    dict[kv[0]] = Uri.UnescapeDataString(kv[1]);
            }

            return dict;
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coRunActivity()
        {
            yield return runner.Prepare().ToCoroutine();
            yield return SystemUI.One.Fader.FadeIn();

            runner.Run();
            yield return null;
        }
    }
}