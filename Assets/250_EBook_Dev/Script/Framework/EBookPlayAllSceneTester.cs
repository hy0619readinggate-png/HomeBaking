using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.EBook.Framework
{
    [RequireComponent(typeof(EBookPlayAllRunner))]
    public class EBookPlayAllSceneTester : MonoBehaviour
    {
        // Fields : caching
        private EBookPlayAllRunner runner_;
        private EBookPlayAllRunner runner => runner_ ??= GetComponent<EBookPlayAllRunner>();



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private string[] eBookIndices = null;
        [SerializeField] private EBookReadMode eBookMode = EBookReadMode.Native;
        [Header("★ Dev")]
        [SerializeField] private string skipStateTo = string.Empty;

        // Unity Messages
        private void Awake()
        {
            if (RunnerParam.SelectedIDX == null)
            {
                SystemUI.One.Fader.FadeOutNow();

                var ebIDX = new EBookPlayAllIndex(eBookIndices, eBookMode);
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
            yield return runner.Prepare().ToCoroutine();
            yield return SystemUI.One.Fader.FadeIn();

            runner.Run();
            yield return null;
        }
    }
}