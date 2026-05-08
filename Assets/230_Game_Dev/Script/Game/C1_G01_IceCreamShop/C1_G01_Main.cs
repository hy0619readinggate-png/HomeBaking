using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System.Collections;
using UnityEngine;
using GameData = DoDoEng.Common.GameData_C1_G01;
using ProblemMGR = DoDoEng.Game.C1_G01.C1_G01_ProblemMGR;

namespace DoDoEng.Game.C1_G01
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_G01_Main : GameMain<C1_G01_Main, GameData>
    {
        // Definitions
        private enum State
        {
            Intro,
            Round, Ready, Game, Next,
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
            fsm.AddState(State.Round,       E_Round,        X_Round);
            fsm.AddState(State.Ready,       E_Ready,        X_Ready);
            fsm.AddState(State.Game,        E_Game,         X_Game);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(ProblemData pData)
        {
            LOG.Assert(
                pData.IceCreams.Length == tins.Length,
                $"pData.IceCreamPool.Length must be same to tins.Length.", this);

            for (var i = 0; i < pData.IceCreams.Length; i++)
                tins[i].Setup(pData.IceCreams[i]);

            UIGameCommon.One.Progress.Setup(pData.CustomerCount);
        }

        // Event Handlers
        private void customerMGR_OnFail()
        {
            LOG.Info($"customerMGR_OnFail()", this);

            if (fsm.CurrentState == State.Game)
                fsm.PerformTransition(State.Next);
        }



        // Overrides
        protected override GameID onGameID() => GameID.C1_G01_IceCreamShop;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitGame()
        {
            base.onInitGame();

            // init
            tins.AutoFillID();
        }
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);

            // UI
            UIGameCommon.One.Progress.Setup(pMGR.Current.CustomerCount);
            UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, pMGR.TotalProblemCount);
            GameProgress.One.Setup(pMGR.TotalProblemCount);

            // Init
            customerMGR.Init(pMGR.InitialHP);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartGame()
        {
            base.onStartGame();

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);

            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishGame()
        {
            base.onFinishGame();

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
                case State.Intro: fsm.PerformTransition(State.Round); break;
                case State.Round: fsm.PerformTransition(State.Ready); break;
                case State.Ready: fsm.PerformTransition(State.Game); break;
                case State.Game: fsm.PerformTransition(State.Next); break;
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
            yield return new WaitForSeconds(1);
            yield return null;

            fsm.PerformTransition(State.Round);
        }
        IEnumerator X_Intro()
        {
            yield return null;
        }
        IEnumerator E_Round()
        {
            if (pMGR.PNO > 1)
                setupProblem(pMGR.Current);

            var tl = pMGR.PNO == 1
                ? UIGameCommon.One.ReadyGoTL
                : UIGameCommon.One.LevelUpTL;
            yield return playTimeline(tl);
            yield return null;

            fsm.PerformTransition(State.Ready);
        }
        IEnumerator X_Round()
        {
            var tl = pMGR.PNO == 1
                ? UIGameCommon.One.ReadyGoTL
                : UIGameCommon.One.LevelUpTL;
            yield return stopTimeline(tl);
        }
        IEnumerator E_Ready()
        {
            UIGameCommon.One.VisiblePauseButton = true;
            yield return null;

            tins.ForEach(t => t.OpenCover(Random.Range(0, 0.5f)));
            yield return new WaitForSeconds(1);

            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Ready()
        {
            yield return null;
        }
        IEnumerator E_Game()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            yield return customerMGR.StartVisit(pMGR.Current, pMGR.Config);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Game()
        {
            customerMGR.StopVisit();
            yield return null;

            tins.ForEach(t => t.CloseCover(Random.Range(0, 0.5f)));
            yield return null;

            CP(CheckPoint.UserFinish);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return new WaitForSeconds(nextDELAY);

            if (pMGR.Next() && customerMGR.IsSuccessAll)
                fsm.PerformTransition(State.Round);
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

            AudioMGR.One.StopAll();
            yield return null;

            gameComplete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private IceCreamTin[] tins = null;
        [SerializeField] private CustomerMGR customerMGR = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ Timing")]
        [SerializeField] private float nextDELAY = 1f;

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

            customerMGR.OnFail += customerMGR_OnFail;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            customerMGR.OnFail -= customerMGR_OnFail;
        }
    }
}