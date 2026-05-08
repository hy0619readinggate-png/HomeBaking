using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using SRDebugger;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.PlayAll
{
    public class EBookPlayAll : EBookPlayAllBase
    {
        // Definitions
        private enum State
        {
            Intro,
            Load,
            Read, NextSentence, NextLayer, NextBook, Wait, Goto,
            Outro, Reward
        }

        // Properties
        public bool IsSubtitle
        {
            get => isSubtitle;
            set
            {
                isSubtitle = value;
                bookController.ShowAllSentences(isSubtitle);

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

                if (isAutoMode && fsm.CurrentState == State.Wait && existLayer(Direction.Next))
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

        // Methods
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
        public void MoveBookToOrigin()
        {
            LOG.Function(this);

            bookController.MoveToOrigin();
        }
        public void MoveBookTo(Transform parentTR)
        {
            LOG.Function(this, $"{parentTR.gameObject.name}");

            bookController.MoveTo(parentTR);
        }
        public void ChangeBook(int id)
        {
            LOG.Function(this, $"{id}");

            cBookNO = id;
            fsm.PerformTransition(State.Load);
        }
        public void UpdateNavigationControl()
        {
            LOG.Function(this);

            updateNavigationControl(!isAutoMode);
        }




        // Fields
        private FSM<State> fsm = null;
        private int cLayerNO = 1;
        private int cSentenceNO = 1;
        private int cLayerTotal = 1;
        private int cBookNO = 1;
        private Direction gotoDirection;

        // Fields
        private bool isSubtitle = true;
        private bool isPaused = false;
        private bool isAutoMode = true;
        private float playbackSpeed = 1.0f;

        // Fields
        private AudioClip[] recordCLIPs = null;
        private Coroutine crState = null;

        // Fields
        private GameObject BOOK_PB = null;
        private LayerData[] LAYERS = null;
        public string Title { get; private set; }

        // Fields : caching
        private BookLoader bookLoader = new BookLoader();

        // Functions : properties
        new protected EBookPlayAllIndex EBIndex => base.EBIndex as EBookPlayAllIndex;
        private EBookSingleIndex cBookIndex => EBIndex.GetEBookIndex(cBookNO - 1);

        // Functions : properties
        private LayerData cData => LAYERS[cLayerNO - 1];
        private SentenceData cSentence => cData.Sentences[cSentenceNO - 1];

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro, E_Intro, X_Intro);
            fsm.AddState(State.Load, E_Load, X_Load);
            fsm.AddState(State.Read, E_Read, X_Read);
            fsm.AddState(State.NextSentence, E_NextSentence);
            fsm.AddState(State.NextLayer, E_NextLayer);
            fsm.AddState(State.NextBook, E_NextBook);
            fsm.AddState(State.Wait, E_Wait, X_Wait);
            fsm.AddState(State.Goto, E_Goto, X_Goto);
            fsm.AddState(State.Outro, E_Outro, X_Outro);
            fsm.AddState(State.Reward, E_Reward);
#pragma warning restore format
        }

        // Functions
        private async UniTask loadBook(EBookSingleIndex ebIDX)
        {
            await bookLoader.LoadBook(ebIDX);

            BOOK_PB = bookLoader.BOOK_PB;
            LAYERS = bookLoader.LAYERS;
            Title = bookLoader.CURRICULUM.Title;
        }
        private async UniTask<MenuItemData[]> loadMenuData(EBookPlayAllIndex ebIDX)
        {
            var list = new List<MenuItemData>();

            for (var i = 0; i < ebIDX.Count; i++)
            {
                var idx = ebIDX.GetEBookIndex(i);
                var spr = await idx.LoadThumbnail();
                list.Add(new MenuItemData
                {
                    ID = i + 1,
                    ThumnailSPR = spr
                });

            }
            return list.ToArray();
        }
        private async UniTask setupBook(EBookSingleIndex ebIDX)
        {
            bookController.Setup(BOOK_PB, LAYERS, !IsNativeMode);

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

            menu.gameObject.SetActive(true);
            menu.SetCurrentBook(cBookNO);

            //isSubtitle = true;    // 유지
            isPaused = false;
            //isAutoMode = true;    // 유지

            if (!IsNativeMode)
                recordCLIPs = await LMS.One.LoadAudioRecords(int.Parse(ebIDX.Index), bookLoader.RECORDS.Length);
        }

        // Functions
        private bool existLayer(Direction direction)
        {
            var layerNew = cLayerNO + (direction == Direction.Next ? 1 : -1);
            return layerNew >= 1 && layerNew <= cLayerTotal;
        }
        private bool existBook(int bookNO)
        {
            var bookNew = bookNO;
            return bookNew >= 1 && bookNew <= EBIndex.Count;
        }
        private void updateNavigationControl(bool show)
        {
            var canPrev = cLayerNO > 1;
            var canNext = cLayerNO < cLayerTotal;

            prevBTN.gameObject.SetActive(show && canPrev && (!menu.IsShown || menu.IsLocked));
            nextBTN.gameObject.SetActive(show && canNext && (!menu.IsShown || menu.IsLocked));
            prevInMenuBTN.gameObject.SetActive(show && canPrev && menu.IsShown);
            nextInMenuBTN.gameObject.SetActive(show && canNext && menu.IsShown);

            prevBTN.interactable = canPrev;
            nextBTN.interactable = canNext;
            prevInMenuBTN.interactable = canPrev;
            nextInMenuBTN.interactable = canNext;
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
        protected override async UniTask onPrepare(EBookPlayAllIndex ebIDX)
        {
            await base.onPrepare(ebIDX);

            IsNativeMode = ebIDX.EBookMode == EBookReadMode.Native;

            var flag = AssetLoadFlag.BookPrefab;
            if (IsNativeMode)
                flag |= AssetLoadFlag.LayerNarration;

            bookLoader.SetAssetLoadFlag(flag);

            var datas = await loadMenuData(ebIDX);
            menu.Setup(this, datas);
            await loadBook(cBookIndex);
            await setupBook(cBookIndex);
        }
        protected override void onStartEBook()
        {
            base.onStartEBook();

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
#if !DISABLE_SRDEBUGGER
        protected override void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            base.onBuildOption(srOptionContainer);
        }
#endif



        // FSM
        IEnumerator E_Intro()
        {
            cBookNO = 1;
            yield return null;

            fsm.PerformTransition(State.Read);
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Load()
        {
            menu.EnableInteraction(false);
            gesture.EnableInteraction(false);
            bookController.ShowLoading();
            crState = StartCoroutine(loadBook(cBookIndex).ToCoroutine());
            yield return crState;
            yield return setupBook(cBookIndex);

            fsm.PerformTransition(State.Read);
        }
        IEnumerator X_Load()
        {
            menu.EnableInteraction(true);
            gesture.EnableInteraction(true);
            bookController.HideLoading();
            this.StopCoroutineSafe(ref crState);
            yield return null;
        }
        IEnumerator E_Read()
        {
            LOG.Important($"Layer {cLayerNO}", this);

            gesture.EnableInteraction(true);
            if (IsNativeMode)
                crState = StartCoroutine(coPlayNative());
            else crState = StartCoroutine(coPlayMyVoice());
            yield return crState;

            fsm.PerformTransition(State.NextSentence);
        }
        IEnumerator X_Read()
        {
            gesture.EnableInteraction(false);
            this.StopCoroutineSafe(ref crState);
            AudioMGR.One.StopNarration();
            bookController.StopHilight();
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
            else fsm.PerformTransition(State.NextBook);
        }
        IEnumerator E_NextBook()
        {
            cBookNO++;
            PlaybackSpeed = 1.0f;
            if (existBook(cBookNO))
                fsm.PerformTransition(State.Load);
            else fsm.PerformTransition(State.Outro);
            yield return null;
        }
        IEnumerator E_Wait()
        {
            gesture.EnableInteraction(true);
            yield return null;
        }
        IEnumerator X_Wait()
        {
            gesture.EnableInteraction(false);
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

            bookController.MoveLayer(gotoDirection);
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
            complete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private BookController bookController = null;
        [SerializeField] private PageIndicator pageIndicator = null;
        [SerializeField] private PageIndicator menuPageIndicator = null;
        [SerializeField] private Menu menu = null;
        [SerializeField] private Gesture gesture = null;
        [SerializeField] private Button prevBTN = null;
        [SerializeField] private Button nextBTN = null;
        [SerializeField] private Button prevInMenuBTN = null;
        [SerializeField] private Button nextInMenuBTN = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pageFlipCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float myVoiceSentenceDelay = 1f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            prevBTN.onClick.AddListener(prevBTN_OnClick);
            nextBTN.onClick.AddListener(nextBTN_OnClick);

            prevInMenuBTN.onClick.AddListener(prevBTN_OnClick);
            nextInMenuBTN.onClick.AddListener(nextBTN_OnClick);

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
                bookController.PlayHilight(cSentence);
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
                var records = bookLoader.RECORDS.Where(r => r.LayerNo == cLayerNO && r.SentenceNo == cSentenceNO);
                foreach (var r in records)
                {
                    bookController.HilightOn(r);
                    var idx = bookLoader.RECORDS.FindIndex(r);
                    if (recordCLIPs == null || recordCLIPs[idx] == null)
                        yield return new WaitForSeconds(1f);
                    else yield return AudioMGR.One.PlayNarrationAndWait(recordCLIPs[idx]);

                    yield return new WaitForSeconds(myVoiceSentenceDelay);
                }
            }
        }
    }
}