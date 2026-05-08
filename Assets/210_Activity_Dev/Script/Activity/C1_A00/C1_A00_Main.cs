using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C1_A00;
using ProblemMGR = DoDoEng.Activity.C1_A00.C1_A00_ProblemMGR;

// devBOX - 삭제요망
#pragma warning disable 0414

namespace DoDoEng.Activity.C1_A00
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_A00_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.NA;
        private enum State { Intro, Solve, Feedback, Outro, Reward }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro,        X_Intro);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.Feedback,    E_Feedback,     X_Feedback);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            // step1

            // step2

            // step3
        }

        // Event Handlers
        private void button_OnClick()
        {
            LOG.Info($"button_OnClick()", this);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            //evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);

            // setup
            setupProblem(pMGR.Current);
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
        protected override void onDebugNextStep()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro:
                case State.Solve:
                case State.Feedback:
                    fsm.PerformTransition(State.Outro);
                    break;
            }
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Solve); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }



        // FSM
        IEnumerator E_Intro()
        {
            //yield return playTimeline(introTL);
            yield return null;
        }
        IEnumerator X_Intro()
        {
            //yield return stopTimeline(introTL);
            //yield return null;

            AudioMGR.One.StopNarration();
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            //fsm.PerformTransition(State.Feedback);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;
        }
        IEnumerator E_Feedback()
        {
            var correct = true;
            yield return null;

            fsm.PerformTransition(correct ? State.Outro : State.Solve);
        }
        IEnumerator X_Feedback()
        {
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
        [Header("★ Bindings - Panels")]
        [SerializeField] private CanvasGroup[] stepPanelCG = null;
        [Header("★ Bindings - S1.Intro")]
        [Header("★ Bindings - S2.Paint")]
        [Header("★ Bindings - S3.Outro")]
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
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
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}