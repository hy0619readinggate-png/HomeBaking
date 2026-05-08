using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C2_A09;
using ProblemMGR = DoDoEng.Activity.C2_A09.C2_A09_ProblemMGR;

namespace DoDoEng.Activity.C2_A09
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A09_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A09_LittleStar;
        private enum State
        {
            Intro,
            Problem, Solve, Correct, Wrong, Next,
            SetComplete, NextSet, ProblemOut,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal wrongSIG_ = null;
        private TimelineSignal wrongSIG => wrongSIG_ ??= wrongTL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,           E_Intro,        X_Intro);
            fsm.AddState(State.Problem,         E_Problem,      X_Problem);
            fsm.AddState(State.Solve,           E_Solve,        X_Solve);
            fsm.AddState(State.Correct,         E_Correct,      X_Correct);
            fsm.AddState(State.Wrong,           E_Wrong,        X_Wrong);
            fsm.AddState(State.Next,            E_Next);
            fsm.AddState(State.SetComplete,     E_SetComplete,  X_SetComplete);
            fsm.AddState(State.NextSet,         E_NextSet);
            fsm.AddState(State.ProblemOut,      E_ProblemOut,   X_ProblemOut);
            fsm.AddState(State.Outro,           E_Outro,        X_Outro);
            fsm.AddState(State.Reward,          E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            constellationSet.ActiveConstellation.Setup(pMGR.Current);
            exampleStarMGR.Setup(pData.Examples);
            sailboat.Setup(pData);
            setupAff();
        }
        private void setupAff()
        {
            var exampleStarTRs = new Transform[] { exampleStarMGR.AnswerExampleStar.transform };
            var problemStarTR = new Transform[] { constellationSet.ActiveConstellation.ProblemStar.transform };
            aff.Setup(exampleStarTRs, problemStarTR);
        }
        private Constellation activeConstellation => constellationSet.ActiveConstellation;

        // Event Handlers
        private void wrongSIG_OnSignal(string signal)
        {
            LOG.Info($"s1toS2SIG_OnSignal() | {signal}", this);

            if (signal == "Activity-SetupProblem")
                exampleStarMGR.Shuffle();
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);

            exampleStarMGR.Init(exampleStarParam);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            playBGM();
            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Solve)
                CP(CheckPoint.UserFinish);

            stopBGM();
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            #pragma warning disable format
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Problem); break;
                case State.Problem: fsm.PerformTransition(State.Solve); break;
                case State.Solve:
                    var answerExampleStar = exampleStarMGR.AnswerExampleStar;
                    constellationSet.ActiveConstellation.DebugSubmit(answerExampleStar);
                    fsm.PerformTransition(State.Correct);
                    break;
                case State.SetComplete: fsm.PerformTransition(State.NextSet); break;
                case State.Correct: fsm.PerformTransition(State.Next); break;
                case State.Wrong: fsm.PerformTransition(State.Solve); break;
                case State.ProblemOut: fsm.PerformTransition(State.Intro); break;
            #pragma warning disable format
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

            if (pMGR.PNO != 1)
                constellationSet.Next();
            setupProblem(pMGR.Current);
            yield return null;

            yield return playTimeline(introTL);
            yield return null;

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            if (!pMGR.Current.FirstStar)
                setupProblem(pMGR.Current);

            setupAff();
            sailboat.Edmond.Idle();
            yield return null;

            activeConstellation.Select();
            yield return null;

            yield return playTimeline(showProblemTL);

            yield return exampleStarMGR.StartAppear();
            yield return new WaitForSeconds(1f);

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            yield return stopTimeline(showProblemTL);

            exampleStarMGR.FinishAppear();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);

            sailboat.Edmond.Idle();
            yield return null;

            activeConstellation.EnableInteraction(true);
            exampleStarMGR.EnableInteraction(true);
            sailboat.EnableInteraction(true);
            yield return null;

            yield return activeConstellation.StartWaitSubmit();

            var correct = activeConstellation.IsAnswerSubmit;
            if (correct)
                fsm.PerformTransition(State.Correct);
            else fsm.PerformTransition(State.Wrong);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            AudioMGR.One.StopNarration();
            yield return null;

            activeConstellation.EnableInteraction(false);
            exampleStarMGR.EnableInteraction(false);
            sailboat.EnableInteraction(false);
            yield return null;

            activeConstellation.FinishWaitSubmit();
        }
        IEnumerator E_Correct()
        {
            var clip = UtilArray.ExtractOne(correctNarCLIP);
            AudioMGR.One.PlayEffect(clip);

            AudioMGR.One.PlayEffect(correctCLIP);
            exampleStarMGR.Correct(activeConstellation.SubmitExampleID);
            yield return null;

            sailboat.Edmond.OutOfControl();
            yield return null;

            correctTL.time = 0;
            correctTL.Play();
            yield return activeConstellation.StartCorrect();

            sailboat.Edmond.Idle();
            yield return null;

            sailboat.ShowText();
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.PhonicsCLIP);
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.WordCLIP);

            yield return exampleStarMGR.StartDisappear();

            yield return playTimeline(hideProblemTL);

            fsm.PerformTransition(State.Next);

        }
        IEnumerator X_Correct()
        {
            AudioMGR.One.StopNarration();
            yield return null;

            correctTL.time = correctTL.duration;
            correctTL.Evaluate();
            correctTL.Stop();
            yield return null;

            activeConstellation.FinishCorrect();
            exampleStarMGR.FinishDisppear();
            yield return null;

            //sailboat.Edmond.Idle();
            yield return null;

            sailboat.ShowImage();
            yield return null;

            hideProblemTL.time = correctTL.duration;
            hideProblemTL.Evaluate();
            hideProblemTL.Stop();
            yield return null;
        }
        IEnumerator E_Wrong()
        {
            ActivityProgress.One.Wrong();

            var clip = UtilArray.ExtractOne(wrongNarCLIP);
            AudioMGR.One.PlayEffect(clip);

            AudioMGR.One.PlayEffect(wrongCLIP);
            yield return null;

            exampleStarMGR.Wrong(activeConstellation.SubmitExampleID);
            yield return new WaitForSeconds(exampleStarParam.returnJumpDuration);

            yield return playTimeline(wrongTL);

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Wrong()
        {
            yield return stopTimeline(wrongTL);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return null;

            if (pMGR.Next() && !pMGR.Current.FirstStar)
            {
                fsm.PerformTransition(State.Problem);
            }
            else fsm.PerformTransition(State.SetComplete);
        }
        IEnumerator E_SetComplete()
        {
            constellationSet.ActiveConstellation.StartComplete();
            yield return playTimeline(setCompleteTL);
            yield return null;

            fsm.PerformTransition(State.NextSet);
        }
        IEnumerator X_SetComplete()
        {
            constellationSet.ActiveConstellation.FinishComplete();
            yield return stopTimeline(setCompleteTL);
            yield return null;
        }
        IEnumerator E_NextSet()
        {
            yield return null;

            if (pMGR.PNO == pMGR.Count)
                fsm.PerformTransition(State.Outro);
            else fsm.PerformTransition(State.ProblemOut);
        }
        IEnumerator E_ProblemOut()
        {
            yield return playTimeline(problemOutTL);
            yield return null;

            fsm.PerformTransition(State.Intro);
        }
        IEnumerator X_ProblemOut()
        {
            yield return stopTimeline(problemOutTL);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
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
        [Header("★ Bindings - Panels")]
        [SerializeField] private CanvasGroup[] stepPanelCG = null;
        [Header("★ Bindings")]
        [SerializeField] private Sailboat sailboat = null;
        [SerializeField] private ExampleStarMGR exampleStarMGR = null;
        [SerializeField] private ConstellationSet constellationSet = null;
        [SerializeField] private AffDrag aff = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip[] correctNarCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;
        [SerializeField] private AudioClip[] wrongNarCLIP = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector correctTL = null;
        [SerializeField] private PlayableDirector wrongTL = null;
        [SerializeField] private PlayableDirector showProblemTL = null;
        [SerializeField] private PlayableDirector hideProblemTL = null;
        [SerializeField] private PlayableDirector setCompleteTL = null;
        [SerializeField] private PlayableDirector problemOutTL = null;
        [Header("★ Timing")]
        [SerializeField] private float rewardDelay = 1f;
        [Header("★ Configs")]
        [SerializeField] private ExampleStarParam exampleStarParam = null;


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

            wrongSIG.OnSignal += wrongSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            wrongSIG.OnSignal -= wrongSIG_OnSignal;
        }
    }

    [System.Serializable]
    public class ExampleStarParam
    {
        public float returnJumpPower = 2f;
        public float returnJumpDuration = 0.3f;

        public AudioClip pickupClip = null;
        public AudioClip returnCLIP = null;
    }
}