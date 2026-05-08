using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.DoDoEng.EBook.UI;
using DoDoEng.EBook.Framework;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DoDoEng.EBook.UI
{
    public class EBookQuizCompletePopup : PopupBase<EBookPopupResult>
    {
        // Methods
        public async UniTask<EBookPopupResult> ShowPopup(EBookQuizResult result, int coinCount, bool read, bool recorded, bool quizDone)
        {
            LOG.Function(this, $"{result} | {coinCount} | read={read} | recorded={recorded} | quizDone={quizDone}");

            this.result = result;
            this.coinCount = coinCount;
            coinAdded = false;

            for (var i = 0; i < detailItems.Length; i++)
            {
                var detailItem = detailItems[i];
                if (i >= result.ProblemCount)
                    detailItem.gameObject.SetActive(false);
                else
                {
                    detailItem.gameObject.SetActive(true);
                    detailItem.Setup(i + 1, result.Corrections[i]);
                }
            }

            var emblemIdx = result.GetEmblemIdx();
            emblemGO.SetActiveOnly(emblemIdx);
            titleTXT.text = titles[emblemIdx];
            coinPannelGO.gameObject.SetActive(coinCount > 0);
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
        private QuizPopupDetailItem[] detailItems_ = null;
        private QuizPopupDetailItem[] detailItems => detailItems_ ??= GetComponentsInChildren<QuizPopupDetailItem>(true);
        private CoinMGR coinMGR_ = null;
        private CoinMGR coinMGR => coinMGR_ ??= GetComponent<CoinMGR>();

        // Fields
        private int current = 0;
        private EBookQuizResult result = null;
        private int coinCount;
        private bool coinAdded;
        private Coroutine crPlayAnimation;

        // Functions
        private void showDetail(int id)
        {
            current = id;

            detailGO.SetActive(true);

            detailPrevBTN.gameObject.SetActive(id > 1);
            detailNextBTN.gameObject.SetActive(id < result.ProblemCount);
            detailIMG.sprite = result.CapturedImages[current - 1];

            detailPNOTXT.text = $"<color=#8bbcea>{id}</color>/{result.ProblemCount}";
        }
        private void closeDetail()
        {
            detailGO.SetActive(false);
        }
        private void skipAnimation()
        {
            this.StopCoroutineSafe(ref crPlayAnimation);

            coinMGR.StopGetCoin();

            if (!coinAdded)
                LMS.One.Coin += coinCount;
        }


        // Event Handlers
        private void detailItem_OnClick(int id)
        {
            LOG.Function(this, $"{id}");

            showDetail(id);
        }
        private void detailCloseBTN_OnClick()
        {
            LOG.Function(this);

            closeDetail();
        }
        private void detailPrevBTN_OnClick()
        {
            LOG.Function(this);

            if (current > 1)
                showDetail(current - 1);
        }
        private void detailNextBTN_OnClick()
        {
            LOG.Function(this);

            if (current < result.ProblemCount)
                showDetail(current + 1);
        }
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
        private void recordBTN_OnClick()
        {
            LOG.Function(this);

            cg.blocksRaycasts = false;
            skipAnimation();

            DOVirtual.DelayedCall(buttonDelay, () => recordBTN_OnClickAsync().Forget());
        }
        private async UniTask recordBTN_OnClickAsync()
        {
            if (recordedGO[0].activeSelf)
            {
                if (await UIEBookCommon.One.ConfirmRerecordPopup.ShowPopup() != SimplePopupResult.Yes)
                {
                    cg.blocksRaycasts = true;
                    return;
                }
            }
            CloseWithResult(EBookPopupResult.Record);
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
            yield return playTimeline(normalHideTL);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button[] backBTN = null;
        [SerializeField] private Button[] readBTN = null;
        [SerializeField] private Button[] quizBTN = null;
        [SerializeField] private Button[] recordBTN = null;
        [SerializeField] private Button[] myEBookBTN = null;
        [SerializeField] private GameObject[] readGO = null;
        [SerializeField] private GameObject[] quizGO = null;
        [SerializeField] private GameObject[] recordedGO = null;
        [SerializeField] private GameObject[] emblemGO = null;
        [SerializeField] private TextMeshProUGUI titleTXT = null;
        [SerializeField] private GameObject detailGO = null;
        [SerializeField] private Image detailIMG = null;
        [SerializeField] private Button detailCloseBTN = null;
        [SerializeField] private Button detailPrevBTN = null;
        [SerializeField] private Button detailNextBTN = null;
        [SerializeField] private TextMeshProUGUI detailPNOTXT = null;
        [SerializeField] private GameObject coinPannelGO = null;
        [SerializeField] private TextMeshProUGUI coinTXT = null;
        [Header("★ Config")]
        [SerializeField] private string[] titles = null;
        [SerializeField] private float buttonDelay = 0.5f;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector normalShowTL = null;
        [SerializeField] private PlayableDirector normalHideTL = null;

        // Unity Messages
        private void Awake()
        {
            detailGO.SetActive(false);

            backBTN.ForEach(btn => btn.onClick.AddListener(backBTN_OnClick));

            readBTN.ForEach(btn => btn.onClick.AddListener(readBTN_OnClick));
            quizBTN.ForEach(btn => btn.onClick.AddListener(quizBTN_OnClick));
            recordBTN.ForEach(btn => btn.onClick.AddListener(recordBTN_OnClick));
            myEBookBTN.ForEach(btn => btn.onClick.AddListener(myEBookBTN_OnClick));

            detailCloseBTN.onClick.AddListener(detailCloseBTN_OnClick);
            detailPrevBTN.onClick.AddListener(detailPrevBTN_OnClick);
            detailNextBTN.onClick.AddListener(detailNextBTN_OnClick);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            cg.blocksRaycasts = true;

            detailItems.ForEach(item => item.OnClick += detailItem_OnClick);
        }
        private void OnDisable()
        {
            detailItems.ForEach(item => item.OnClick -= detailItem_OnClick);
        }

        // Unity Coroutine
        IEnumerator coPlayAnimation()
        {
            using (LOG.Coroutine($"coPlayAnimation()", this))
            {
                // 팝업 등장
                yield return playTimeline(normalShowTL);

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