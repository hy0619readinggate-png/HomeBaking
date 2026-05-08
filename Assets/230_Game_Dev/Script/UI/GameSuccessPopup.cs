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
    public enum GamePopupResult { NA, Back, Next, Retry };
    public class GameSuccessPopup : PopupBase<GamePopupResult>
    {
        // Methods
        public async UniTask<GamePopupResult> ShowPopup(int slot, GameResult result, bool canNext, int coinCount)
        {
            LOG.Info($"ShowPopup() | {slot} {result} {canNext}", this);

            this.slot = slot;

            starCount = result.EarnStar;
            this.coinCount = coinCount;
            coinAdded = false;

            slotTXT.text = $"Slot {slot}";
            coinTXT.text = $"{coinCount}";

            nextSetGO.SetActive(canNext);
            nextBTN.gameObject.SetActive(RunnerParam.CanNextPlayground());

            AudioMGR.One.PlayEffect(popupCLIP);
            return await showPopup();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private CoinMGR coinMGR_ = null;
        private CoinMGR coinMGR => coinMGR_ ??= GetComponent<CoinMGR>();

        // Fields 
        private int starCount;
        private int coinCount;
        private bool coinAdded;
        private Coroutine crPlayAnimation;
        private int slot;

        // Functions
        private void skipAnimation()
        {
            this.StopCoroutineSafe(ref crPlayAnimation);

            for (var i = 0; i < starCount; i++)
                stars[i].SetTrigger("GetNoAni");

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
        private async void nextBTN_OnClick()
        {
            LOG.Info($"nextBTN_OnClick()", this);

            cg.blocksRaycasts = false;
            skipAnimation();

            int candy = await LMS.One.LoadCandy();
            if (candy > 0)
            {
                if (await UIGameCommon.One.StartPU.ShowPopup(slot + 1, RunnerParam.PlaygroundNexts[0]) == SimplePopupResult.Yes)
                {
                    await LMS.One.UseCandy();
                    candyNextGO.SetActive(true);
                    AudioMGR.One.PlayEffect(candyCLIP);

                    DOVirtual.DelayedCall(nextDelay, () => CloseWithResult(GamePopupResult.Next));
                }
                else
                {
                    cg.blocksRaycasts = true;
                }
            }
            else
            {
                await UIGameCommon.One.LimitPopup.ShowPopup();
                DOVirtual.DelayedCall(backDelay, () => CloseWithResult(GamePopupResult.Back));
            }
        }
        private async void retryBTN_OnClick()
        {
            LOG.Info($"retryBTN_OnClick()", this);

            cg.blocksRaycasts = false;
            skipAnimation();

            if (await LMS.One.UseCandy())
            {
                candyRetryGO.SetActive(true);
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
        [SerializeField] private Animator[] stars = null;
        [SerializeField] private GameObject nextSetGO = null;
        [SerializeField] private Button backBTN = null;
        [SerializeField] private Button nextBTN = null;
        [SerializeField] private Button retryBTN = null;
        [SerializeField] private GameObject candyNextGO = null;
        [SerializeField] private GameObject candyRetryGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        [SerializeField] private AudioClip starCLIP = null;
        [SerializeField] private AudioClip candyCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float starPreDelay = 1f;
        [SerializeField] private float starDelay = 0.4f;
        [SerializeField] private float backDelay = 0.5f;
        [SerializeField] private float nextDelay = 2f;
        [SerializeField] private float retryDelay = 1.5f;

        // Unity Messages
        private void Awake()
        {
            candyNextGO.SetActive(false);
            candyRetryGO.SetActive(false);

            backBTN.onClick.AddListener(backBTN_OnClick);
            nextBTN.onClick.AddListener(nextBTN_OnClick);
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
                yield return new WaitForSeconds(starPreDelay);

                // 별 획득 애니메이션
                for (var i = 0; i < starCount; i++)
                {
                    AudioMGR.One.PlayEffect(starCLIP);
                    stars[i].SetTrigger("Get");
                    stars[i].transform.SetAsLastSibling();

                    yield return new WaitForSeconds(starDelay);
                }

                // 리워드(코인) 획득 애니메이션
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