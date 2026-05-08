using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace DoDoEng.Launcher.UI
{
	public class MyInfoPopupPageCoin : TabPage
    {
        // Definitions
        // Properties

        // Methods
        public void Init(RewardTableLoaderResult rewardTable)
        {
            foreach (var ch in cardsRT.GetChildren())
                Destroy(ch.gameObject);
            int categoryId = 0;
            MyInfoPopupPageCoinTutorialCard card = null;
            foreach (var gui in rewardTable.RewardListGUI)
            {
                //LOG.Warning($"gui: {gui}", this);
                var reward = rewardTable.RewardList.SingleOrDefault(list => list.Index == gui.RewardIndex);
                if (reward != null)
                {
                    if (categoryId != gui.CategoryIndex)
                    {
                        var category = rewardTable.CategoryList.SingleOrDefault(category => category.Index == gui.CategoryIndex);
                        card = Instantiate(cardPF, cardsRT);
                        categoryId = gui.CategoryIndex;
                        card.Init(category.Name, category.Image);
                    }
                
                    card.AddText(LocalizationMGR.One.Select(reward.NameKor, reward.NameEng, reward.NameVie), reward.Coin);
                }
            }
        }

        // Events



        // Fields : caching
        // Fields
        // Functions

        // Event Handlers
        private void historyBT_onClick()
        {
            LOG.Function(this);

            SystemUI.One.CoinHistoryPU.ShowPopup().Forget();
        }
        private void tutorialBT_onClick()
        {
            LOG.Function(this);

            cardsScroll.horizontalNormalizedPosition = 0;
            tutorialGO.SetActive(true);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text coinTMP = null;
        [SerializeField] private Button historyBT = null;
        [SerializeField] private Button tutorialBT = null;
        [SerializeField] private GameObject tutorialGO = null;

        [SerializeField] private ScrollRect cardsScroll = null;
        [SerializeField] private RectTransform cardsRT = null;
        [SerializeField] private MyInfoPopupPageCoinTutorialCard cardPF = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            tutorialGO.SetActive(false);

            historyBT.onClick.AddListener(() => historyBT_onClick());
            tutorialBT.onClick.AddListener(() => tutorialBT_onClick());
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            coinTMP.text = String.Format("{0:N0}", LMS.One.Coin);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        // Unity Coroutine
    }
}