using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A01
{
    public class TraceAff : MonoBehaviour
    {
        // Methods
        public void StartAff()
        {
            LOG.Info($"StartAff()", this);

            initDots();
            crAff = StartCoroutine(coAff());
        }
        public void FinishAff()
        {
            LOG.Info($"FinishAff()", this);

            StopCoroutine(crAff);
            clearDots();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Image[] dots_ = null;
        private Image[] dots => dots_ ??= GetComponentsInChildren<Image>(true);

        // Fields
        private Coroutine crAff = null;

        // Functions
        private void initDots()
        {
            dots.ForEach(d => d.gameObject.SetActive(true));
        }
        private void clearDots()
        {
            dots.ForEach(d => d.gameObject.SetActive(false));
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private float aniDotInterval = 0.1f; // seconds
        [SerializeField] private float aniSeqInterval = 1f; // seconds
        [SerializeField] private Color affCOLOR = Color.black;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            clearDots();
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coAff()
        {
            while (true)
            {
                foreach (var dot in dots)
                {
                    var color = dot.color;

                    dot.DOColor(affCOLOR, aniDotInterval / 2);
                    yield return new WaitForSeconds(aniDotInterval / 2);

                    dot.DOColor(color, aniDotInterval / 2);
                    yield return new WaitForSeconds(aniDotInterval / 2);
                }

                yield return new WaitForSeconds(aniSeqInterval);
            }
        }
    }
}