using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

using ActivityData = DoDoEng.Common.ActivityData_C3_A04;
using ProblemMGR = DoDoEng.Activity.C3_A04.C3_A04_ProblemMGR;

// devBOX - 삭제요망
#pragma warning disable 0414

namespace DoDoEng.Activity.C3_A04
{
    [RequireComponent(typeof(ProblemMGR))]
    public class Test : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C3_A04_SpaceAdventure;
        private enum State
        {
            ProblemIn, Spread, Ready,
            Expedition, ExpeditionFin, Next,
            ProblemOut,
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
            fsm.AddState(State.ProblemIn,       E_ProblemIn,        X_ProblemIn);
            fsm.AddState(State.Spread,          E_Spread,           X_Spread);
            fsm.AddState(State.Ready,           E_Ready,            X_Ready);
            fsm.AddState(State.Expedition,      E_Expedition,       X_Expedition);
            fsm.AddState(State.ExpeditionFin,   E_ExpeditionFin,    X_ExpeditionFin);
            fsm.AddState(State.ProblemOut,      E_ProblemOut,       X_ProblemOut);
            fsm.AddState(State.Next,            E_Next);
            fsm.AddState(State.Outro,           E_Outro,            X_Outro);
            fsm.AddState(State.Reward,          E_Reward);
            #pragma warning restore format  
        }
        private void setupProblem(ProblemData pData)
        {
            holoSpaceShip.Setup(pData);
            planetMGR.Setup(pData);
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(problemInTL);

            stepPanelCG.SetActiveOnly(0);

            //SpaceShip.One.playerController = playerController;
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);

            // setup
            //setupProblem(pMGR.Current);
        }
        protected override void onStartActivity()
        {
            base.onStartActivity();

            fsm.StartFSM(State.ProblemIn, RunnerParam.SkipStateTo);
        }
        protected override void onFinishActivity()
        {
            base.onFinishActivity();

            if (fsm.CurrentState == State.Expedition)
                CP(CheckPoint.UserFinish);

            fsm?.StopFSM();
            AudioMGR.One.StopAll();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.ProblemIn: fsm.PerformTransition(State.Spread); break;
                case State.Spread: fsm.PerformTransition(State.Ready); break;
                case State.Ready: fsm.PerformTransition(State.Expedition); break;
                case State.Expedition: fsm.PerformTransition(State.ExpeditionFin); break;
                case State.ExpeditionFin: fsm.PerformTransition(State.Next); break;
                case State.ProblemOut: fsm.PerformTransition(State.ProblemIn); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }



        // FSM
        IEnumerator E_ProblemIn()
        {
            setupProblem(pMGR.Current);
            yield return playTimeline(problemInTL);
            yield return null;

            fsm.PerformTransition(State.Spread);
        }
        IEnumerator X_ProblemIn()
        {
            yield return stopTimeline(problemInTL);
            yield return null;
        }
        IEnumerator E_Spread()
        {
            yield return holoSpaceShip.ScatterCharacters();
            yield return null;

            fsm.PerformTransition(State.Ready);
        }
        IEnumerator X_Spread()
        {
            yield return null;
        }
        IEnumerator E_Ready()
        {
            yield return playTimeline(readyTL);
            yield return null;

            fsm.PerformTransition(State.Expedition);
        }
        IEnumerator X_Ready()
        {
            yield return stopTimeline(readyTL);
            yield return null;
        }
        IEnumerator E_Expedition()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            playerController.EnableInteraction(true);
            yield return null;

            //yield return SpaceShip.One.StartExpedition(pMGR.Current.Answer);
            yield return new WaitForSeconds(1f);

            fsm.PerformTransition(State.ExpeditionFin);
        }
        IEnumerator X_Expedition()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            playerController.EnableInteraction(false);
            yield return null;

            //SpaceShip.One.FinishExpedition();
            yield return new WaitForSeconds(1f);

        }
        IEnumerator E_ExpeditionFin()
        {
            yield return playTimeline(expeditionFinTL);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_ExpeditionFin()
        {
            yield return stopTimeline(expeditionFinTL);
            yield return null;
        }
        IEnumerator E_Next()
        {
            if (pMGR.Next())
                fsm.PerformTransition(State.ProblemOut);
            else fsm.PerformTransition(State.Outro);
            yield return null;
        }
        IEnumerator E_ProblemOut()
        {
            yield return playTimeline(problemOutTL);
            yield return null;

            fsm.PerformTransition(State.ProblemIn);
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

            yield return playTimeline(problemOutTL);
            yield return null;

            fsm.PerformTransition(State.Reward);
        }
        IEnumerator X_Outro()
        {
            yield return stopTimeline(problemOutTL);
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
        [SerializeField] private HoloSpaceShip holoSpaceShip = null;
        [SerializeField] private PlanetMGR planetMGR = null;
        [SerializeField] private PlayerController playerController = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector problemInTL = null;
        [SerializeField] private PlayableDirector readyTL = null;
        [SerializeField] private PlayableDirector expeditionFinTL = null;
        [SerializeField] private PlayableDirector problemOutTL = null;
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