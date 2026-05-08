using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A04
{
    public class StingrayGroup : MonoBehaviour
    {
        // Property
        public StingrayExam[] Exams => stingrayExams;

        // Methods
        public void Setup(ExampleData[] exams)
        {
            LOG.Info($"Setup()", this);

            exams.ForEach((i, e) => stingrayExams[i].Setup(e));
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public Coroutine Show()
        {
            LOG.Info($"Show()", this);

            crShow = StartCoroutine(coShow());
            return crShow;
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            stingrayExams.ForEach(s => s.Idle());
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            stingrayExams.ForEach(s => s.Hide());
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private Coroutine crShow;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private StingrayExam[] stingrayExams = null;
        [Header("★ Config")]
        [SerializeField] private float stingrayShowDelay = 0.3f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coShow()
        {
            using (LOG.Coroutine($"coShow()", this))
            {
                var randomSeq = UtilArray.Random(0, stingrayExams.Length - 1);
                foreach (var (s, i) in stingrayExams.Select((s, i) => (s, i)))
                {
                    stingrayExams[randomSeq[i]].Show();
                    yield return new WaitForSeconds(stingrayShowDelay);
                }
                yield return null;

                yield return new WaitForSeconds(1f);
                yield return null;
            }
        }
    }
}