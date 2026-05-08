using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.TodaysStudy.Framework
{
    public class TodaysStudyTester : MonoBehaviour
    {
        // Fields : caching
        private TodaysStudyRunner runner_;
        private TodaysStudyRunner runner => runner_ ??= GetComponent<TodaysStudyRunner>();



        // Unity Inspectors
        [Header("¡Ú Config")]
        [SerializeField] private int stage = 1;
        [SerializeField] private int day = 1;
        [SerializeField] private int order = 2;
        [Header("¡Ú Dev")]
        [SerializeField] private string userID = string.Empty;
        [SerializeField] private string userPassword = string.Empty;

        // Unity Messages
        private void Awake()
        {
            if (RunnerParam.ReturnScene == null)
            {
                SystemUI.One.Fader.FadeOutNow();

                LOG.Info($"Start TodaysStudyTest", this);

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
            yield return LMS.One.LoadDayProgress();
            yield return LMS.One.LoadSettingsChild();
            RunnerParam.TodaysStage = stage > 0 ? stage : UserData.One.Child.DayProgress.Value<int>("stageId");
            RunnerParam.TodaysDay = day > 0 ? day : UserData.One.Child.DayProgress.Value<int>("day");
            RunnerParam.TodaysOrder = order > 0 ? order : 1;
            yield return runner.Prepare().ToCoroutine();
            yield return SystemUI.One.Fader.FadeIn();

            runner.Run();
            yield return null;
        }
    }
}