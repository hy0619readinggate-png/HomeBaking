using beyondi.FSM;
using beyondi.Util;
using Com.LuisPedroFonseca.ProCamera2D;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;
using ActivityData = DoDoEng.Common.ActivityData_C3_A04;
using ProblemMGR = DoDoEng.Activity.C3_A04.C3_A04_ProblemMGR;

namespace DoDoEng.Activity.C3_A04
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C3_A04_Main : ActivityMain<ActivityData>
    {
        // Definitions
        private const ActivityID TheActivityID = ActivityID.C3_A04_SpaceAdventure;
        private enum State
        {
            Intro,
            ProblemIn, Spread, Ready,
            Game, Clear, Review, Next,
            ProblemOut,
            Outro, Reward
        }

        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private ProCamera2D procamera2D_ = null;
        private ProCamera2D procamera2D => procamera2D_ ??= Camera.main.GetComponent<ProCamera2D>();

        // Fields
        private FSM<State> fsm = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,           E_Intro);
            fsm.AddState(State.ProblemIn,       E_ProblemIn,        X_ProblemIn);
            fsm.AddState(State.Spread,          E_Spread,           X_Spread);
            fsm.AddState(State.Ready,           E_Ready,            X_Ready);
            fsm.AddState(State.Game,            E_Game,             X_Game);
            fsm.AddState(State.Clear,           E_Clear,            X_Clear);
            fsm.AddState(State.Review,          E_Review,           X_Review);
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
            answerIMG.sprite = pData.SentenceSPR;
        }



        // Overrides
        protected override ActivityID onActivityID() => TheActivityID;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitActivity()
        {
            base.onInitActivity();

            evaluateTimeline(problemInTL);

            stepPanelCG.SetActiveOnly(0);
            spaceShip.Init();
        }
        protected override async UniTask onPrepare(ActivityIndex actIDX)
        {
            await base.onPrepare(actIDX);

            // Load Unit Data
            await pMGR.Build(actIDX, CURRICULUM, TABLES);
            ActivityProgress.One.Setup(pMGR.BlanksCount);

            // setup
            //setupProblem(pMGR.Current);
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

            if (fsm.CurrentState == State.Game)
                CP(CheckPoint.UserFinish);

            stopBGM();
            AudioMGR.One.StopAmbient(true);

            fsm?.StopFSM();
        }
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.ProblemIn: fsm.PerformTransition(State.Spread); break;
                case State.Spread: fsm.PerformTransition(State.Ready); break;
                case State.Ready: fsm.PerformTransition(State.Game); break;
                case State.Game: fsm.PerformTransition(State.Clear); break;
                case State.Clear: fsm.PerformTransition(State.Review); break;
                case State.Review: fsm.PerformTransition(State.Next); break;
                case State.ProblemOut: fsm.PerformTransition(State.ProblemIn); break;
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
            yield return null;

            fsm.PerformTransition(State.ProblemIn);
        }
        IEnumerator E_ProblemIn()
        {
            setupProblem(pMGR.Current);
            spaceShip.Ready();
            procamera2D.enabled = true;
            procamera2D.Reset(true);
            yield return playTimeline(problemInTL);
            yield return null;

            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);
            yield return new WaitForSeconds(1);

            fsm.PerformTransition(State.Spread);
        }
        IEnumerator X_ProblemIn()
        {
            AudioMGR.One.StopNarration();

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

            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Ready()
        {
            yield return stopTimeline(readyTL);
            yield return null;
        }
        IEnumerator E_Game()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            playerController.EnableInteraction(true);
            yield return null;

            yield return spaceShip.StartExpedition(pMGR.Current.Answer);

            fsm.PerformTransition(State.Clear);
        }
        IEnumerator X_Game()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            procamera2D.enabled = false;
            playerController.EnableInteraction(false);
            yield return null;

            spaceShip.FinishExpedition();
            yield return null;

        }
        IEnumerator E_Clear()
        {
            planetMGR.DeactivateAll();
            holoSpaceShip.Complete();
            yield return spaceShip.MoveToExitPosition1();

            AudioMGR.One.PlayNarration(pMGR.Current.WordCLIP, 2f);
            yield return spaceShip.MoveToExitPosition2();
            yield return new WaitForSeconds(0.5f);

            yield return playTimeline(expeditionFinTL);
            yield return null;

            fsm.PerformTransition(State.Review);
        }
        IEnumerator X_Clear()
        {
            spaceShip.StopMoving();
            yield return null;

            yield return stopTimeline(expeditionFinTL);
            yield return null;
        }
        IEnumerator E_Review()
        {
            yield return AudioMGR.One.PlayNarrationAndWait(pMGR.Current.SentenceCLIP);
            yield return new WaitForSeconds(reviewDelay);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Review()
        {
            AudioMGR.One.StopNarration();
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
            yield return new WaitForSeconds(1);

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
        [SerializeField] private HoloSpaceShip holoSpaceShip = null;
        [SerializeField] private SpaceShip spaceShip = null;
        [SerializeField] private PlanetMGR planetMGR = null;
        [SerializeField] private PlayerController playerController = null;
        [SerializeField] private Image answerIMG = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector problemInTL = null;
        [SerializeField] private PlayableDirector readyTL = null;
        [SerializeField] private PlayableDirector expeditionFinTL = null;
        [SerializeField] private PlayableDirector problemOutTL = null;
        [Header("★ Timing")]
        [SerializeField] private float reviewDelay = 2f;
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