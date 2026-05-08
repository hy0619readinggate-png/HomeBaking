using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.UI
{
    public class UIActivityCommon : BYDSingleton<UIActivityCommon>
    {
        // Properties
        public SimplePopup PausePopup => pausePU;
        public SimplePopup ConfirmExitPopup => confirmExitPU;
        public ActivityCompletePopup CompletePopup => completePU;

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
        public bool VisiblePauseButton
        {
            get => visiblePauseButton;
            set
            {
                visiblePauseButton = value;
                updateButtonVisible();
            }
        }
        public bool VisibleSpeakerButton
        {
            get => visibleSpeakerButton;
            set
            {
                visibleSpeakerButton = value;
                updateButtonVisible();
            }
        }
        public bool EnableSpeakerButton
        {
            get => speakerBTN.interactable;
            set => speakerBTN.interactable = value;
        }




        // Fields
        private bool visibleBackButton = false;
        private bool visiblePauseButton = false;
        private bool visibleSpeakerButton = false;

        // Functions
        private void updateButtonVisible()
        {
            backButtonGO.SetActive(visibleBackButton);
            pauseButtonGO.SetActive(visiblePauseButton && !visibleSpeakerButton);
            speakerButtonGO.SetActive(visibleSpeakerButton);
        }



        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private GameObject backButtonGO = null;
        [SerializeField] private GameObject pauseButtonGO = null;
        [SerializeField] private GameObject speakerButtonGO = null;
        [SerializeField] private Button speakerBTN = null;
        [SerializeField] private SimplePopup pausePU = null;
        [SerializeField] private SimplePopup confirmExitPU = null;
        [SerializeField] private ActivityCompletePopup completePU = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            backButtonGO.SetActive(false);
            pauseButtonGO.SetActive(false);
            speakerButtonGO.SetActive(false);
            speakerBTN.interactable = false;

            pausePU.gameObject.SetActive(false);
            confirmExitPU.gameObject.SetActive(false);
            completePU.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}