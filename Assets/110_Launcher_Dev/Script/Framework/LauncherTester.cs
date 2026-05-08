using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Launcher.Framework
{
    public class LauncherTester : MonoBehaviour
    {
        // Fields : caching
        private LauncherRunner runner_;
        private LauncherRunner runner => runner_ ??= GetComponent<LauncherRunner>();



        // Unity Messages
        private void Awake()
        {
            if (RunnerParam.ReturnScene == null)
            {
                SystemUI.One.Fader.FadeOutNow();

                LOG.Info($"Start LauncherTest", this);
                StartCoroutine(coRun());
            }
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coRun()
        {
            yield return runner.Prepare().ToCoroutine();
            yield return SystemUI.One.Fader.FadeIn();

            runner.Run();
            yield return null;
        }
    }
}