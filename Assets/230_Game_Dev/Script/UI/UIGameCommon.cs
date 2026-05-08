using beyondi.Behaviour;
using DoDoEng.Common;
using DoDoEng.Game.Common;
using DoDoEng.Playground.UI;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Game.UI
{
    public class UIGameCommon : BYDSingleton<UIGameCommon>
    {
        // Properties
        public GamePausePopup PausePopup => pausePU;
        public SimplePopup ConfirmExitPopup => confirmExitPU;
        public SimplePopup LimitPopup => limitPU;
        public GameSuccessPopup PlaygroundSuccessPopup => playgroundSuccessPU;
        public GameFailPopup PlaygroundFailPopup => playgroundFailPU;
        public GameReviewCompletePopup ReviewCompletePU => reviewCompletePU;
        public GameHowToPlayPopup HowToPlayPU => howToPlayPU;
        public PlaygroundStartPopup StartPU => startPU;

        // Properties
        public UIStarGauge StarGauge => starGauge;
        public UILife Life => life;
        public UIProgress Progress => progress;
        public UIHealthBar HealthBar => healthBar;
        public UITimer Timer => timer;
        public UITimerWithAnimator TimerWithAnimator => timerWA;

        // Properties
        public PlayableDirector ReadyGoTL => readyGoTL;
        public PlayableDirector LevelUpTL => levelUpTL;

        // Properties
        public bool VisibleBackButton
        {
            get => backButtonGO.activeSelf;
            set => backButtonGO.SetActive(value);
        }
        public bool VisiblePauseButton
        {
            get => pauseButtonGO.activeSelf;
            set => pauseButtonGO.SetActive(value);
        }



        // Fields : caching
        private UIStarGauge starGauge_ = null;
        private UIStarGauge starGauge => starGauge_ ??= FindObjectOfType<UIStarGauge>();
        private UILife life_ = null;
        private UILife life => life_ ??= FindObjectOfType<UILife>();
        private UIProgress progress_ = null;
        private UIProgress progress => progress_ ??= FindObjectOfType<UIProgress>();
        private UIHealthBar healthBar_ = null;
        private UIHealthBar healthBar => healthBar_ ??= FindObjectOfType<UIHealthBar>();
        private UITimer timer_ = null;
        private UITimer timer => timer_ ??= FindObjectOfType<UITimer>();
        private UITimerWithAnimator timerWA_ = null;
        private UITimerWithAnimator timerWA => timerWA_ ??= FindObjectOfType<UITimerWithAnimator>();



        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private GameObject backButtonGO = null;
        [SerializeField] private GameObject pauseButtonGO = null;
        [SerializeField] private SimplePopup confirmExitPU = null;
        [SerializeField] private SimplePopup limitPU = null;
        [SerializeField] private GamePausePopup pausePU = null;
        [SerializeField] private GameSuccessPopup playgroundSuccessPU = null;
        [SerializeField] private GameFailPopup playgroundFailPU = null;
        [SerializeField] private GameReviewCompletePopup reviewCompletePU = null;
        [SerializeField] private GameHowToPlayPopup howToPlayPU = null;
        [SerializeField] private GameObject readyGoPanel = null;
        [SerializeField] private PlaygroundStartPopup startPU = null;
        [Header("¡Ú TimeLine")]
        [SerializeField] private PlayableDirector readyGoTL = null;
        [SerializeField] private PlayableDirector levelUpTL = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            backButtonGO.SetActive(false);
            pauseButtonGO.SetActive(false);
            readyGoPanel.SetActive(false);

            pausePU.gameObject.SetActive(false);
            confirmExitPU.gameObject.SetActive(false);
            limitPU.gameObject.SetActive(false);
            playgroundSuccessPU.gameObject.SetActive(false);
            playgroundFailPU.gameObject.SetActive(false);
            reviewCompletePU.gameObject.SetActive(false);
            startPU?.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
    }
}