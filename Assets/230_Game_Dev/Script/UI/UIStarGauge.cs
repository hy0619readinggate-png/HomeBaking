using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.Common
{
    public class UIStarGauge : MonoBehaviour
    {
        // Methods
        public void Setup(float[] ratioForStar, int total)
        {
            LOG.Info($"Setup()", this);

            starRatio = ratioForStar;
            totalCount = total;
            currentCount = 0;
            currentStar = 0;

            initialize();
            locateStars();
        }
        public void Success(int count = 1)
        {
            LOG.Info($"Success() | {count}", this);

            currentCount += count;
            updateGaguge();
        }
        public void SuccessForWonderland(int count = 1)
        {
            LOG.Info($"Success() | {count}", this);

            currentCount += count;
            updateGaugeForWonderland();
        }



        // Fields
        private float[] starRatio;
        private int totalCount;
        private int currentCount;
        private int currentStar;

        private float ratioW = 0f;

        // Functions
        private void initialize()
        {
            progressIMG.fillAmount = 0;
            starsANIM.ForEach(anim => anim.SetBool("On", false));
        }
        private void locateStars()
        {
            var baseRT = progressIMG.GetComponent<RectTransform>();
            var L = baseRT.offsetMin.x;
            var R = baseRT.offsetMax.x;
            foreach (var (star, i) in starsANIM.Select((star, i) => (star, i)))
            {
                var ratio = starRatio[i];
                var rt = star.GetComponent<RectTransform>();
                var x = Mathf.Lerp(L, R, ratio);
                var y = rt.anchoredPosition.y;
                rt.anchoredPosition = new Vector2(x, y);
            }
        }
        private void updateGaguge()
        {
            var duration = 0.2f;
            var ratio = Mathf.Clamp01(currentCount / (float)totalCount);
            progressIMG.DOFillAmount(ratio, duration);

            // devBOX(swon) - 별이 채워지지 않는 오류 수정 #3216
            StartCoroutine(coUpdateStar(duration * 1.2f));
        }
        private void updateGaugeForWonderland()
        {
            var duration = 0.2f;

            if (currentCount <= totalCount / 3 || currentCount >= totalCount * 2 / 3) ratioW += 0.15f;
            else ratioW += 0.25f;

            progressIMG.DOFillAmount(ratioW, duration);

            StartCoroutine(coUpdateStar(duration + 1.2f));
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image progressIMG = null;
        [SerializeField] private Animator[] starsANIM = null;

        // Unity Messages
        private void Awake()
        {
            progressIMG.fillAmount = 0;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coUpdateStar(float duration)
        {
            using (LOG.Coroutine($"coUpdateStar()", this))
            {
                var lamda = 0.001f;
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    if (currentStar >= starRatio.Length)
                        break;

                    if (progressIMG.fillAmount + lamda >= starRatio[currentStar])
                    {
                        starsANIM[currentStar].SetBool("On", true);
                        currentStar++;


                    }

                    yield return null;
                    elapsed += Time.deltaTime;
                }
            }
        }
    }
}