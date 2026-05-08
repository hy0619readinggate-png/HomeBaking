using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
// using RenderHeads.Media.AVProVideo;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Game.UI
{
    public class GameHowToPlayPopup : PopupBase<SimplePopupResult>, IPointerDownHandler, IPointerUpHandler
    {
        // Properteis
        public bool IsPlaying
        {
            get => isPlaying;
            private set
            {
                isPlaying = value;
                updateState();
            }
        }
        public bool IsMenuShown
        {
            get => isMenuShown;
            private set
            {
                isMenuShown = value;
                updateState();
            }
        }

        // Methods
        public void Ready(GameIndex gameIDX)
        {
            LOG.Function(this, $"{gameIDX}");

            // https://content.dev.gohidodo.com/null/C1_G01_HowTo.mp4
            var url = gameIDX.HowToVideoPath;
            // var path = new MediaPath(url, MediaPathType.AbsolutePathOrURL);
            // mediaPlayer.OpenMedia(path, false);
        }
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Info($"ShowPopup()", this);

            IsPlaying = true;
            IsMenuShown = false;
            // mediaPlayer.Rewind(true);

            return await showPopup();
        }



        // Fields
        private bool isMenuShown = false;
        private bool isPlaying = false;
        private bool wasPlayingBeforeTimelineDrag = false;
        private Coroutine crAutoHideMenu = null;

        // Functions
        private void updateState()
        {
            playBTN.gameObject.SetActive(!IsPlaying && IsMenuShown);
            pauseBTN.gameObject.SetActive(IsPlaying && IsMenuShown);

            pannelGO.SetActive(IsMenuShown);
        }
        // private TimeRange getTimelineRange()
        // {
        //     if (mediaPlayer.Info != null)
        //     {
        //         return Helper.GetTimelineRange(mediaPlayer.Info.GetDuration(), mediaPlayer.Control.GetSeekableTimes());
        //     }
        //     return new TimeRange();
        // }
        private void createTimelineDragEvents()
        {
            EventTrigger trigger = progressSlider.gameObject.GetComponent<EventTrigger>();
            if (trigger != null)
            {
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerDown;
                entry.callback.AddListener((data) => { OnTimeSliderBeginDrag(); });
                trigger.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback.AddListener((data) => { OnTimeSliderDrag(); });
                trigger.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerUp;
                entry.callback.AddListener((data) => { OnTimeSliderEndDrag(); });
                trigger.triggers.Add(entry);
            }
        }

        // Event Handlers
        private void playBTN_OnClick()
        {
            LOG.Info($"playBTN_OnClick()", this);

            // if (mediaPlayer.Control.GetPlaybackRate() == 1)
            //     mediaPlayer.Control.SeekFast(0);
            // mediaPlayer.Play();

            IsPlaying = true;

            this.StopCoroutineSafe(ref crAutoHideMenu);
            crAutoHideMenu = StartCoroutine(coAutoHideMenu());
        }
        private void pauseBTN_OnClick()
        {
            LOG.Info($"pauseBTN_OnClick()", this);

            // mediaPlayer.Pause();

            IsPlaying = false;
        }
        private void mediaPlayer_Event()//MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode
        {
            // LOG.Important($"mediaPlayer_Event() | {et}", this);

            // switch (et)
            // {
            //     case MediaPlayerEvent.EventType.MetaDataReady:
            //         break;

            //     case MediaPlayerEvent.EventType.ReadyToPlay:
            //         break;
            //     case MediaPlayerEvent.EventType.FinishedPlaying:
            //         IsPlaying = false;
            //         IsMenuShown = true;
            //         break;
            // }
        }
        private void OnTimeSliderBeginDrag()
        {
            // if (mediaPlayer && mediaPlayer.Control != null)
            // {
            //     wasPlayingBeforeTimelineDrag = mediaPlayer.Control.IsPlaying();
            //     if (wasPlayingBeforeTimelineDrag)
            //         mediaPlayer.Pause();
            //     OnTimeSliderDrag();
            // }
        }
        private void OnTimeSliderDrag()
        {
            // if (mediaPlayer && mediaPlayer.Control != null)
            // {
            //     TimeRange timelineRange = getTimelineRange();
            //     double time = timelineRange.startTime + (progressSlider.value * timelineRange.duration);
            //     mediaPlayer.Control.Seek(time);
            // }
        }
        private void OnTimeSliderEndDrag()
        {
            // if (mediaPlayer && mediaPlayer.Control != null)
            // {
            //     if (wasPlayingBeforeTimelineDrag)
            //     {
            //         mediaPlayer.Play();
            //         wasPlayingBeforeTimelineDrag = false;
            //     }
            // }
        }



        // Override
        protected override void onOpen()
        {
            base.onOpen();

            // mediaPlayer.Control.SeekFast(0);
            // mediaPlayer.Play();

            IsPlaying = true;
        }
        protected override void onClose(SimplePopupResult result)
        {
            base.onClose(result);

            // mediaPlayer.Stop();

            IsPlaying = false;
            this.StopCoroutineSafe(ref crAutoHideMenu);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        // [SerializeField] private MediaPlayer mediaPlayer = null;
        // [SerializeField] private DisplayUGUI mediaDisplay = null;
        [SerializeField] private Button playBTN = null;
        [SerializeField] private Button pauseBTN = null;
        [SerializeField] private Slider progressSlider = null;
        [SerializeField] private GameObject pannelGO = null;
        [Header("★ Config")]
        [SerializeField] private float autoHideDuration = 3;

        // Unity Messages
        private void Awake()
        {
            playBTN.gameObject.SetActive(false);
            playBTN.onClick.AddListener(playBTN_OnClick);

            pauseBTN.gameObject.SetActive(false);
            pauseBTN.onClick.AddListener(pauseBTN_OnClick);

            // mediaDisplay.Player = mediaPlayer;
            // mediaPlayer.Events.AddListener(mediaPlayer_Event);

            createTimelineDragEvents();
        }
        private void Start()
        {
        }
        private void Update()
        {
            // if (mediaPlayer.Info != null)
            // {
            //     TimeRange timelineRange = getTimelineRange();

            //     double t = 0.0;
            //     if (timelineRange.duration > 0.0)
            //     {
            //         t = ((mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration);
            //     }
            //     progressSlider.value = Mathf.Clamp01((float)t);
            // }
        }
        private void OnEnable()
        {

        }



        // Unity Coroutine
        IEnumerator coAutoHideMenu()
        {
            using (LOG.Coroutine($"coAutoHideMenu()", this))
            {
                var timeout = autoHideDuration;
                while (timeout > 0)
                {
                    yield return null;

                    timeout -= Time.unscaledDeltaTime;

                    if (Input.GetMouseButtonDown(0))
                        timeout = autoHideDuration;
                }

                IsMenuShown = false;
            }
        }



        // Implementation Interface
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown() | {eventData.position}", this);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp() | {eventData.position}", this);

            if (!IsMenuShown)
            {
                IsMenuShown = true;

                this.StopCoroutineSafe(ref crAutoHideMenu);
                crAutoHideMenu = StartCoroutine(coAutoHideMenu());
            }
            else
            {
                IsMenuShown = false;

                this.StopCoroutineSafe(ref crAutoHideMenu);
            }
        }
    }
}