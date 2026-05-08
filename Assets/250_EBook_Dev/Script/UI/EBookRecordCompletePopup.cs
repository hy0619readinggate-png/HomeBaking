using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DoDoEng.EBook.UI
{
    public class EBookRecordCompletePopup : PopupBase<EBookPopupResult>
    {
        // Methods
        public async UniTask<EBookPopupResult> ShowPopup(Sprite thumbnail, int coinCount, bool read = false, bool recorded = false, bool quizDone = false)
        {
            LOG.Function(this, $"coinCount={coinCount} | read={read} | recorded={recorded} | quizDone={quizDone}");

            thumbnailIMG.ForEach(img => img.sprite = thumbnail);
            this.coinCount = coinCount;
            coinAdded = false;

            currentTL = coinCount > 0 ? coinShowTL : normalShowTL;
            evaluateTimeline(currentTL);

            if (coinCount > 0)
                coinTXT.text = $"+{coinCount.ToString()}";

            readGO.ForEach(go => go.SetActive(read));
            recordedGO.ForEach(go => go.SetActive(recorded));
            quizGO.ForEach(go => go.SetActive(quizDone));
            myEBookBTN.ForEach(btn => btn.gameObject.SetActive(recorded));

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

            DOVirtual.DelayedCall(buttonDelay, () => CloseWithResult(EBookPopupResult.Back));
        }
        private void readBTN_OnClick()
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;
            skipAnimation();

            DOVirtual.DelayedCall(buttonDelay, () => CloseWithResult(EBookPopupResult.Read));
        }
        private void quizBTN_OnClick()
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;
            skipAnimation();

            DOVirtual.DelayedCall(buttonDelay, () => CloseWithResult(EBookPopupResult.Quiz));
        }
        private async void recordBTN_OnClickAsync()
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;
            skipAnimation();

            var result = await UIEBookCommon.One.ConfirmRerecordPopup.ShowPopup();
            if (result == SimplePopupResult.Yes)
            {
                CloseWithResult(EBookPopupResult.Record);
            }
            else cg.blocksRaycasts = true;
        }
        private void myEBookBTN_OnClick()
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;
            skipAnimation();

            DOVirtual.DelayedCall(buttonDelay, () => CloseWithResult(EBookPopupResult.MyEBook));
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();

            crPlayAnimation = StartCoroutine(coPlayAnimation());
        }
        protected override IEnumerator onClosing()
        {
            currentTL = coinCount > 0 ? coinHideTL : normalHideTL;
            yield return playTimeline(currentTL);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image[] thumbnailIMG = null;
        [SerializeField] private Button[] backBTN = null;
        [SerializeField] private Button[] readBTN = null;
        [SerializeField] private Button[] quizBTN = null;
        [SerializeField] private Button[] recordBTN = null;
        [SerializeField] private Button[] myEBookBTN = null;
        [SerializeField] private GameObject[] readGO = null;
        [SerializeField] private GameObject[] quizGO = null;
        [SerializeField] private GameObject[] recordedGO = null;
        [SerializeField] private TextMeshProUGUI coinTXT = null;
        [Header("★ Config")]
        [SerializeField] private float buttonDelay = 0.5f;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector normalShowTL = null;
        [SerializeField] private PlayableDirector normalHideTL = null;
        [SerializeField] private PlayableDirector coinShowTL = null;
        [SerializeField] private PlayableDirector coinHideTL = null;

        // Unity Messages
        private void Awake()
        {
            backBTN.ForEach(btn => btn.onClick.AddListener(backBTN_OnClick));

            readBTN.ForEach(btn => btn.onClick.AddListener(readBTN_OnClick));
            quizBTN.ForEach(btn => btn.onClick.AddListener(quizBTN_OnClick));
            recordBTN.ForEach(btn => btn.onClick.AddListener(recordBTN_OnClickAsync));
            myEBookBTN.ForEach(btn => btn.onClick.AddListener(myEBookBTN_OnClick));
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
                    yield return coinMGR.StartGetCoin(coinCount);

                    coinAdded = true;
                    LMS.One.Coin += coinCount;
                    yield return null;
                }
            }
        }
    }
}