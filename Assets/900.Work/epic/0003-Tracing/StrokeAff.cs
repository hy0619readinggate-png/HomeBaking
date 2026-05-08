using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

namespace DoDoEng
{
    public class StrokeAff : MonoBehaviour
    {
        // Methods
        public void Setup(SplineContainer spline)
        {
            LOG.Info($"Setup()", this);

            clearDots();
            setupDots(spline);
        }
        public void StartAff()
        {
            LOG.Info($"StartAff()", this);

            crAff = StartCoroutine(coAff());
        }
        public void FinishAff()
        {
            LOG.Info($"FinishAff()", this);

            StopCoroutine(crAff);
            clearDots();
        }



        // Fields
        private Image[] dots = null;
        private Coroutine crAff = null;
        private Color originalDotColor;

        // Functions
        private void initDots()
        {
            var count = (int)(3000 / space);  // for max length 3000 pixel
            dots = new Image[count];
            dots[0] = dot;
            for (var i = 1; i < count; i++)
                dots[i] = Instantiate(dot, transform);

            clearDots();
        }
        private void setupDots(SplineContainer spline)
        {
            var length = spline.Spline.GetLength();
            var count = (int)(length / space + 1);

            dotStart.transform.position = spline.EvaluatePosition(0);
            dotStart.SetActive(true);
            for (var i = 0; i < count; i++)
            {
                var t = (i + 1) * (1f / count);
                dots[i].transform.position = spline.EvaluatePosition(t);
                dots[i].gameObject.SetActive(true);

                print($"{t} {1f / count} {count}");
            }
        }
        private void clearDots()
        {
            dotStart.SetActive(false);
            dots.ForEach(d => d.gameObject.SetActive(false));
            dots.ForEach(d => d.color = originalDotColor);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject dotStart = null;
        [SerializeField] private Image dot = null;
        [Header("★ Config")]
        [SerializeField] private float space = 80; // pixels
        [SerializeField] private float aniDotInterval = 0.1f; // seconds
        [SerializeField] private float aniSeqInterval = 1f; // seconds
        [SerializeField] private Color affCOLOR = Color.black;

        // Unity Messages
        private void Awake()
        {
            originalDotColor = dot.color;
            initDots();
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coAff()
        {
            var actives = dots.Where(d => d.gameObject.activeSelf);

            while (true)
            {
                foreach (var dot in actives)
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