using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C4_A03;
using ProblemMGR = DoDoEng.Activity.C3_A05.C4_A03_ProblemMGR;

// Variation : C3_A05_GrilledSkewers, C4_A03_IceFishing
namespace DoDoEng.Activity.C3_A05
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C4_A03_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C4_A03_IceFishing;
        private enum State
        {
            Intro,
            ProblemIn, Problem, Solve, Correct1, Correct2, Wrong, Next,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal problemSIG_ = null;
        private TimelineSignal problemSIG => problemSIG_ ??= problemTL.GetComponent<TimelineSignal>();
        private TimelineSignal correct1UpSIG_ = null;
        private TimelineSignal correct1UpSIG => correct1UpSIG_ ??= correct1TL[0].GetComponent<TimelineSignal>();
        private TimelineSignal correct1DownSIG_ = null;
        private TimelineSignal correct1DownSIG => correct1DownSIG_ ??= correct1TL[1].GetComponent<TimelineSignal>();
        private TimelineSignal correct2UpSIG_ = null;
        private TimelineSignal correct2UpSIG => correct2UpSIG_ ??= correct2TL[0].GetComponent<TimelineSignal>();
        private TimelineSignal correct2DownSIG_ = null;
        private TimelineSignal correct2DownSIG => correct2DownSIG_ ??= correct2TL[1].GetComponent<TimelineSignal>();
        private TimelineSignal wrongUpSIG_ = null;
        private TimelineSignal wrongUpSIG => wrongUpSIG_ ??= wrongTL[0].GetComponent<TimelineSignal>();
        private TimelineSignal wrongDownSIG_ = null;
        private TimelineSignal wrongDownSIG => wrongDownSIG_ ??= wrongTL[1].GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;
        private Example submitedExam;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.ProblemIn,   E_ProblemIn,    X_ProblemIn);
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Correct1,    E_Correct1,     X_Correct1);
            fsm.AddState(State.Correct2,    E_Correct2,     X_Correct2);
            fsm.AddState(State.Wrong,       E_Wrong,        X_Wrong);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            problemBoard.Setup(pData);
            examples.ForEach((i, e) => e.Setup(pData.Examples[i]));
        }

        // Functions
        private PlayableDirector correctTL1 => correct1TL[submitedExam.ID - 1];
        private PlayableDirector correctTL2 => correct2TL[submitedExam.ID - 1];
        private PlayableDirector wrongTL1 => wrongTL[submitedExam.ID - 1];

        // Event Handlers
        private void problemSIG_OnSignal(string signal)
        {
            LOG.Info($"problemSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-SetupProblem")
                goma.TakeOut(pMGR.Current.IngredientID);
        }
        private void correct1SIG_OnSignal(string signal)
        {
            LOG.Info($"correct1SIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
                goma.Grilled(pMGR.Current.IngredientID);
        }
        private void correct2SIG_OnSignal(string signal)
        {
            LOG.Info($"correct2SIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
                goma.Empty();
        }
        private void wrongSIG_OnSignal(string signal)
        {
            LOG.Info($"wrongSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
            {
                var clip = UtilArray.ExtractOne(wrongClip);
                AudioMGR.One.PlayEffect(clip);
                goma.Wrong(pMGR.Current.IngredientID);
            }
            else if (signal == "Activity-SetupProblem")
                goma.TakeOut(pMGR.Current.IngredientID);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            UIActivityCommon.One.VisibleSpeakerButton = true;
            examples.AutoFillID();
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            evaluateTimeline(introTL);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);

            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);

            fsm?.StopFSM();
        }
        protected override void onSpeaker()
        {
            base.onSpeaker();

            if (fsm.CurrentState == State.Solve)
            {
                AudioMGR.One.PlayNarration(pMGR.Current.SentenceCLIP);
            }
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.ProblemIn); break;
                case State.ProblemIn: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    submitedExam = examples.First(e => e.IsAnswer);
                    fsm.PerformTransition(State.Correct1);
                    break;
                case State.Correct1: fsm.PerformTransition(State.Correct2); break;
                case State.Correct2: fsm.PerformTransition(State.Next); break;
                case State.Wrong: fsm.PerformTransition(State.Solve); break;
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

            fsm.PerformTransition(State.ProblemIn);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_ProblemIn()
        {
            setupProblem(pMGR.Current);
            goma.Empty();
            yield return null;

            yield return playTimeline(problemTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_ProblemIn()
        {
            yield return stopTimeline(problemTL);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = true;
            problemBoard.EnableInteraction(true);
            examples.ForEach(e => e.EnableInteraction(true));
            var wait = new WaitForSubmit(this, examples);
            yield return wait;

            problemBoard.EnableInteraction(false);
            examples.ForEach(e => e.EnableInteraction(false));
            yield return null;

            submitedExam = wait.Submited as Example;
            if (submitedExam.IsAnswer)
                fsm.PerformTransition(State.Correct1);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            UIActivityCommon.One.EnableSpeakerButton = false;
            problemBoard.EnableInteraction(false);
            problemBoard.AbortPlaySound();
            examples.ForEach(e => e.EnableInteraction(false));
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Correct1()
        {
            submitedExam.Correct();
            yield return new WaitForSeconds(0.5f);

            yield return playTimeline(correctTL1);

            fsm.PerformTransition(State.Correct2);
        }
        IEnumerator X_Correct1()
        {
            yield return stopTimeline(correctTL1);
        }
        IEnumerator E_Correct2()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);

            yield return playTimeline(correctTL2);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Correct2()
        {
            AudioMGR.One.StopNarration();

            yield return stopTimeline(correctTL2);
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();

            yield return playTimeline(wrongTL1);

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Wrong()
        {
            yield return stopTimeline(wrongTL1);
        }
        IEnumerator E_Next()
        {
            if (pMGR.Next())
                fsm.PerformTransition(State.ProblemIn);
            else fsm.PerformTransition(State.Outro);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

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
        [Header("★ Bindings")]
        [SerializeField] private ProblemBoard problemBoard = null;
        [SerializeField] private Example[] examples = null;
        [SerializeField] private Goma goma = null;
        [Header("★ Auiods")]
        [SerializeField] private AudioClip[] wrongClip = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector problemTL = null;
        [SerializeField] private PlayableDirector[] correct1TL = null;
        [SerializeField] private PlayableDirector[] correct2TL = null;
        [SerializeField] private PlayableDirector[] wrongTL = null;
        [SerializeField] private PlayableDirector outroTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;

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

            problemSIG.OnSignal += problemSIG_OnSignal;
            correct1UpSIG.OnSignal += correct1SIG_OnSignal;
            correct1DownSIG.OnSignal += correct1SIG_OnSignal;
            correct2UpSIG.OnSignal += correct2SIG_OnSignal;
            correct2DownSIG.OnSignal += correct2SIG_OnSignal;
            wrongUpSIG.OnSignal += wrongSIG_OnSignal;
            wrongDownSIG.OnSignal += wrongSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            problemSIG.OnSignal -= problemSIG_OnSignal;
            correct1UpSIG.OnSignal -= correct1SIG_OnSignal;
            correct1DownSIG.OnSignal -= correct1SIG_OnSignal;
            correct2UpSIG.OnSignal -= correct2SIG_OnSignal;
            correct2DownSIG.OnSignal -= correct2SIG_OnSignal;
            wrongUpSIG.OnSignal -= wrongSIG_OnSignal;
            wrongDownSIG.OnSignal -= wrongSIG_OnSignal;
        }
    }
}