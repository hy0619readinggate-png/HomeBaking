using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
    public class ContinuousAttendPopup : PopupBase<SimplePopupResult>
	{
        // Definitions
        // Properties

        // Methods
        public void Init(RewardTableLoaderResult rewardTable)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                rewardList[i] = rewardTable.RewardList.SingleOrDefault(list => list.Index == 9002 + i);
            }
        }
        public async UniTask<SimplePopupResult> ShowPopup(int numContinuous, int coin)
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(popupCLIP);

            this.numContinuous = numContinuous;
            this.coin = coin;

            return await showPopup();
        }

        // Events
        //[HideInInspector] public event Action OnSignOut;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private RewardList[] rewardList = new RewardList[5];
        private int numContinuous = 0;
        private int coin = 0;

        // Functions
        private async UniTask coinAni()
        {
            cg.blocksRaycasts = false;
            
            await coinInfo.StartGetCoin(slots[numContinuous - 1].transform, coin);

            cg.blocksRaycasts = true;
        }

        // Event Handlers

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Init(rewardList[i].Coin, i <= numContinuous - 1, i == numContinuous - 1, i == slots.Length - 1);
            }

            if (coin > 0)
                coinAni().Forget();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private ContinuousAttendSlot[] slots = null;
        [SerializeField] private CoinInfo coinInfo = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        //[Header("★ Config")]
        //[SerializeField] private float buttonDelay = 0.5f;

        // Unity Messages
        private void Awake()
		{
        }
		private void Start()
		{
        }
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }

        // Unity Coroutine
    }
}