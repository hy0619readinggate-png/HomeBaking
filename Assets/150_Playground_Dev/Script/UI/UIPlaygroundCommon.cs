using beyondi.Behaviour;
using DoDoEng.Common;
using DoDoEng.Game.UI;
using UnityEngine;

namespace DoDoEng.Playground.UI
{
    public class UIPlaygroundCommon : BYDSingleton<UIPlaygroundCommon>
    {
        // Properties
        public GameHowToPlayPopup HowToPlayPU => howToPlayPU;
        public SimplePopup LimitPopup => limitPU;
        public PlaygroundStartPopup StartPU => startPU;
        public PlayCounter PlayCounter => playCounter;

        // Properties
        public bool VisibleBackButton
        {
            get => visibleBackButton;
            set
            {
                visibleBackButton = value;
                updateButtonVisible();
            }
        }



        // Fields
        private bool visibleBackButton = false;

        // Functions
        private void updateButtonVisible()
        {
            backButtonGO.SetActive(visibleBackButton);
        }



        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private GameObject backButtonGO = null;
        [SerializeField] private GameHowToPlayPopup howToPlayPU = null;
        [SerializeField] private SimplePopup limitPU = null;
        [SerializeField] private PlaygroundStartPopup startPU = null;
        [SerializeField] private PlayCounter playCounter = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            backButtonGO.SetActive(false);

            howToPlayPU.gameObject.SetActive(false);
            limitPU?.gameObject.SetActive(false);
            startPU?.gameObject.SetActive(false);

            playCounter.Activate(true);
        }
        private void Start()
        {
        }
    }
}