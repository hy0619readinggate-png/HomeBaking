using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.C2_A12;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

using ActivityData = DoDoEng.Common.ActivityData_C2_A02;
using ProblemMGR = DoDoEng.Activity.C2_A02.C2_A02_ProblemMGR;

namespace DoDoEng.Activity.C2_A02
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A02_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A02_Splash;
        private enum State
        {
            S1Intro,
            S1Problem, S1Solve, S1Correct, S1Wrong, S1NextSeat, S1ToS2,
            S2TakePhoto,
            Next, S2ToS1,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal s1toS2SIG_ = null;
        private TimelineSignal s1toS2SIG => s1toS2SIG_ ??= s1ToS2TL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private Coroutine crPlayWord = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.S1Intro,     E_S1Intro,      X_S1Intro);
            fsm.AddState(State.S1Problem,   E_S1Problem,    X_S1Problem);
            fsm.AddState(State.S1Solve,     E_S1Solve,      X_S1Solve);
            fsm.AddState(State.S1Correct,   E_S1Correct,    X_S1Correct);
            fsm.AddState(State.S1Wrong,     E_S1Wrong,      X_S1Wrong);
            fsm.AddState(State.S1NextSeat,  E_S1NextSeat);
            fsm.AddState(State.S1ToS2,      E_S1ToS2,       X_S1ToS2);
            fsm.AddState(State.S2TakePhoto, E_S2TakePhoto,  X_S2TakePhoto);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.S2ToS1,      E_S2ToS1,       X_S2ToS1);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            // step1
            s1EntranceMGR.Setup(pData.Examples);
            s1Boat.Setup(pData);
            s1Problem.Setup(pData.WordSPR);
        }
        private void showFallVFX(Transform posTR)
        {
            var position = new Vector3(posTR.position.x,
                                        s1VFXWrongGO.transform.position.y);
            s1VFXWrongGO.transform.SetXYOnly(position);

            s1VFXWrongGO.SetActive(false);
            s1VFXWrongGO.SetActive(true);
        }
        private void setupAff()
        {
            var answerOfBoat = s1Boat.ActiveTextAreas;
            var answer = answerOfBoat[0];
            var answerTRs = new Transform[] { answer.transform };

            var exampleTRs = s1EntranceMGR.GetAnswerEntrancesTRs(answer.Text);

            aff.Setup(exampleTRs, answerTRs);
        }

        // Event Handlers
        private void s1Problem_OnClick()
        {
            LOG.Info($"s1Problem_OnClick()", this);


            crPlayWord = StartCoroutine(coPlayWord());
        }
        private void s1toS2SIG_OnSignal(string signal)
        {
            LOG.Info($"s1toS2SIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
                AudioMGR.One.PlayEffectLL(waterCLIP, true);
        }

        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            s1EntranceMGR.Init(s1FloatTR);

            s1EntranceMGR.EnableInteraction(false);
            s1Problem.EnableInteraction(false);
            s1VFXWrongGO.SetActive(false);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);
            fsm.StartFSM(State.S1Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.S1Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);
            fsm?.StopFSM();

            AudioMGR.One.StopEffectLL();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.S1Intro: fsm.PerformTransition(State.S1Problem); break;
                case State.S1Problem: fsm.PerformTransition(State.S1Solve); break;
                //case State.S1Solve: fsm.PerformTransition(State.S1Correct); break;
                case State.S1Correct: fsm.PerformTransition(State.S1NextSeat); break;
                case State.S1ToS2: fsm.PerformTransition(State.S2TakePhoto); break;
                case State.S2TakePhoto: fsm.PerformTransition(State.Next); break;
                case State.S2ToS1: fsm.PerformTransition(State.S1Problem); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }



        // FSM
        IEnumerator E_S1Intro()
        {
            CP(CheckPoint.Start);
            yield return playTimeline(introTL, 0);
            yield return null;

            fsm.PerformTransition(State.S1Problem);
        }
        IEnumerator X_S1Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_S1Problem()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);
            yield return null;

            fsm.PerformTransition(State.S1Solve);
        }
        IEnumerator X_S1Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_S1Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            setupAff();
            yield return null;

            s1Problem.EnableInteraction(true);
            s1EntranceMGR.EnableInteraction(true);
            s1EntranceMGR.Idle();
            yield return s1Boat.StartWaitSubmit();

            if (s1Boat.IsCorrect)
                fsm.PerformTransition(State.S1Correct);
            else fsm.PerformTransition(State.S1Wrong);
        }
        IEnumerator X_S1Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            s1Problem.EnableInteraction(false);
            s1EntranceMGR.EnableInteraction(false);
            s1Boat.FinishWaitSubmit();
            yield return null;

            AudioMGR.One.StopNarration();
            this.StopCoroutineSafe(ref crPlayWord);
            yield return null;
        }
        IEnumerator E_S1Correct()
        {
            var exampleTextID = s1Boat.SubmitExampleText.ID;
            var characterID = s1EntranceMGR.GetCharacterID(exampleTextID);
            s1Boat.Correct(characterID);
            s1EntranceMGR.Correct(exampleTextID);
            yield return playTimeline(stepUpTL[exampleTextID - 1], 0);
            yield return s1Boat.StartRide(characterID);

            fsm.PerformTransition(State.S1NextSeat);
            yield return null;
        }
        IEnumerator X_S1Correct()
        {
            //var exampleTextID = s1Boat.SubmitExampleText.ID;
            //yield return stopTimeline(stepUpTL[exampleTextID - 1]);

            //s1Boat.FinishRide();
            yield return null;
        }
        IEnumerator E_S1Wrong()
        {
            ActivityProgress.One.Wrong();

            var exampleTextID = s1Boat.SubmitExampleText.ID;
            s1EntranceMGR.Wrong(exampleTextID);
            s1Boat.SubmitExampleText.Wrong();
            s1Boat.Wrong();
            yield return new WaitForSeconds(wrongDuration * 0.5f);

            showFallVFX(s1Boat.SubmitExampleText.transform);
            yield return new WaitForSeconds(wrongDuration * 0.5f);

            s1Boat.SubmitExampleText.Respawn();
            yield return new WaitForSeconds(0.5f);

            fsm.PerformTransition(State.S1Solve);
        }
        IEnumerator X_S1Wrong()
        {
            yield return null;
        }
        IEnumerator E_S1NextSeat()
        {
            yield return null;

            if (s1Boat.IsComplete)
                fsm.PerformTransition(State.S1ToS2);
            else fsm.PerformTransition(State.S1Solve);
        }
        IEnumerator E_S1ToS2()
        {
            s1PhotoBoat.Setup(s1Boat.CharacterIDs);
            s2TakePhoto.Setup(s1Boat.CharacterIDs);
            yield return null;

            yield return playTimeline(s1ToS2TL);
            yield return null;

            fsm.PerformTransition(State.S2TakePhoto);
        }
        IEnumerator X_S1ToS2()
        {
            yield return stopTimeline(s1ToS2TL);
            yield return null;
        }
        IEnumerator E_S2TakePhoto()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            s2TakePhoto.EnableInteraction(true);
            yield return s2TakePhoto.StartWaitTakePhoto();
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_S2TakePhoto()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            AudioMGR.One.StopEffectLL();
            s2TakePhoto.EnableInteraction(false);
            s2TakePhoto.FinishWaitTakePhoto();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next())
                fsm.PerformTransition(State.S2ToS1);
            else fsm.PerformTransition(State.Outro);
        }
        IEnumerator E_S2ToS1()
        {
            setupProblem(pMGR.Current);
            yield return null;

            s1CaptureIMG.sprite = s2TakePhoto.Captured;
            yield return playTimeline(s2ToS1TL);
            yield return null;

            fsm.PerformTransition(State.S1Problem);
        }
        IEnumerator X_S2ToS1()
        {
            yield return stopTimeline(s2ToS1TL);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            s1CaptureIMG.sprite = s2TakePhoto.Captured;
            yield return playTimeline(outroTL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return stopTimeline(outroTL);
            yield return null;
        }
        IEnumerator E_Reward()
        {
            CP(CheckPoint.Complete);
            yield return null;

            AudioMGR.One.PlayEffect(SfxMoment.Activity_Complete);
            yield return new WaitForSeconds(rewardDelay);

            complete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings - S1.Main")]
        [SerializeField] private S1EntranceMGR s1EntranceMGR = null;
        [SerializeField] private S1Boat s1Boat = null;
        [SerializeField] private S1Problem s1Problem = null;
        [SerializeField] private Transform s1FloatTR = null;
        [SerializeField] private S2Boat s1PhotoBoat = null;
        [SerializeField] private GameObject s1VFXWrongGO = null;
        [SerializeField] private AffDrag aff = null;
        [Header("★ Bindings - S2.Capture")]
        [SerializeField] private S2TakePhoto s2TakePhoto = null;
        [SerializeField] private Image s1CaptureIMG = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector[] stepUpTL = null;
        [SerializeField] private PlayableDirector s1ToS2TL = null;
        [SerializeField] private PlayableDirector s2ToS1TL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float wrongDuration = 1f;
        [SerializeField] private float rewardDelay = 1f;
        [Header("★ Sound")]
        [SerializeField] private AudioClip waterCLIP = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;

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

            s1Problem.OnClick += s1Problem_OnClick;
            s1toS2SIG.OnSignal += s1toS2SIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            s1Problem.OnClick -= s1Problem_OnClick;
            s1toS2SIG.OnSignal -= s1toS2SIG_OnSignal;
        }

        // Unity Coroutine
        IEnumerator coPlayWord()
        {
            using (LOG.Coroutine($"coPlayWord()", this))
            {
                s1Problem.EnableInteraction(false);
                s1EntranceMGR.EnableInteraction(false);
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

                s1Problem.EnableInteraction(true);
                s1EntranceMGR.EnableInteraction(true);
                yield return null;
            }
        }
    }
}