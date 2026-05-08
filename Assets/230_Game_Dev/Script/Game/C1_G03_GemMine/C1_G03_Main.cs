using beyondi.Coroutine;
using beyondi.FSM;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using SRDebugger;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using GameData = DoDoEng.Common.GameData_C1_G03;
using ProblemMGR = DoDoEng.Game.C1_G03.C1_G03_ProblemMGR;

namespace DoDoEng.Game.C1_G03
{
    [RequireComponent(typeof(ProblemMGR))]
    public class C1_G03_Main : GameMain<C1_G03_Main, GameData>
    {
        // Definitions
        private enum State
        {
            Intro,
            Round, QuickLook, Enter, Game, Next,
            NextLevel,
            Outro, Reward
        }



        // Fields : caching
        private ProblemMGR pMGR_ = null;
        private ProblemMGR pMGR => pMGR_ ??= GetComponent<ProblemMGR>();
        private TimelineSignal nextLevelSIG_ = null;
        private TimelineSignal nextLevelSIG => nextLevelSIG_ ??= nextLevelTL.GetComponent<TimelineSignal>();

        // Fields
        private FSM<State> fsm = null;

        // Functions
        private void initFSM()
        {
            #pragma warning disable format
            fsm = new FSM<State>(this);
            fsm.AddState(State.Intro,       E_Intro);
            fsm.AddState(State.Round,       E_Round,        X_Round);
            fsm.AddState(State.QuickLook,   E_QuickLook,    X_QuickLook);
            fsm.AddState(State.Enter,       E_Enter,        X_Enter);
            fsm.AddState(State.Game,        E_Game,         X_Game);
            fsm.AddState(State.Next,        E_Next);
            fsm.AddState(State.NextLevel,   E_NextLevel,    X_NextLevel);
            fsm.AddState(State.Outro,       E_Outro,        X_Outro);
            fsm.AddState(State.Reward,      E_Reward);
            #pragma warning restore format
        }
        private void setupProblem(RoundData rData)
        {
            UIGameCommon.One.TimerWithAnimator.DisplayTime(rData.Duration);

            roundMGR.Setup(rData);
            world.Setup(rData.MapVariation);
            progress.Setup(rData.GemVariation);
        }

        // Event Handlers
        private void nextLevelSIG_OnSignal(string signal)
        {
            LOG.Function(this, $"{signal}");

            if (signal == "Activity-SetupProblem")
            {
                setupProblem(pMGR.Current);
                CameraControl.One.CenterOnPlayer();
            }
        }

        // Overrides
        protected override GameID onGameID() => GameID.C1_G03_GemMine;
        protected override System.Type onStateType() => typeof(State);
        protected override async UniTask onPrepare(GameIndex gameIDX)
        {
            await base.onPrepare(gameIDX);

            // Load Unit Data
            await pMGR.Build(gameIDX, CURRICULUM, TABLES);

            // init
            evaluateTimeline(quickLookTL);

            // UI
            UIGameCommon.One.Progress.Setup(pMGR.Current.ProblemCount);
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
        protected override void onDebugNext()
        {
            switch (fsm.CurrentState)
            {
                case State.Intro: fsm.PerformTransition(State.Round); break;
                case State.Round: fsm.PerformTransition(State.QuickLook); break;
                case State.QuickLook: fsm.PerformTransition(State.Enter); break;
                case State.Enter: fsm.PerformTransition(State.Game); break;
                case State.Game: fsm.PerformTransition(State.Next); break;
                case State.NextLevel: fsm.PerformTransition(State.Enter); break;
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
                OptionDefinition.FromMethod("Place Roads to Answer GEM", () =>
                {
                    var word = roundMGR.DEV_GetCurrentWord();
                    if (!string.IsNullOrEmpty(word))
                        Map.One.DEV_PlaceRoad_AnswerGem(word);

                }, "C1_G03_GemMine", ++sort));

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Place Roads to Nearest GEM", () =>
                {
                    Map.One.DEV_PlaceRoad_NearestGem();

                }, "C1_G03_GemMine", ++sort));

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Place Roads to Furthest GEM", () =>
                {
                    Map.One.DEV_PlaceRoad_FurthestGem();

                }, "C1_G03_GemMine", ++sort));

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Skip 10s of Timer", () =>
                {
                    UIGameCommon.One.TimerWithAnimator.DEV_Skip(10);

                }, "C1_G03_GemMine", ++sort));
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Skip 30s of Timer", () =>
                {
                    UIGameCommon.One.TimerWithAnimator.DEV_Skip(30);

                }, "C1_G03_GemMine", ++sort));
        }
#endif



        // FSM
        IEnumerator E_Intro()
        {
            CP(CheckPoint.Start);
            yield return null;

            fsm.PerformTransition(State.Round);
        }
        IEnumerator E_Round()
        {
            var tl = pMGR.PNO == 1
                ? UIGameCommon.One.ReadyGoTL
                : UIGameCommon.One.LevelUpTL;
            yield return playTimeline(tl);

            fsm.PerformTransition(State.QuickLook);
        }
        IEnumerator X_Round()
        {
            var tl = pMGR.PNO == 1
                ? UIGameCommon.One.ReadyGoTL
                : UIGameCommon.One.LevelUpTL;
            yield return stopTimeline(tl);
        }
        IEnumerator E_QuickLook()
        {
            yield return playTimeline(quickLookTL);
            yield return null;

            fsm.PerformTransition(State.Enter);
        }
        IEnumerator X_QuickLook()
        {
            yield return stopTimeline(quickLookTL);
            yield return null;
        }
        IEnumerator E_Enter()
        {
            CameraControl.One.FollowPlayer();
            yield return new WaitForSeconds(1);

            yield return playTimeline(enterTL);
            yield return null;

            fsm.PerformTransition(State.Game);
        }
        IEnumerator X_Enter()
        {
            CameraControl.One.UnfollowPlayer();
            yield return null;

            yield return stopTimeline(enterTL);
            yield return null;
        }
        IEnumerator E_Game()
        {
            CP(CheckPoint.UserStart);
            yield return null;

            UIGameCommon.One.VisiblePauseButton = true;
            UIGameCommon.One.TimerWithAnimator.StartTimer(pMGR.Current.Duration);
            roundMGR.StartRound();
            yield return new WaitForComplete(this, roundMGR);
            yield return new WaitForSeconds(1f);

            fsm.PerformTransition(State.Next);
        }
        IEnumerator X_Game()
        {
            CP(CheckPoint.UserFinish);
            yield return null;

            UIGameCommon.One.TimerWithAnimator.StopTimer();
            roundMGR.StopRound();
        }
        IEnumerator E_Next()
        {
            yield return new WaitForSeconds(nextDelay);

            if (pMGR.Next() && roundMGR.IsCleared)
                fsm.PerformTransition(State.NextLevel);
            else fsm.PerformTransition(State.Outro);
            yield return null;
        }
        IEnumerator E_NextLevel()
        {
            yield return playTimeline(nextLevelTL);
            yield return null;

            fsm.PerformTransition(State.Intro);
        }
        IEnumerator X_NextLevel()
        {
            yield return stopTimeline(nextLevelTL);
            yield return null;
        }
        IEnumerator E_Outro()
        {
            CP(CheckPoint.Outro);
            yield return null;

            if (roundMGR.IsCleared)
            {
                roundMGR.Ending();
                yield return new WaitForSeconds(outroDuration);
            }

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
        [SerializeField] private RoundMGR roundMGR = null;
        [SerializeField] private World world = null;
        [SerializeField] private Progress progress = null;
        [Header("★ Sound - Ambient")]
        [SerializeField] private AudioClip ambientCLIP = null;
        [Range(0, 100)]
        [SerializeField] int ambientVolume = 100;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector quickLookTL = null;
        [SerializeField] private PlayableDirector enterTL = null;
        [SerializeField] private PlayableDirector nextLevelTL = null;
        [Header("★ Timing")]
        [SerializeField] private float nextDelay = 1f;
        [SerializeField] private float outroDuration = 4f;

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

            nextLevelSIG.OnSignal += nextLevelSIG_OnSignal;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            nextLevelSIG.OnSignal -= nextLevelSIG_OnSignal;
        }
    }
}