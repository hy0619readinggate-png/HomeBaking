using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.UI
{
    public class GameFailPopup : PopupBase<GamePopupResult>
    {
        // Methods
        public async UniTask<GamePopupResult> ShowPopup(int slot, GameResult result, int earnCoin)
        {
            LOG.Info($"ShowPopup() | {slot} {result} {earnCoin}", this);

            coinCount = earnCoin;
            coinAdded = false;

            slotTXT.text = $"Slot {slot}";
            coinTXT.text = $"{earnCoin}";

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
            LOG.Info($"backBTN_OnClick()", this);

            cg.blocksRaycasts = false;
            skipAnimation();

            DOVirtual.DelayedCall(backDelay, () => CloseWithResult(GamePopupResult.Back));
        }
        private async void retryBTN_OnClick()
        {
            LOG.Info($"retryBTN_OnClick()", this);

            cg.blocksRaycasts = false;
            skipAnimation();

            if (await LMS.One.UseCandy())
            {
                candyGO.SetActive(true);
                AudioMGR.One.PlayEffect(candyCLIP);

                DOVirtual.DelayedCall(retryDelay, () => CloseWithResult(GamePopupResult.Retry));
            }
            else
            {
                await UIGameCommon.One.LimitPopup.ShowPopup();
                DOVirtual.DelayedCall(backDelay, () => CloseWithResult(GamePopupResult.Back));
            }
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();

            crPlayAnimation = StartCoroutine(coPlayAnimation());
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI slotTXT = null;
        [SerializeField] private TextMeshProUGUI coinTXT = null;
        [SerializeField] private Button backBTN = null;
        [SerializeField] private Button retryBTN = null;
        [SerializeField] private GameObject candyGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        [SerializeField] private AudioClip candyCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float backDelay = 0.5f;
        [SerializeField] private float retryDelay = 1.5f;

        // Unity Messages
        private void Awake()
        {
            candyGO.SetActive(false);

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
                if (coinCount > 0)
                {
                    yield return coinMGR.StartGetCoin(coinCount);

                    coinAdded = true;
                    LMS.One.Coin += coinCount;
                    yield return null;
                }
            }
        }
    }
}