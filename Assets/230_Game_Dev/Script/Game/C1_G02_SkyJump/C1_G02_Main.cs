using beyondi.Coroutine;
using beyondi.FSM;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using SRDebugger;
using System.Collections;
using UnityEngine;
using GameData = DoDoEng.Common.GameData_C1_G02;
using ProblemMGR = DoDoEng.Game.C1_G02.C1_G02_ProblemMGR;

namespace DoDoEng.Game.C1_G02
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_G02_Main : GameMain<C1_G02_Main, GameData>
    {
        // Definitions
        private enum State { Ready, Game, Outro, Reward }



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();

        // Fields
        private FSM<State> fsm = null;
        private float hpInitial = 100;
        private float hp = 0;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Ready,       E_Ready,       X_Ready);
            fsm.AddState(State.Game,        E_Game,        X_Game);
            fsm.AddState(State.Outro,       E_Outro,       X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(ProblemData pData)
        {
            //UIGameCommon.One.Progress.Setup(pData.CustomerCount);
        }

        // Event Handlers
        private void player_OnCorrect()
        {
            LOG.Info($"player_OnCorrect()", this);

            UIGameCommon.One.StarGauge.Success();
            GameProgress.One.Correct();
        }
        private void player_OnDeath()
        {
            LOG.Info($"player_OnDeath()", this);

            hp -= 20;
            UIGameCommon.One.HealthBar.UpdateHP(hp);
            if (hp > 0)
            {
                var cloud = cloudMGR.GetCloudAtHeight(player.MaxHeight);
                if (cloud != null)
                    player.Respwan(cloud.transform);
                else LOG.Error($"no cloud on height {player.MaxHeight}", this);
            }
            else DOVirtual.DelayedCall(lastDeathDelay, () => fsm.PerformTransition(State.Reward));
        }
        private void player_OnBoost()
        {
            LOG.Info($"player_OnBoost()", this);

            ui.ShowBoostEffect();
        }
        private void player_OnLevelUp()
        {
            LOG.Info($"player_OnLevelUp()", this);

            ui.ShowBoostEffect();

            UIGameCommon.One.Progress.Setup(pMGR.RoundConfigs[0].ProblemCount);
        }



        // Overrides
        protected override GameID onGameID() => GameID.C1_G02_SkyJump;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);

            // Build Level
            cloudMGR.BuildLevel(pMGR.Problems, pMGR.RoundConfigs);

            // Data
            hp = hpInitial;

            // UI
            UIGameCommon.One.HealthBar.Setup(hp);
            UIGameCommon.One.StarGauge.Setup(GameReward.StarRatio, pMGR.TotalProblemCount);
            GameProgress.One.Setup(pMGR.TotalProblemCount);
            UIGameCommon.One.Progress.Setup(pMGR.RoundConfigs[0].ProblemCount);

            // setup
            setupProblem(pMGR.Current);
        }
        protected override void onStartGame()
        {
            base.onStartGame();

            playBGM();
            AudioMGR.One.PlayAmbient(ambientCLIP, ambientVolume);

            fsm.StartFSM(State.Ready, RunnerParam.SkipStateTo);
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
                case State.Ready: fsm.PerformTransition(State.Game); break;
                case State.Game: fsm.PerformTransition(State.Outro); break;
                case State.Outro: fsm.PerformTransition(State.Reward); break;
            }
        }
        protected override void onDebugForceFinish()
        {
            base.onDebugForceFinish();

            fsm.PerformTransition(State.Reward);
        }
#if !DISABLE_SRDEBUGGER
        protected override void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            base.onBuildOption(srOptionContainer);

            var sort = 500;

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Teleport to Next LevelUp", () =>
                {
                    var cloud = cloudMGR.GetCloudForNextLevelUp(player.MaxHeight);
                    if (cloud != null)
                        player.Respwan(cloud.transform);
                    else LOG.Warning($"No Cloud.", this);
                }, "C1_G02_SkyJump", ++sort));

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Teleport to Top", () =>
                {
                    var cloud = cloudMGR.GetCloudAtTop();
                    if (cloud != null)
                        player.Respwan(cloud.transform);
                    else LOG.Warning($"No Cloud.", this);
                }, "C1_G02_SkyJump", ++sort));
        }
#endif


        // FSM
        IEnumerator E_Ready()
        {
            CP(CheckPoint.Start);
            yield return playTimeline(UIGameCommon.One.ReadyGoTL);
            yield return null;

            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Ready()
        {
            yield return stopTimeline(UIGameCommon.One.ReadyGoTL);
            yield return null;
        }
        IEnumerator E_Game()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIGameCommon.One.VisiblePauseButton = true;
            yield return null;

            Player.One.AutoJump = true;
            yield return null;

            yield return new WaitForComplete(this, castle);

            fsm.PerformTransition(State.Outro);
        }
        IEnumerator X_Game()
        {
            Player.One.AutoJump = false;

            CP(CheckPoint.UserFinish);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            Player.One.HappyOnTop();
            ui.ShowCompleteEffect();
            yield return new WaitForSeconds(outroDuration);

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



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CloudMGR cloudMGR = null;
        [SerializeField] private Player player = null;
        [SerializeField] private UI ui = null;
        [SerializeField] private Castle castle = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;

        [Header("★ Timing")]
        [SerializeField] private float lastDeathDelay = 2f;
        [SerializeField] private float outroDuration = 5f;



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

            player.OnCorrect += player_OnCorrect;
            player.OnDeath += player_OnDeath;
            player.OnBoost += player_OnBoost;
            player.OnLevelUp += player_OnLevelUp;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            player.OnCorrect -= player_OnCorrect;
            player.OnDeath -= player_OnDeath;
            player.OnBoost -= player_OnBoost;
            player.OnLevelUp -= player_OnLevelUp;
        }
    }
}