using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Library.Framework
{
    public class LibraryTester : MonoBehaviour
    {
        // Fields : caching
        private LibraryRunner runner_;
        private LibraryRunner runner => runner_ ??= GetComponent<LibraryRunner>();



        // Unity Inspectors
        [Header("¡Ú Config")]
        [SerializeField] private int course = 1;
        [Header("¡Ú Dev")]
        [SerializeField] private string userID = string.Empty;
        [SerializeField] private string userPassword = string.Empty;

        // Unity Messages
        private void Awake()
        {
            if (RunnerParam.ReturnScene == null)
            {
                SystemUI.One.Fader.FadeOutNow();

                LOG.Info($"Start LibraryTest", this);

                UserData.One.Child.Course = course;
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
            yield return runner.Prepare().ToCoroutine();
            yield return SystemUI.One.Fader.FadeIn();

            runner.Run();
            yield return null;
        }
    }
}