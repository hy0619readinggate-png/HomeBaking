using beyondi.Coroutine;
using beyondi.FSM;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using System.Collections;
using System.Linq;
using UnityEngine;

using GameData = DoDoEng.Common.GameData_C2_G02;
using ProblemMGR = DoDoEng.Game.C2_G02.C2_G02_ProblemMGR;

namespace DoDoEng.Game.C2_G02
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C2_G02_Main : GameMain<C2_G02_Main, GameData>
    {
        // Definitions
        private enum State
        {
            Intro, Round, Game, Next, Resurrect,
            Outro, Reward,
            GameOver
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
            fsm.AddState(State.Game,        E_Game,         X_Game);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.Resurrect,   E_Resurrect,    X_Resurrect);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            fsm.AddState(State.GameOver,    E_GameOver);
            #pragma warning restore format
        }
        private void setupProblem(ProblemData pData)
        {
            //UIGameCommon.One.Progress.Setup(pData.CustomerCount);

            plant.Setup(pData.Bullets, pData.ConveyorSpeed);
            progress.Setup(pData.MonsterCount);
        }

        // Event Handlers
        private void monster_OnDied(Monster m)
        {
            LOG.Info($"monster_OnDied()", this);

            UIGameCommon.One.StarGauge.Success();
            GameProgress.One.Correct();
        }
        private void aff_OnAffStart(GameObject obj)
        {
            //LOG.Info($"aff_OnAffStart()", this);

            var peachTRs = plant.MovingPeacheTRs.TakeLast(1).ToArray();
            var cannonTRs = cannons
                            .Where(c => c.IsAlive)
                            .Take(1)
                            .Select(c => c.transform)
                            .ToArray();
            aff.Setup(peachTRs, cannonTRs);
        }



        // Overrides
        protected override GameID onGameID() => GameID.C2_G02_ForestGuard;
        protected override System.Type onStateType() => typeof(State);
        protected override void onInitGame()
        {
            base.onInitGame();
        }
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);

            // Init
            plant.Init();

            // UI
            UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, pMGR.TotalProblemCount);
            GameProgress.One.Setup(pMGR.TotalProblemCount);

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
        protected override void onDebugNextProblem()
        {
            if (fsm.CurrentState == State.Game)
            {
                var monster = monsterMGR.DEV_GetFrontMonster();
                if (monster != null)
                {
                    var bulletData = pMGR.Current.DEV_GetBullet(monster.SoundPhonics);

                    CannonMGR.One.DEV_Fire(monster.Lane, bulletData);
                }
            }
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
            yield return null;

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

            plant.StartProduction();
            monsterMGR.StartSpawn(pMGR.Current.Monsters, progress);

            var wait = new WaitForComplete(this, monsterMGR, plant);
            yield return wait;

            if (wait.Completed is MonsterMGR)
            {
                plant.StopProduction();
                yield return new WaitForSeconds(stopProductionDelay);

                fsm.PerformTransition(State.Next);
            }
            else if (wait.Completed is Plant)
            {
                fsm.PerformTransition(State.GameOver);

            }
        }
        IEnumerator X_Game()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            plant.StopProduction();
            monsterMGR.StopSpawn();
            monsterMGR.ClearAllSurvivors();
            yield return null;
        }
        IEnumerator E_Next()
        {
            yield return new WaitForSeconds(nextDELAY);

            if (pMGR.Next())
                fsm.PerformTransition(State.Resurrect);
            else fsm.PerformTransition(State.Outro);
            yield return null;
        }
        IEnumerator E_Resurrect()
        {
            CannonMGR.One.ResurrectAll();
            yield return null;

            fsm.PerformTransition(State.Round);
        }
        IEnumerator X_Resurrect()
        {
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

            AudioMGR.One.PlayEffect(SfxMoment.Activity_Complete);

            gameComplete();
            yield return null;
        }
        IEnumerator E_GameOver()
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
        [SerializeField] private Plant plant = null;
        [SerializeField] private MonsterMGR monsterMGR = null;
        [SerializeField] private UIProgress progress = null;
        [SerializeField] private Cannon[] cannons = null;
        [SerializeField] private AffDrag aff = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ Timing")]
        [SerializeField] private float stopProductionDelay = 2f;
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

            aff.OnAffStart += aff_OnAffStart;
            EventBus.Subscribe<EventBus.MonsterDiedEvent>(monster_OnDied);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            aff.OnAffStart -= aff_OnAffStart;
            EventBus.Unsubscribe<EventBus.MonsterDiedEvent>(monster_OnDied);
        }
    }
}