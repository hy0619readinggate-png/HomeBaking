using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.UI
{
    public class GamePausePopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(GameIndex idx)
        {
            LOG.Info($"ShowPopup() | {idx}", this);

            UIGameCommon.One.HowToPlayPU.Ready(idx);
            return await showPopup();
        }



        // Functions
        private async void howToPlayBTN_OnClick()
        {
            LOG.Info($"howToPlayBTN_OnClick()", this);

            await UIGameCommon.One.HowToPlayPU.ShowPopup();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button howToPlayBTN = null;

        // Unity Messages
        private void Awake()
        {
            howToPlayBTN.onClick.AddListener(howToPlayBTN_OnClick);
        }
        private void Start()
        {

        }
    }
}