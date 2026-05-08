using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.Game.UI;
using SRDebugger;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Game.Framework
{
    public enum CheckPoint { Start, UserStart, UserFinish, Outro, Complete }

    [RequireComponent(typeof(GameRunner))]
    [RequireComponent(typeof(GameSceneTester))]
    [RequireComponent(typeof(GameProgress))]
    public abstract class GameBase : MonoBehaviour
    {
        // Properties
        public GameID GameID => onGameID();

        // Methods
        public async UniTask Prepare(GameIndex gameIDX)
        {
            LOG.Info($"Prepare() | {gameIDX}", this);

            await onPrepare(gameIDX);
        }
        public void StartGame()
        {
            LOG.Info($"StartGame()", this);

#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            DOVirtual.DelayedCall(startDelay, () => { onStartGame(); });
        }
        public void FinishGame()
        {
            LOG.Info($"FinishGame()", this);

#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishGame();
        }
        public Type GetStateType()
        {
            return onStateType();
        }

        // Events
        public event Action<GameResult> OnGameComplete;



        // Virtual
        protected abstract GameID onGameID();
        protected virtual Type onStateType() { return null; }
        protected virtual void onInitGame() { }
        protected virtual async UniTask onPrepare(GameIndex gameIDX)
        {
            UIGameCommon.One.VisibleBackButton = true;
            await UniTask.Yield();
        }
        protected virtual void onStartGame()
        {
        }
        protected virtual void onFinishGame()
        {
            AudioMGR.One.StopAll();

            AffordanceMGR.One.Clear();
            DataLoader.One.ReleaseHandles();
        }
        protected virtual void onDebugNext() { }
        protected virtual void onDebugNextStep() { }
        protected virtual void onDebugNextProblem() { }
        protected virtual void onDebugPrevProblem() { }
        protected virtual void onDebugForceFinish() { }
#if !DISABLE_SRDEBUGGER
        protected virtual void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            var sort = 300;

            if (GetType().IsOverride("onDebugNext"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Next(F1)", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_Next);
                    }, "Game", ++sort));
            }
            if (GetType().IsOverride("onDebugNextStep"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("NextStep(F2)", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextStep);
                    }, "Game", ++sort));
            }
            if (GetType().IsOverride("onDebugNextProblem"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("NextProblem(F3)", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextProblem);
                    }, "Game", ++sort));
            }
            if (GetType().IsOverride("onDebugPrevProblem"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("PrevProblem(F4)", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_PrevProblem);
                    }, "Game", ++sort));
            }
            if (GetType().IsOverride("onDebugForceFinish"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Force Finish", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_ForceFinish);
                    }, "Game", ++sort));
            }

            sort = 400;
            srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Correct 1 time", () =>
                    {
                        UIGameCommon.One.StarGauge.Success(1);
                        GameProgress.One.Correct(1);
                    }, "Game Progress", ++sort));
            srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Correct 5 times", () =>
                    {
                        UIGameCommon.One.StarGauge.Success(5);
                        GameProgress.One.Correct(5);
                    }, "Game Progress", ++sort));
            srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Correct 10 times", () =>
                    {
                        UIGameCommon.One.StarGauge.Success(10);
                        GameProgress.One.Correct(10);
                    }, "Game Progress", ++sort));
        }
#endif

        // Functions : for concrete
        protected void gameComplete()
        {
            AudioMGR.One.StopBGM(true);

            OnGameComplete?.Invoke(GameProgress.One.Result);
        }
        protected void CP(CheckPoint cp)
        {
            switch (cp)
            {
                case CheckPoint.Start:
                    GameProgress.One.StartMeasureOfPlayingTime();
                    break;

                case CheckPoint.UserStart:
                    AffordanceMGR.One.StartMonitor(affTimeout);
                    break;

                case CheckPoint.UserFinish:
                    AffordanceMGR.One.StopMonitor();
                    break;

                case CheckPoint.Outro:
                    UIGameCommon.One.VisibleBackButton = false;
                    UIGameCommon.One.VisiblePauseButton = false;
                    break;

                case CheckPoint.Complete:
                    stopBGM();
                    GameProgress.One.FinishMeasureOfPlayingTime();
                    break;
            }
        }

        // Functions : for concrete
        protected void evaluateTimeline(PlayableDirector timeline, double time = 0)
        {
            timeline.time = time;
            timeline.Evaluate();
            timeline.Stop();
        }
        protected IEnumerator playTimeline(PlayableDirector timeline, float delay = 0.3f)
        {
            timeline.time = 0;
            timeline.Play();
            yield return new WaitForSeconds((float)timeline.duration);
            yield return new WaitForSeconds(delay);
        }
        protected IEnumerator stopTimeline(PlayableDirector timeline)
        {
            timeline.time = timeline.duration;
            timeline.Evaluate();
            timeline.Stop();
            yield return null;
        }
        protected void playBGM()
        {
            AudioMGR.One.PlayBGM(bgmCLIP, bgmVolume / 100f);
        }
        protected void stopBGM()
        {
            AudioMGR.One.StopBGM(true);
        }


        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif

        // Event Handlers
        private void systemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Debug_NextStep: onDebugNextStep(); break;
                case SystemButtonType.Debug_PrevProblem: onDebugPrevProblem(); break;
                case SystemButtonType.Debug_NextProblem: onDebugNextProblem(); break;
                case SystemButtonType.Debug_Next: onDebugNext(); break;
                case SystemButtonType.Debug_ForceFinish: onDebugForceFinish(); break;
            }
        }



        // Unity Inspectors
        [Header("¡Ú (GameBase) Sound")]
        [SerializeField] private AudioClip bgmCLIP = null;
        [Range(0, 100)]
        [SerializeField] int bgmVolume = 100;
        [Header("¡Ú (GameBase) Config")]
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private float affTimeout = 10f;

        // Unity Messages
        protected virtual void Awake()
        {
            onInitGame();
        }
        protected virtual void Start()
        {

        }
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.F4) && GetType().IsOverride("onDebugPrevProblem"))
                SystemEventManager.SystemButtonClick(SystemButtonType.Debug_PrevProblem);
            if (Input.GetKeyDown(KeyCode.F3) && GetType().IsOverride("onDebugNextProblem"))
                SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextProblem);
            if (Input.GetKeyDown(KeyCode.F2) && GetType().IsOverride("onDebugNextStep"))
                SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextStep);
            if (Input.GetKeyDown(KeyCode.F1) && GetType().IsOverride("onDebugNext"))
                SystemEventManager.SystemButtonClick(SystemButtonType.Debug_Next);
        }
        protected virtual void OnEnable()
        {
            SystemEventManager.OnSystemButtonClicked += systemEventManager_OnSystemButtonClicked;
        }
        protected virtual void OnDisable()
        {
            SystemEventManager.OnSystemButtonClicked -= systemEventManager_OnSystemButtonClicked;
        }

    }
}