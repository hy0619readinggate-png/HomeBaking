using beyondi.FSM;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using GameData = DoDoEng.Common.GameData_C2_G01;
using ProblemMGR = DoDoEng.Game.C2_G01.C2_G01_ProblemMGR;

namespace DoDoEng.Game.C2_G01
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_G01_Main : GameMain<C2_G01_Main, GameData>
    {
        // Definitions
        private enum State { Intro, Round, Game, Next, Outro, Reward }

        // Properties
        private float hp = 0;



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
        private void setupProblem(RoundData pData)
        {
            hp = pData.Problems.Length;
            UIGameCommon.One.HealthBar.Setup(hp);
        }

        // Functions
        private void enableTouchPads(bool enable)
        {
            touchPads.ForEach(p => p.EnableInteraction(enable));
        }

        // Event Handlers
        private void note_OnCorrect(Note note)
        {
            LOG.Info($"{nameof(note_OnCorrect)}()", this);

        }
        private void note_OnWrong(Note note)
        {
            LOG.Info($"{nameof(note_OnWrong)}()", this);

            hp--;
            if (hp <= 0)
                hp = 0;

            UIGameCommon.One.HealthBar.UpdateHP(hp);

            if (hp == 0 && fsm.CurrentState == State.Game)
                fsm.PerformTransition(State.Reward);
        }
        private void note_OnFloor(Note note)
        {
            LOG.Info($"{nameof(note_OnFloor)}()", this);

            if (note.IsFever)
                return;

            var problemFail = noteMGR.checkProblemFail(note);
            if (problemFail)
            {
                hp--;
                if (hp <= 0)
                    hp = 0;

                UIGameCommon.One.HealthBar.UpdateHP(hp);
            }

            if (hp == 0 && fsm.CurrentState == State.Game)
                fsm.PerformTransition(State.Reward);
        }

        // Overrides
        protected override GameID onGameID() => GameID.C2_G01_Concert;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);

            // init
            //round.gameObject.SetActive(false);
            noteMGR.Init(quizBoard);
            chello.PlayIdle();

            // UI
            UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, pMGR.TotalProblemCount);
            GameProgress.One.Setup(pMGR.TotalProblemCount);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartGame()
        {
            base.onStartGame();

            fsm.StartFSM(State.Intro, RunnerParam.SkipStateTo);
        }
        protected override void onFinishGame()
        {
            base.onFinishGame();

            if (fsm.CurrentState == State.Game)
                CP(CheckPoint.UserFinish);

            fsm?.StopFSM();
            AudioMGR.One.StopAll();
        }
        protected override void onDebugNext()
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

            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Round()
        {
            var tl = pMGR.PNO == 1
                ? UIGameCommon.One.ReadyGoTL
                : UIGameCommon.One.LevelUpTL;
            yield return stopTimeline(tl);
        }
        IEnumerator E_Game()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIGameCommon.One.VisiblePauseButton = true;
            yield return null;

            enableTouchPads(true);
            chello.PlayDance();
            yield return noteMGR.StartMusic(pMGR.Current);
            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Game()
        {
            enableTouchPads(false);
            chello.PlayIdle();
            noteMGR.StopMusic();
            yield return null;

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

            yield return playTimeline(outroTL);

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

            AudioMGR.One.StopAll();
            yield return null;

            gameComplete();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        //[SerializeField] private Round round = null;
        [SerializeField] private QuizBoard quizBoard = null;
        [SerializeField] private NoteMGR noteMGR = null;
        [SerializeField] private Chello chello = null;
        [SerializeField] private TouchPad[] touchPads = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector outroTL = null;
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

            EventBus.Subscribe<EventBus.NoteWrong>(note_OnWrong);
            EventBus.Subscribe<EventBus.NoteFloor>(note_OnFloor);

        }
        protected override void OnDisable()
        {
            base.OnDisable();

            EventBus.Unsubscribe<EventBus.NoteWrong>(note_OnWrong);
            EventBus.Unsubscribe<EventBus.NoteFloor>(note_OnFloor);
        }
    }
}