using beyondi.Coroutine;
using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C2_A12;
using ProblemMGR = DoDoEng.Activity.C2_A12.C2_A12_ProblemMGR;

namespace DoDoEng.Activity.C2_A12
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_A12_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C2_A12_Pipes;
        private enum State
        {
            Intro,
            Problem, Solve,
            TurnOn, WaterSupply, Next,
            Outro, Reward
        }

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
            fsm.AddState(State.Problem,     E_Problem,      X_Problem);
            fsm.AddState(State.Solve,       E_Solve,        X_Solve);
            fsm.AddState(State.TurnOn,      E_TurnOn,       X_TurnOn);
            fsm.AddState(State.WaterSupply, E_WaterSupply,  X_WaterSupply);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            exampleMGR.Setup(pData.Examples);
            dragAff.Setup(currentProblem.Subjects);
        }
        private Problem currentProblem => problems[pMGR.PNO - 1];
        private Subject[] currentSubjects => currentProblem.Subjects;
        private PlayableDirector currentSupplayTL => waterSupplyTL[pMGR.PNO - 1];



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(introTL);

            stepPanelCG.SetActiveOnly(0);
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            problems.ForEach((i, p) => p.Setup(pMGR.Problems[i]));
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

            if (fsm.CurrentState == State.Solve || fsm.CurrentState == State.TurnOn)
                CP(CheckPoint.UserFinish);

            stopBGM();
            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Problem); break;
                case State.Solve: fsm.PerformTransition(State.WaterSupply); break;
                case State.TurnOn: fsm.PerformTransition(State.WaterSupply); break;
                case State.WaterSupply: fsm.PerformTransition(State.Next); break;
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

            fsm.PerformTransition(State.Problem);
        }
        IEnumerator X_Intro()
        {
            yield return stopTimeline(introTL);
            yield return null;
        }
        IEnumerator E_Problem()
        {
            setupProblem(pMGR.Current);
            yield return null;

            fsm.PerformTransition(State.Solve);
        }
        IEnumerator X_Problem()
        {
            yield return null;
        }
        IEnumerator E_Solve()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            Millo.One.Idle();
            yield return null;

            exampleMGR.StartSpawnExample();
            yield return new WaitForComplete(this, currentProblem);

            exampleMGR.FinishSpawnExample();
            yield return new WaitForSeconds(correctDelay);

            fsm.PerformTransition(State.WaterSupply);
        }
        IEnumerator X_Solve()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            exampleMGR.FinishSpawnExample();
            yield return null;
        }
        IEnumerator E_TurnOn()
        {
            Millo.One.IdleWithButton();
            yield return null;

            supplyButton.EnableInteraction(true);
            yield return null;

            yield return new WaitForSubmit(this, supplyButton);
            yield return null;

            fsm.PerformTransition(State.WaterSupply);
        }
        IEnumerator X_TurnOn()
        {
            supplyButton.EnableInteraction(false);
            yield return null;
        }
        IEnumerator E_WaterSupply()
        {
            yield return playTimeline(currentSupplayTL);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_WaterSupply()
        {
            yield return stopTimeline(currentSupplayTL);
            yield return null;
        }
        IEnumerator E_Next()
        {
            if (pMGR.Next())
                fsm.PerformTransition(State.Problem);
            else fsm.PerformTransition(State.Outro);
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
        [SerializeField] private ExampleMGR exampleMGR = null;
        [SerializeField] private Problem[] problems = null;
        [SerializeField] private SupplyButton supplyButton = null;
        [SerializeField] private DragAff dragAff = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector introTL = null;
        [SerializeField] private PlayableDirector[] waterSupplyTL = null;
        [Header("★ Timing")]
        [SerializeField] private float correctDelay = 2f;
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