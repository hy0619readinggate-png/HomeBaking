using beyondi.FSM;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System.Collections;
using UnityEngine;

using GameData = DoDoEng.Common.GameData_C1_G00;
using ProblemMGR = DoDoEng.Game.C1_G00.C1_G00_ProblemMGR;

namespace DoDoEng.Game.C1_G00
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_G00_Main : GameMain<C1_G00_Main, GameData>
    {
        // Definitions
        private enum State { Intro, Round, Game, Next, Outro, Reward }



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
            fsm.AddState(State.Game,        E_Game,        X_Game);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(ProblemData pData)
        {
            //UIGameCommon.One.Progress.Setup(pData.CustomerCount);
        }



        // Overrides
        protected override GameID onGameID() => GameID.C1_G01_IceCreamShop;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);

            // init
            round.gameObject.SetActive(false);

            // UI
            //UIGameCommon.One.Progress.Setup(pMGR.Current.CustomerCount);
            //UIGameCommon.One.StarGauge.Setup(pMGR.CustomerCounts);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartGame()
        {
            base.onStartGame();

            playBGM();
            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishGame()
        {
            base.onFinishGame();

            if (fsm.CurrentState == State.Game)
                CP(CheckPoint.UserFinish);

            stopBGM();
            fsm?.StopFSM();
        }
        protected override void onDebugNextStep()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Round); break;
                case State.Round: fsm.PerformTransition(State.Game); break;
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

            yield return round.PlayRoundAndWait(pMGR.Current.Round);
            yield return null;

            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Round()
        {
            round.Hide();
            yield return null;
        }
        IEnumerator E_Game()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Game()
        {
            CP(CheckPoint.UserFinish);
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return new WaitForSeconds(nextDelay);

            if (pMGR.Next())
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
        [SerializeField] private Round round = null;
        [Header("★ Timing")]
        [SerializeField] private float nextDelay = 1f;

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