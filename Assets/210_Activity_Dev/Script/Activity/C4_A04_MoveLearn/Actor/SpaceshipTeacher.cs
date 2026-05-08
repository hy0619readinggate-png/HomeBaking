using beyondi.Util;
using DoDoEng.Common;
// using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C4_A04
{
    public class SpaceshipTeacher : MonoBehaviour, IPointerDownHandler
    {
        // Methods
        public void Init()
        {
            LOG.Info($"Init()", this);

            isMediaPlaying = false;

            wordTXT.text = "";
            textBTN.interactable = false;

            updateControlButtons(false);
        }
        public void Setup(ProblemData pData)//, MediaPlayer mediaPlayer 두번째 매개변수
        {
            LOG.Info($"Setup()", this);

            // this.mediaPlayer = mediaPlayer;
            // this.mediaPlayer.Events.AddListener(mediaPlayer_Event);
            this.wordClip = pData.WordCLIP;

            wordTXT.text = pData.Word;
            // displayUGUI.Player = mediaPlayer;
        }
        public Coroutine StartPlayVideo()
        {
            LOG.Info($"StartPlayVideo()", this);

            isVideoFinish = false;
            isMediaPlaying = true;

            updateControlButtons(false);

            crPlayVideo = StartCoroutine(coPlayVideo());
            return crPlayVideo;
        }
        public void PauseVideo()
        {
            LOG.Info($"PauseVideo()", this);

            this.StopCoroutineSafe(ref crPlayVideo);

            isMediaPlaying = false;

            // mediaPlayer.Pause();

            updateControlButtons(isVideoFinish);

        }
        public void StopPlayVideo()
        {
            LOG.Info($"StopPlayVideo()", this);

            this.StopCoroutineSafe(ref crPlayVideo);

            isMediaPlaying = false;

            // mediaPlayer.Stop();

            updateControlButtons(isVideoFinish);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void EnableTextNarration(bool enable)
        {
            LOG.Info($"EnableTextNarration() | {enable}", this);

            textBTN.interactable = enable;
        }

        // Events
        public event Action OnPause;
        public event Action OnResume;
        public event Action OnReplay;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        // private MediaPlayer mediaPlayer = null;
        private Coroutine crPlayVideo = null;
        private Coroutine crPlaySound = null;
        private bool isVideoReady = false;
        private bool isVideoFinish = false;
        private AudioClip wordClip = null;
        private bool isActiveControl = false;
        private bool isMediaPlaying = false;

        // Functions
        private void updateControlButtons(bool active)
        {
            isActiveControl = active;
            playBTN.gameObject.SetActive(active && !isMediaPlaying && !isVideoFinish);
            pauseBTN.gameObject.SetActive(active && isMediaPlaying && !isVideoFinish);
            replayBTN.gameObject.SetActive(active && !isMediaPlaying && isVideoFinish);

        }

        // Event Handlers
        private void mediaPlayer_Event()//MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode
        {
            // switch (et)
            // {
            //     case MediaPlayerEvent.EventType.ReadyToPlay:
            //         LOG.Important($"mediaPlayer_Event() | ReadyToPlay", this);
            //         isVideoReady = true;
            //         break;
            //     case MediaPlayerEvent.EventType.FinishedPlaying:
            //         LOG.Important($"mediaPlayer_Event() | FinishedPlaying", this);
            //         isVideoFinish = true;
            //         isMediaPlaying = false;
            //         updateControlButtons(true);
            //         break;
            // }
        }
        // Event Handlers
        private void OnTextClicked()
        {
            LOG.Function(this);

            crPlaySound = StartCoroutine(coPlaySound());
        }
        private void OnPlayClicked()
        {
            LOG.Function(this);

            OnResume?.Invoke();
        }
        private void OnPauseClicked()
        {
            LOG.Function(this);

            OnPause?.Invoke();
        }
        private void OnReplayClicked()
        {
            LOG.Function(this);

            // mediaPlayer.Control.SeekFast(0);
            OnReplay?.Invoke();
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        // [SerializeField] private DisplayUGUI displayUGUI = null;
        [SerializeField] private TextMeshProUGUI wordTXT = null;
        [SerializeField] private Button textBTN = null;
        [SerializeField] private Button playBTN = null;
        [SerializeField] private Button pauseBTN = null;
        [SerializeField] private Button replayBTN = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            updateControlButtons(false);

            textBTN.onClick.AddListener(OnTextClicked);
            playBTN.onClick.AddListener(OnPlayClicked);
            pauseBTN.onClick.AddListener(OnPauseClicked);
            replayBTN.onClick.AddListener(OnReplayClicked);
        }
        private void Start()
        {
        }



        // Unity Coroutine
        IEnumerator coPlayVideo()
        {
            using (LOG.Coroutine($"coPlayVideo", this))
            {
                yield return new WaitUntil(() => isVideoReady);

                // mediaPlayer.Play();

                yield return new WaitUntil(() => isVideoFinish);
            }
        }
        IEnumerator coPlaySound()
        {
            using (LOG.Coroutine($"coPlaySound()", this))
            {
                textBTN.interactable = false;
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(wordClip);

                textBTN.interactable = true;
                yield return null;
            }
        }

        // IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            LOG.Info($"isMediaPlaying : {isMediaPlaying}, isVideoFinish : {isVideoFinish}", this);

            updateControlButtons(!isActiveControl);
        }
    }
}