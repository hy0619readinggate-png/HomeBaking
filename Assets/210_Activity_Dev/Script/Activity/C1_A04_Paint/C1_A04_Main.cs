using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

using ActivityData = DoDoEng.Common.ActivityData_C1_A04;
using ProblemMGR = DoDoEng.Activity.C1_A04.C1_A04_ProblemMGR;

namespace DoDoEng.Activity.C1_A04
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A04_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C1_A04_Paint;
        private enum State
        {
            Intro, Solve, Feedback, ToPaint,
            Paint,
            Outro, Reward
        }



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal outroSIG_ = null;
        private TimelineSignal outroSIG => outroSIG_ ??= outroTL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;

        // Fields
        private Example submitExam = null;
        private Texture2D userImage = null;
        private Tween showPaletteTween = null;
        private Coroutine crPlaySound = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Feedback,    E_Feedback,     X_Feedback);
            fsm.AddState(State.ToPaint,     E_ToPaint,      X_ToPaint);
            fsm.AddState(State.Paint,       E_Paint,        X_Paint);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(ProblemData pData)
        {
            // step1
            s1Word.Setup(pData);
            for (var i = 0; i < s1Examples.Length; i++)
                s1Examples[i].Setup(i + 1, pData.Examples[i]);

            // step2
            s2PaintBoard.Setup(pData.PaintPB);
            s2SignPost.Setup(pData);

            // step3
            s3Word.Setup(pData);
        }

        // Event Handlers
        private void s1Word_OnStateChanged(Word word, bool isPlaying)
        {
            LOG.Info($"s1Word_OnStateChanged() | {word}, {isPlaying}", this);

            if (word == s1Word)
                s1CG.interactable = !isPlaying;
        }
        private void s2SignPost_OnStateChanged(bool isPlaying)
        {
            LOG.Info($"s2SignPost_OnClick()", this);

            s2PaintBoard.EnableInteraction(!isPlaying);
            s2PaintTool.EnableInteraction(!isPlaying);
            s2SignPost.EnableInteraction(!isPlaying);
            s2DoneBTN.EnableInteraction(!isPlaying);
        }
        private void s2PaintTool_OnToolChanged(ToolType toolType, BrushInfo brushInfo)
        {
            LOG.Info($"s2PaintTool_OnToolChanged() | {toolType}, {brushInfo}", this);

            s2PaintBoard.ChangeTool(toolType, brushInfo);
        }
        private void s2PaintBoard_OnPaintStarted()
        {
            LOG.Info($"s2PaintBoard_OnPaintStarted()", this);

            showPaletteTween.Kill();
            s2PaintTool.HidePalette();
            s2UndoButtonGO.SetActive(false);

            if (!s2DoneBTN.gameObject.activeSelf)
            {
                s2DoneBTN.gameObject.SetActive(true);
                s2DoneBTN.EnableInteraction(true);
                s2PaintTool.TurnAffdance(false);
            }
        }
        private void s2PaintBoard_OnPaintEnded()
        {
            LOG.Info($"s2PaintBoard_OnPaintEnded()", this);

            showPaletteTween.Kill();
            showPaletteTween = DOVirtual.DelayedCall(
                stickerPaletteDelay,
                () =>
                {
                    s2UndoButtonGO.SetActive(true);
                    s2PaintTool.ShowPalette();
                });
        }
        private void outroSIG_OnSignal(string signal)
        {
            if (signal == "Activity-ExtraAnimation")
            {

                s3DropAcorn.DoDrop(cornCount);

                crPlaySound = StartCoroutine(coPlaySound());

            }

        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            // step1
            s1Word.gameObject.SetActive(false);
            s1Tori.gameObject.SetActive(false);
            s1Roro.gameObject.SetActive(false);
            s1Word.EnableInteraction(false);
            s1Examples.ForEach(ex => ex.EnableInteraction(false));

            // step2
            s2PaintBoard.EnableInteraction(false);
            s2SignPost.EnableInteraction(false);
            s2PaintTool.EnableInteraction(false);
            s2DoneBTN.EnableInteraction(false);
            s2DoneBTN.gameObject.SetActive(false);

            // step3
            s3Word.EnableInteraction(false);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // step1
            evaluateTimeline(introTL);

            // init
            s1CG.gameObject.SetActive(true);
            s2CG.gameObject.SetActive(false);
            s3CG.gameObject.SetActive(false);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            Input.multiTouchEnabled = false;

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);

            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Solve ||
                fsm.CurrentState == State.Paint)
                CP(CheckPoint.UserFinish);

            Input.multiTouchEnabled = true;

            stopBGM();
            AudioMGR.One.StopAmbient(true);

            fsm?.StopFSM();
        }
        protected override void onPause()
        {
            s2PaintBoard.EnableInteraction(false);
        }
        protected override void onResume()
        {
            if (fsm.CurrentState == State.Paint)
                s2PaintBoard.EnableInteraction(true);
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    submitExam = s1Examples.Single(s => s.IsAnswer);
                    fsm.PerformTransition(State.Feedback);
                    break;
                case State.Feedback: fsm.PerformTransition(State.ToPaint); break;
                case State.ToPaint: fsm.PerformTransition(State.Paint); break;
                case State.Paint: fsm.PerformTransition(State.Outro); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);
            yield return playTimeline(introTL);
            yield return null;

            yield return s1Word.PlayWordAnimationAndWait();
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;

            s1Word.ShowLastFrame();
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            s1Word.EnableInteraction(true);
            s1Examples.ForEach(ex => ex.EnableInteraction(true));
            var wait = new WaitForSubmit(this, s1Examples);
            yield return wait;

            submitExam = wait.Submited as Example;
            yield return null;

            fsm.PerformTransition(State.Feedback);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            s1Word.EnableInteraction(false);
            s1Examples.ForEach(ex => ex.EnableInteraction(false));
            yield return null;

            s1Word.ShowLastFrame();
            yield return null;
        }
        IEnumerator E_Feedback()
        {
            var correct = submitExam.IsAnswer;

            if (!correct)
                ActivityProgress.One.Wrong();

            submitExam.Feedback(correct);
            yield return null;


            var isSubmitTori = submitExam.ID == 1;
            if (isSubmitTori)
            {
                var feedbackClips = correct ? toriCorrectCLIP : toriWrongCLIP;
                var clip = UtilArray.ExtractOne(feedbackClips);
                AudioMGR.One.PlayEffect(clip);

                yield return s1Tori.PlayAnimationAndWait(correct
                ? CharacterAnimation.Correct
                : CharacterAnimation.Wrong);
            }
            else
            {
                var feedbackClips = correct ? roroCorrectCLIP : roroWrongCLIP;
                var clip = UtilArray.ExtractOne(feedbackClips);
                AudioMGR.One.PlayEffect(clip);

                yield return s1Roro.PlayAnimationAndWait(correct
                ? CharacterAnimation.Correct
                : CharacterAnimation.Wrong);
            }
            yield return new WaitForSeconds(0.8f);

            if (!correct)
                submitExam.Clear();

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return new WaitForSeconds(feedbackDelay);

            fsm.PerformTransition(correct ? State.ToPaint : State.Solve);
        }
        IEnumerator X_Feedback()
        {
            s1Examples.ForEach(ex => ex.Clear());
            yield return null;

            var submitCharacter = submitExam.ID == 1 ? s1Tori : s1Roro;
            submitCharacter.AbortAnimation();
            yield return null;
        }
        IEnumerator E_ToPaint()
        {
            yield return playTimeline(toPaintTL);
            yield return null;

            fsm.PerformTransition(State.Paint);
        }
        IEnumerator X_ToPaint()
        {
            yield return stopTimeline(toPaintTL);
            yield return null;
        }
        IEnumerator E_Paint()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            s2PaintBoard.EnableInteraction(true);
            s2SignPost.EnableInteraction(true);
            s2PaintTool.EnableInteraction(true);
            s2PaintTool.SelectDefault();
            yield return null;

            var wait = new WaitForSubmit(this, s2DoneBTN);
            yield return wait;

            fsm.PerformTransition(State.Outro);
        }
        IEnumerator X_Paint()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            s2SignPost.StopWordSound();
            yield return null;

            s2PaintBoard.EnableInteraction(false);
            s2SignPost.EnableInteraction(false);
            s2PaintTool.EnableInteraction(false);
            s2DoneBTN.EnableInteraction(false);
            yield return null;

            showPaletteTween.Kill();
            s2PaintTool.HidePalette();
            yield return null;

            userImage = s2PaintBoard.UserImage;
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            s3ImageRI.texture = userImage;
            yield return null;

            yield return playTimeline(outroTL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            AudioMGR.One.StopNarration();
            this.StopCoroutineSafe(ref crPlaySound);
            yield return stopTimeline(outroTL);
            yield return null;
        }
        IEnumerator E_Reward()
        {
            s3Word.ShowLastFrame();
            yield return null;

            CP(CheckPoint.Complete);
            yield return null;

            AudioMGR.One.PlayEffect(SfxMoment.Activity_Complete);
            yield return new WaitForSeconds(rewardDelay);

            complete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings - S1.Intro")]
        [SerializeField] private CanvasGroup s1CG = null;
        [SerializeField] private Word s1Word = null;
        [SerializeField] private CharacterAni s1Tori = null;
        [SerializeField] private CharacterAni s1Roro = null;
        [SerializeField] private Example[] s1Examples = null;
        [Header("★ Bindings - S2.Paint")]
        [SerializeField] private CanvasGroup s2CG = null;
        [SerializeField] private PaintTool s2PaintTool = null;
        [SerializeField] private PaintBoard s2PaintBoard = null;
        [SerializeField] private SignPost s2SignPost = null;
        [SerializeField] private SubmitButton s2DoneBTN = null;
        [SerializeField] private GameObject s2UndoButtonGO = null;
        [Header("★ Bindings - S3.Outro")]
        [SerializeField] private CanvasGroup s3CG = null;
        [SerializeField] private RawImage s3ImageRI = null;
        [SerializeField] private Word s3Word = null;
        [SerializeField] private DropAcorn s3DropAcorn = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector toPaintTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float stickerPaletteDelay = 0.7f;
        [SerializeField] private float feedbackDelay = 0.5f;
        [SerializeField] private float rewardDelay = 1f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] toriCorrectCLIP = null;
        [SerializeField] private AudioClip[] toriWrongCLIP = null;
        [SerializeField] private AudioClip[] roroCorrectCLIP = null;
        [SerializeField] private AudioClip[] roroWrongCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ Corn")]
        [SerializeField] private int cornCount = 70;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            initFSM();
        }
        protected override void Start()
        {

        }
        protected override void OnEnable()
        {
            base.OnEnable();

            s1Word.OnStateChanged += s1Word_OnStateChanged;
            s2SignPost.OnStateChanged += s2SignPost_OnStateChanged;
            s2PaintTool.OnToolChanged += s2PaintTool_OnToolChanged;
            s2PaintBoard.OnPaintStarted += s2PaintBoard_OnPaintStarted;
            s2PaintBoard.OnPaintEnded += s2PaintBoard_OnPaintEnded;

            outroSIG.OnSignal += outroSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            s1Word.OnStateChanged -= s1Word_OnStateChanged;
            s2SignPost.OnStateChanged -= s2SignPost_OnStateChanged;
            s2PaintTool.OnToolChanged -= s2PaintTool_OnToolChanged;
            s2PaintBoard.OnPaintStarted -= s2PaintBoard_OnPaintStarted;
            s2PaintBoard.OnPaintEnded -= s2PaintBoard_OnPaintEnded;

            outroSIG.OnSignal -= outroSIG_OnSignal;
        }

        // Unity Coroutine
        IEnumerator coPlaySound()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhoneticCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
        }

    }
}