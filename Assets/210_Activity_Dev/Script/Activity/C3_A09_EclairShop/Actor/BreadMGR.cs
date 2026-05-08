using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C3_A09
{
    public class BreadMGR : MonoBehaviour
    {
        // Properties
        public Bread[] AvaliableBreads => breads.Where(b => b.IsAvailable && b.IsIdle).ToArray();

        // Methods
        public void Setup(ExampleData[] exams)
        {
            LOG.Info($"Setup()", this);

            breads.ForEach((i, b) => b.Setup(exams[i]));
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction()", this);

            cg.blocksRaycasts = enable;
        }
        public Coroutine StartShow()
        {
            LOG.Info($"StartShow()", this);

            crShow = StartCoroutine(coShow());
            return crShow;
        }
        public void FinishShow()
        {
            LOG.Info($"FinishShow()", this);

            this.StopCoroutineSafe(ref crShow);

            breads.ForEach(b => b.Idle());
        }
        public Coroutine StartHide(bool all = false)
        {
            LOG.Info($"StartHide()", this);

            crHide = StartCoroutine(coHide(all));
            return crHide;
        }
        public void FinishHide()
        {
            LOG.Info($"FinishHide()", this);

            this.StopCoroutineSafe(ref crHide);

            breads.ForEach(b => b.Hidden());
        }
        public void HideTexts()
        {
            LOG.Info($"HideTexts()", this);

            breads.ForEach(b => b.HideText());
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Bread[] breads_ = null;
        private Bread[] breads => breads_ ??= GetComponentsInChildren<Bread>();

        // Fields
        private Coroutine crShow = null;
        private Coroutine crHide = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform dragTR = null;
        [Header("★ Config")]
        [SerializeField] private float showHideInterval = 0.3f;



        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            breads.ForEach(b => b.Init(dragTR));
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coShow()
        {
            using (LOG.Coroutine($"coShow()", this))
            {
                foreach (var b in breads)
                {
                    b.Show();
                    yield return new WaitForSeconds(showHideInterval);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
        IEnumerator coHide(bool all = false)
        {
            using (LOG.Coroutine($"coHide() | {all}", this))
            {
                foreach (var b in breads.Where(b => (b.IsIdle || all)))
                {
                    b.Hide();
                    yield return new WaitForSeconds(showHideInterval);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}