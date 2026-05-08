using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Read
{
    public class EBookRead : EBookSingleBase
    {
        // Definitions
        private enum State
        {
            Intro,
            Read, NextSentence, NextLayer, Wait, Goto,
            Outro, Reward,
            Halt
        }

        // Properties
        public bool IsSubtitle
        {
            get => isSubtitle;
            set
            {
                isSubtitle = value;
                layerController.ShowAllSentences(isSubtitle);

                SystemUI.One.ShowToastMessage(isSubtitle
                    ? LocalizationMGR.One.GetText("MESSAGE_21")
                    : LocalizationMGR.One.GetText("MESSAGE_20"));
            }
        }
        public bool IsPaused
        {
            get => isPaused;
            set
            {
                isPaused = value;
                if (isPaused)
                {
                    Time.timeScale = 0;
                    AudioMGR.One.Pitch = 0;

                    AudioListener.pause = true;
                }
                else
                {
                    Time.timeScale = playbackSpeed;
                    AudioMGR.One.Pitch = playbackSpeed;

                    AudioListener.pause = false;
                }
            }
        }
        public bool IsAutoMode
        {
            get => isAutoMode;
            set
            {
                isAutoMode = value;
                updateNavigationControl(!isAutoMode);

                SystemUI.One.ShowToastMessage(isAutoMode
                    ? LocalizationMGR.One.GetText("MESSAGE_88")
                    : LocalizationMGR.One.GetText("MESSAGE_89"));

                if (isAutoMode && fsm.CurrentState == State.Wait)
                    fsm.PerformTransition(State.Goto);
            }
        }
        public float PlaybackSpeed
        {
            get => playbackSpeed;
            set
            {
                playbackSpeed = value;
                if (!isPaused)
                {
                    Time.timeScale = playbackSpeed;
                    AudioMGR.One.Pitch = playbackSpeed;
                }
            }
        }
        public bool IsNativeMode { get; private set; }

        // Methods : navigation
        public void MoveLayer(Direction direction)
        {
            LOG.Info($"MoveLayer() | {direction}", this);

            if (fsm.CurrentState == State.Read ||
                fsm.CurrentState == State.Wait)
            {
                if (existLayer(direction))
                {
                    gotoDirection = direction;

                    fsm.PerformTransition(State.Goto);
                }
                else LOG.Warning($"Can't move layer to [{direction}]", this);
            }
        }
        public void Halt()
        {
            LOG.Function(this);
            fsm.PerformTransition(State.Halt);
        }


        // Fields
        private FSM<State> fsm = null;
        private int cLayerNO = 1;
        private int cSentenceNO = 1;
        private int cLayerTotal = 1;
        private Direction gotoDirection;

        // Fields
        private bool isSubtitle = true;
        private bool isPaused = false;
        private bool isAutoMode = true;
        private float playbackSpeed = 1.0f;

        // Fields
        private AudioClip[] recordCLIPs = null;
        private Coroutine crState = null;

        // Functions : properties
        private LayerData cData => LAYERS[cLayerNO - 1];
        private SentenceData cSentence => cData.Sentences[cSentenceNO - 1];
        new protected EBookReadIndex EBIndex => base.EBIndex as EBookReadIndex;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,           E_Intro,        X_Intro);
            fsm.AddState(State.Read,            E_Read,         X_Read);
            fsm.AddState(State.NextSentence,    E_NextSentence);
            fsm.AddState(State.NextLayer,       E_NextLayer);
            fsm.AddState(State.Wait,            E_Wait);
            fsm.AddState(State.Goto,            E_Goto,         X_Goto);
            fsm.AddState(State.Outro,           E_Outro,        X_Outro);
            fsm.AddState(State.Reward,          E_Reward);
            fsm.AddState(State.Halt,            E_Halt);
            #pragma warning restore format
        }
        private void prepare(EBookSingleIndex ebIDX)
        {
            var ebReadIDX = ebIDX as EBookReadIndex;
            if (ebReadIDX == null)
                LOG.Error($"EbookIndex must be EBookReadIndex", this);

            IsNativeMode = ebReadIDX.EBookMode == EBookReadMode.Native;

            var flag = AssetLoadFlag.BookPrefab;
            if (IsNativeMode)
                flag |= AssetLoadFlag.LayerNarration;

            setAssetLoadFlag(flag);
        }

        // Functions
        private bool existLayer(Direction direction)
        {
            var layerNew = cLayerNO + (direction == Direction.Next ? 1 : -1);
            return layerNew >= 1 && layerNew <= cLayerTotal;
        }
        private void updateNavigationControl(bool show)
        {
            var canPrev = cLayerNO > 1;
            var canNext = cLayerNO < cLayerTotal;

            prevBTN.gameObject.SetActive(show && canPrev);
            nextBTN.gameObject.SetActive(show && canNext);

            prevBTN.interactable = canPrev;
            nextBTN.interactable = canNext;
        }
        private void updateIndicator()
        {
            pageIndicator.UpdatePageNo(cLayerNO);
            menuPageIndicator.UpdatePageNo(cLayerNO);
        }

        // Event Handlers
        private void gesture_OnSwipe(Swipe swipe)
        {
            LOG.Info($"gesture_OnSwipe() | {swipe}", this);

            if (!isAutoMode)
            {
                var direction = swipe switch
                {
                    Swipe.Left => Direction.Next,
                    Swipe.Right => Direction.Prev,
                    _ => Direction.Next
                };

                MoveLayer(direction);
            }
        }
        private void prevBTN_OnClick()
        {
            LOG.Info($"prevBTN_OnClick()", this);

            gotoDirection = Direction.Prev;
            fsm.PerformTransition(State.Goto);
        }
        private void nextBTN_OnClick()
        {
            LOG.Info($"nextBTN_OnClick()", this);

            gotoDirection = Direction.Next;
            fsm.PerformTransition(State.Goto);
        }

        // Overrides
        protected override async UniTask onPrepare(EBookSingleIndex ebIDX)
        {
            prepare(ebIDX);
            await base.onPrepare(ebIDX);

            layerController.Setup(BOOK_PB, LAYERS, !IsNativeMode);

            cLayerNO = 1;
            cSentenceNO = 1;
            cLayerTotal = LAYERS.Length;

            pageIndicator.Setup(cLayerTotal);
            menuPageIndicator.Setup(cLayerTotal);

            updateNavigationControl(!isAutoMode);
            updateIndicator();

            gesture.Setup(this);
            gesture.EnableInteraction(false);
            gesture.gameObject.SetActive(true);

            menu.Setup(this);
            menu.gameObject.SetActive(true);

            if (!IsNativeMode)
                recordCLIPs = await LMS.One.LoadAudioRecords(int.Parse(ebIDX.Index), RECORDS.Length);
        }
        protected override void onStartEBook()
        {
            base.onStartEBook();

            gesture.EnableInteraction(true);
            AudioMGR.One.PlayBGM(bgmCLIP);
            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishEBook()
        {
            base.onFinishEBook();

            fsm?.StopFSM();
            AudioMGR.One.StopAll();
            gesture.EnableInteraction(false);
            PlaybackSpeed = 1.0f;
        }
        protected override void onDebugMoveLayer(Direction direction)
        {
            if (fsm.CurrentState != State.Read &&
                fsm.CurrentState != State.Wait)
                return;

            MoveLayer(direction);
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Outro);
        }



        // FSM
        IEnumerator E_Intro()
        {
            yield return null;

            fsm.PerformTransition(State.Read);
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Read()
        {
            LOG.Important($"Layer {cLayerNO}", this);

            if (IsNativeMode)
                crState = StartCoroutine(coPlayNative());
            else crState = StartCoroutine(coPlayMyVoice());
            yield return crState;

            fsm.PerformTransition(State.NextSentence);
        }
        IEnumerator X_Read()
        {
            this.StopCoroutineSafe(ref crState);
            AudioMGR.One.StopNarration();
            layerController.StopHilight();
            yield return null;
        }
        IEnumerator E_NextSentence()
        {
            yield return null;

            cSentenceNO++;
            if (cSentenceNO <= cData.Sentences.Length)
                fsm.PerformTransition(State.Read);
            else fsm.PerformTransition(State.NextLayer);
        }
        IEnumerator E_NextLayer()
        {
            yield return null;

            if (existLayer(Direction.Next))
            {
                if (isAutoMode)
                {
                    gotoDirection = Direction.Next;
                    fsm.PerformTransition(State.Goto);
                }
                else fsm.PerformTransition(State.Wait);
            }
            else
            {
                PlaybackSpeed = 1.0f;
                fsm.PerformTransition(State.Outro);
            }
        }
        IEnumerator E_Wait()
        {
            yield return null;
        }
        IEnumerator E_Goto()
        {
            updateNavigationControl(false);
            yield return null;

            AudioMGR.One.PlayEffect(pageFlipCLIP);
            yield return null;

            cLayerNO += gotoDirection == Direction.Next ? +1 : -1;
            cSentenceNO = 1;

            layerController.MoveLayer(gotoDirection);
            yield return new WaitForSeconds(0.7f);

            updateIndicator();
            yield return null;

            fsm.PerformTransition(State.Read);
        }
        IEnumerator X_Goto()
        {
            updateNavigationControl(!isAutoMode);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return null;
        }
        IEnumerator E_Reward()
        {
            backButtonGO.SetActive(false);
            yield return null;

            complete();
            yield return null;
        }
        IEnumerator E_Halt()
        {
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private BookController layerController = null;
        [SerializeField] private PageIndicator pageIndicator = null;
        [SerializeField] private PageIndicator menuPageIndicator = null;
        [SerializeField] private Menu menu = null;
        [SerializeField] private Gesture gesture = null;
        [SerializeField] private Button prevBTN = null;
        [SerializeField] private Button nextBTN = null;
        [SerializeField] private GameObject backButtonGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip bgmCLIP = null;
        [SerializeField] private AudioClip pageFlipCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float myVoiceSentenceDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            prevBTN.onClick.AddListener(prevBTN_OnClick);
            nextBTN.onClick.AddListener(nextBTN_OnClick);

            initFSM();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            gesture.OnSwipe += gesture_OnSwipe;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            gesture.OnSwipe -= gesture_OnSwipe;
        }

        // Unity Coroutine
        IEnumerator coPlayNative()
        {
            using (LOG.Coroutine($"coPlayNative()", this))
            {
                layerController.PlayHilight(cSentence);
                yield return AudioMGR.One.PlayNarrationAndWait(cSentence.SoundCLIP);

                // #704 대응 수정
                //layerController.StopHilight();
                yield return null;
            }
        }
        IEnumerator coPlayMyVoice()
        {
            using (LOG.Coroutine($"coPlayMyVoice()", this))
            {
                var records = RECORDS.Where(r => r.LayerNo == cLayerNO && r.SentenceNo == cSentenceNO);
                foreach (var r in records)
                {
                    layerController.HilightOn(r);
                    var idx = RECORDS.FindIndex(r);
                    if (recordCLIPs == null || recordCLIPs[idx] == null)
                        yield return new WaitForSeconds(1f);
                    else yield return AudioMGR.One.PlayNarrationAndWait(recordCLIPs[idx]);

                    yield return new WaitForSeconds(myVoiceSentenceDelay);
                }
            }
        }
    }
}