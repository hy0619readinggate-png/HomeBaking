using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DoDoEng.Activity.UI
{
    public enum ActivityPopupResult { NA, Back, Retry };
    public class ActivityCompletePopup : PopupBase<ActivityPopupResult>
    {
        // Methods
        public async UniTask<ActivityPopupResult> ShowPopup(int coinCount)
        {
            LOG.Function(this, $"{coinCount}");

            this.coinCount = coinCount;
            coinAdded = false;

            currentTL = coinCount > 0 ? coinShowTL : greatShowTL;
            evaluateTimeline(currentTL);

            if (coinCount > 0)
                coinTXT.text = $"+{coinCount.ToString()}";

            AudioMGR.One.PlayEffect(popupCLIP);
            return await showPopup();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private CoinMGR coinMGR_ = null;
        private CoinMGR coinMGR => coinMGR_ ??= GetComponent<CoinMGR>();

        // Fields
        private int coinCount;
        private bool coinAdded;
        private PlayableDirector currentTL = null;
        private Coroutine crPlayAnimation;

        // Functions
        private void skipAnimation()
        {
            this.StopCoroutineSafe(ref crPlayAnimation);

            coinMGR.StopGetCoin();

            if (!coinAdded)
                LMS.One.Coin += coinCount;
        }

        // Event Handlers
        private void backBTN_OnClick()
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;
            skipAnimation();

            DOVirtual.DelayedCall(backDelay, () => CloseWithResult(ActivityPopupResult.Back));
        }
        private void retryBTN_OnClick()
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;
            skipAnimation();

            DOVirtual.DelayedCall(retryDelay, () => CloseWithResult(ActivityPopupResult.Retry));
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();

            crPlayAnimation = StartCoroutine(coPlayAnimation());
        }
        protected override IEnumerator onClosing()
        {
            currentTL = coinCount > 0 ? coinHideTL : greatHideTL;
            yield return playTimeline(currentTL);
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button backBTN = null;
        [SerializeField] private Button retryBTN = null;
        //[SerializeField] private GameObject coinInfoGO = null;
        //[SerializeField] private GetCoinEffect coinFxPB = null;
        //[SerializeField] private Transform coinFXStartTR = null;
        [SerializeField] private TextMeshProUGUI coinTXT = null;
        [SerializeField] private Animator getCoinTextAnim = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        //[SerializeField] private AudioClip coinCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float backDelay = 0.5f;
        [SerializeField] private float retryDelay = 1.5f;
        //[SerializeField] private float coinPreDelay = 0.4f;
        //[SerializeField] private float coinDelay = 1.0f;
        //[SerializeField] private float coinPostDelay = 1.0f;
        [SerializeField] private float autoCloseDelay = 2.0f;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector greatShowTL = null;
        [SerializeField] private PlayableDirector greatHideTL = null;
        [SerializeField] private PlayableDirector coinShowTL = null;
        [SerializeField] private PlayableDirector coinHideTL = null;

        // Unity Messages
        private void Awake()
        {
            backBTN.onClick.AddListener(backBTN_OnClick);
            retryBTN.onClick.AddListener(retryBTN_OnClick);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            cg.blocksRaycasts = true;
        }

        // Unity Coroutine
        IEnumerator coPlayAnimation()
        {
            using (LOG.Coroutine($"coPlayAnimation()", this))
            {
                // 팝업 등장
                yield return playTimeline(currentTL);

                // 리워드(코인) 획득 애니메이션
                if (coinCount > 0)
                {
                    getCoinTextAnim.SetTrigger("Get");
                    yield return null;

                    yield return coinMGR.StartGetCoin(coinCount);

                    // devBOX(swon) : coin

                    coinAdded = true;
                    LMS.One.Coin += coinCount;
                    yield return null;
                }

                yield return new WaitForSeconds(autoCloseDelay);
                CloseWithResult(ActivityPopupResult.Back);
            }
        }
    }
}