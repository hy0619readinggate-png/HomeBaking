using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using DoDoEng.Common;
using Cysharp.Threading.Tasks;
using DoDoEng.Game.UI;

namespace DoDoEng.Playground.UI
{
	public class PlaygroundStartPopup : PopupBase<SimplePopupResult>
    {
        // Definitions
        // Properties

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(int slotNum, GameIndex idx)
        {
            LOG.Function(this);

            LOG.Function(this, $"| slotNum={slotNum} | index={idx}");

            titleTMP.text = $"Slot {slotNum}";

            var contentIndexArr = idx.Index.ToCharArray();
            var address = $"Game/Course{contentIndexArr[1]}/{idx.Index}.png";
            thumbnailIMG.sprite = null;
            try
            {
                thumbnailIMG.sprite = await DataLoader.One.LoadSprite(address);
            }
            catch (Exception ex) {
                LOG.Warning(ex.Message, this);
            }

            howToPlayPU.Ready(idx);

            return await showPopup();
        }

        // Events



        // Fields : caching

        // Fields

        // Functions

        // Event Handlers

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text titleTMP = null;
        [SerializeField] private Image thumbnailIMG = null;
        [SerializeField] private Button closeBT = null;
        [SerializeField] private Button startBT = null;
        [SerializeField] private Button howToPlayBT = null;
        [SerializeField] private GameHowToPlayPopup howToPlayPU = null;

        // Unity Messages
        private void Awake()
		{
            closeBT.onClick.AddListener(() => CloseWithResult(SimplePopupResult.Back));
            startBT.onClick.AddListener(() => CloseWithResult(SimplePopupResult.Yes));
            howToPlayBT.onClick.AddListener(() => howToPlayPU.ShowPopup().Forget());
        }
		private void Start()
		{
		}
        private void OnEnable()
        {
        }
        private void OnDisable()
        {
        }

        // Unity Coroutine
    }
}