using beyondi.Util;
using DG.Tweening;
using NaughtyAttributes;
using System.Collections;
using UnityEngine;


namespace DoDoEng.Common
{
    public class CoinMGR : MonoBehaviour
    {
        // Methods
        public Coroutine StartGetCoin(int coinCount)
        {
            LOG.Function(this);

            crGetCoin = StartCoroutine(coGetCoin(coinCount));
            return crGetCoin;

        }
        public void StopGetCoin()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crGetCoin);

            var fxs = GetComponentsInChildren<GetCoinEffect>(true);
            foreach (var fx in fxs)
                Destroy(fx.gameObject);
        }



        // Fields
        private Coroutine crGetCoin;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GetCoinEffect coinPB = null;
        [SerializeField] private CoinInfoInPopup coinInfo = null;
        [Label("시작 위치")][SerializeField] private Transform startTR = null;
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
            coinPB.gameObject.SetActive(false);
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coGetCoin(int coinCount)
        {
            using (LOG.Coroutine($"coGetCoin()", this))
            {
                yield return new WaitForSeconds(preDelay);

                for (var i = 0; i < coinCount; i++)
                {
                    var fx = Instantiate(coinPB, transform);
                    fx.gameObject.SetActive(true);
                    fx.SpawnAndFly(
                        startTR.position, targetTR.position,
                        spawnDuration, spawnRange, waitDuration, flyDuration,
                        spawnCLIP, flyCLIP,
                        spawnEase, flyEase,
                        () => coinInfo.AddCoin(1));
                    yield return new WaitForSeconds(spawnInterval.RandomValue());
                }

                yield return new WaitForSeconds(spawnDuration + waitDuration + flyDuration);
            }
        }
    }
}