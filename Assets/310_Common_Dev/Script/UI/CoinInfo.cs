using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class CoinInfo : MonoBehaviour
    {
        // Methods
        public async UniTask StartGetCoin(Transform startTR, int coin)
        {
            LOG.Function(this);

            await aniGetCoin(cancellationTokenSource.Token, startTR, coin);
        }
        public void StopGetCoin()
        {
            LOG.Function(this);

            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            var fxs = GetComponentsInChildren<GetCoinEffect>(true);
            foreach (var fx in fxs)
                Destroy(fx.gameObject);

            textTMP.text = $"{LMS.One.Coin}";
        }



        // Event Handlers
        private void lms_OnChangeCoin(int coin)
        {
            if (nowGettingAni) return;

            int getCoin = coin - int.Parse(textTMP.text);
            if (getCoin < 0)
            {
                getTMP.text = $"{getCoin}";
                getAni.SetTrigger("Get");
            }
            else if (getCoin > 0)
            {
                //var fx = Instantiate(getFX, transform);
                //fx.gameObject.SetActive(true);
                //getTMP.text = $"+{getCoin}";
                //getAni.SetTrigger("Get");
            }
            textTMP.text = $"{coin}";
        }

        // Functions
        private async UniTask aniGetCoin(CancellationToken cancellationToken, Transform startTR, int coin)
        {
            using (LOG.Coroutine($"aniGetCoin()", this))
            {
                nowGettingAni = true;
                LMS.One.Coin += coin;
                nowGettingAni = false;

                await UniTask.Delay((int)(preDelay * 1000));

                if (cancellationToken.IsCancellationRequested) return;

                for (var i = 0; i < coin; i++)
                {
                    var fx = Instantiate(coinPB, transform);
                    fx.gameObject.SetActive(true);
                    fx.SpawnAndFly(
                        startTR.position, targetTR.position,
                        spawnDuration, spawnRange, waitDuration, flyDuration,
                        spawnCLIP, flyCLIP,
                        spawnEase, flyEase,
                        () => effAddCoin(1));
                    await UniTask.Delay((int)(spawnInterval.RandomValue() * 1000));
                    if (cancellationToken.IsCancellationRequested) return;
                }

                await UniTask.Delay((int)((spawnDuration + waitDuration + flyDuration) * 1000));
                textTMP.text = $"{LMS.One.Coin}";
            }
        }
        public void effAddCoin(int coin)
        {
            LOG.Function(this);

            if (coin > 0)
            {
                var fx = Instantiate(getFX, getFX.transform.parent);
                fx.gameObject.SetActive(true);
                getTMP.text = $"+{coin}";
            }
            textTMP.text = $"{coin + int.Parse(textTMP.text)}";
        }

        // Fields
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private bool nowGettingAni = false;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text textTMP = null;
        [SerializeField] private TMP_Text getTMP = null;
        [SerializeField] private Animator getAni = null;
        [SerializeField] private GameObject getFX = null;
        [SerializeField] private GetCoinEffect coinPB = null;
        [SerializeField] private Button coinBT = null;
        [Label("도착 위치")][SerializeField] private Transform targetTR = null;
        [Header("★ Sound")]
        [Label("생성 효과음")][SerializeField] private AudioClip spawnCLIP = null;
        [Label("날라갈때 효과음")][SerializeField] private AudioClip flyCLIP = null;
        [Header("★ Config")]
        [Label("동전 생성 간격")][SerializeField] private Range spawnInterval = new Range(0.05f, 0.1f);
        [Label("생성 시간")][SerializeField] private float spawnDuration = 0.2f;
        [Label("생성 범위")][SerializeField] private float spawnRange = 1.2f;
        [Label("생성 후 대기 시간")][SerializeField] private float waitDuration = 0.4f;
        [Label("날라기는 시간")][SerializeField] private float flyDuration = 0.6f;
        [Label("사전 딜레이")][SerializeField] private float preDelay = 0.4f;
        [Label("생성시 Easing")][SerializeField] private Ease spawnEase = Ease.OutCirc;
        [Label("날라가는 Easing")][SerializeField] private Ease flyEase = Ease.InQuad;

        // Unity Messages
        private void Awake()
        {
            textTMP.text = $"{LMS.One.Coin}";
            coinPB.gameObject.SetActive(false);

            coinBT.onClick.AddListener(() => SystemUI.One.CoinHistoryPU.ShowPopup().Forget());
        }
        private void OnEnable()
        {
            LMS.One.OnChangeCoin += lms_OnChangeCoin;
        }
        private void OnDisable()
        {
            if (LMS.One != null)
            {
                LMS.One.OnChangeCoin -= lms_OnChangeCoin;
            }
        }
    }
}