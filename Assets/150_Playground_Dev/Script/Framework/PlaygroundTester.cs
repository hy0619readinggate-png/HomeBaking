using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Playground.Framework
{
    public class PlaygroundTester : MonoBehaviour
    {
        // Fields : caching
        private PlaygroundRunner runner_;
        private PlaygroundRunner runner => runner_ ??= GetComponent<PlaygroundRunner>();



        // Unity Inspectors
        [Header("¡Ú Dev")]
        [SerializeField] private string userID = string.Empty;
        [SerializeField] private string userPassword = string.Empty;

        // Unity Messages
        private void Awake()
        {
            if (RunnerParam.ReturnScene == null)
            {
                SystemUI.One.Fader.FadeOutNow();

                LOG.Info($"Start PlaygroundTest", this);
                StartCoroutine(coRun());
            }
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coRun()
        {
            yield return LMS.One.SignInChild(userID, userPassword).ToCoroutine();
            yield return LMS.One.LoadDayProgress().ToCoroutine();
            yield return LMS.One.LoadCandy().ToCoroutine();
            yield return runner.Prepare().ToCoroutine();
            yield return SystemUI.One.Fader.FadeIn();

            runner.Run();
            yield return null;
        }
    }
}