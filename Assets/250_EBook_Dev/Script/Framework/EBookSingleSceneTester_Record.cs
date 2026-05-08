using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.EBook.Framework
{
    [RequireComponent(typeof(EBookSingleRunner))]
    public class EBookSingleSceneTester_Record : MonoBehaviour
    {
        // Fields : caching
        private EBookSingleRunner runner_;
        private EBookSingleRunner runner => runner_ ??= GetComponent<EBookSingleRunner>();



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private int mainCategory = 0;
        [SerializeField] private int subCategory = 0;
        [SerializeField] private int ebookNum = 1;
        [Header("★ Dev")]
        [SerializeField] private string skipStateTo = string.Empty;
        [SerializeField] private string userID = string.Empty;
        [SerializeField] private string userPassword = string.Empty;

        // Unity Messages
        private void Awake()
        {
            if (RunnerParam.SelectedIDX == null)
            {
                SystemUI.One.Fader.FadeOutNow();

                var ebIDX = new EBookRecordIndex(mainCategory, subCategory, ebookNum);
                RunnerParam.SelectedIDX = ebIDX;
                RunnerParam.SkipStateTo = skipStateTo;

                LOG.Info($"Start SceneTest | {ebIDX}", this);
                StartCoroutine(coRunEBook());
            }
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coRunEBook()
        {
            yield return LMS.One.SignInChild(userID, userPassword).ToCoroutine();
            yield return runner.Prepare().ToCoroutine();
            yield return SystemUI.One.Fader.FadeIn();

            runner.Run();
            yield return null;
        }
    }
}