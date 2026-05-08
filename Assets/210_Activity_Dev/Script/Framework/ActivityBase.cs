using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using SRDebugger;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.Framework
{
    public enum CheckPoint { Start, UserStart, UserFinish, Outro, Complete }

    [RequireComponent(typeof(ActivityRunner))]
    [RequireComponent(typeof(ActivitySceneTester))]
    [RequireComponent(typeof(ActivityProgress))]
    public abstract class ActivityBase : MonoBehaviour
    {
        // Properties
        public ActivityID ActivityID => onActivityID();

        // Methods
        public async UniTask Prepare(ActivityIndex actIDX)
        {
            LOG.Info($"Prepare() | {actIDX}", this);

            await onPrepare(actIDX);
        }
        public void StartActivity()
        {
            LOG.Info($"StartActivity()", this);

#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            DOVirtual.DelayedCall(startDelay, () => { onStartActivity(); });
        }
        public void FinishActivity()
        {
            LOG.Info($"FinishActivity()", this);

#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishActivity();
        }
        public Type GetStateType()
        {
            return onStateType();
        }

        // Events
        public event Action<ActivityResult> OnActivityComplete;
        public event Action OnActivityError;



        // Virtual
        protected abstract ActivityID onActivityID();
        protected virtual Type onStateType() { return null; }
        protected virtual void onInitActivity() { }
        protected virtual async UniTask onPrepare(ActivityIndex actIDX)
        {
            UIActivityCommon.One.VisibleBackButton = true;
            await UniTask.Yield();
        }
        protected virtual void onStartActivity()
        {
        }
        protected virtual void onFinishActivity()
        {
            AffordanceMGR.One.Clear();
            DataLoader.One.ReleaseHandles();
        }
        protected virtual void onPause() { }
        protected virtual void onResume() { }
        protected virtual void onSpeaker() { }
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
                    }, "Activity", ++sort));
            }
            if (GetType().IsOverride("onDebugNextStep"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("NextStep(F2)", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextStep);
                    }, "Activity", ++sort));
            }
            if (GetType().IsOverride("onDebugNextProblem"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("NextProblem(F3)", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextProblem);
                    }, "Activity", ++sort));
            }
            if (GetType().IsOverride("onDebugPrevProblem"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("PrevProblem(F4)", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_PrevProblem);
                    }, "Activity", ++sort));
            }
            if (GetType().IsOverride("onDebugForceFinish"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Force Finish", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_ForceFinish);
                    }, "Activity", ++sort));
            }

            sort = 400;
            srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Wrong 1 time", () =>
                    {
                        ActivityProgress.One.Wrong(1);
                    }, "Activity Progress", ++sort));
            srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Wrong 5 times", () =>
                    {
                        ActivityProgress.One.Wrong(5);
                    }, "Activity Progress", ++sort));
            srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Wrong 10 times", () =>
                    {
                        ActivityProgress.One.Wrong(10);
                    }, "Activity Progress", ++sort));
        }
#endif

        // Functions : for concrete
        protected void complete()
        {
            OnActivityComplete?.Invoke(ActivityProgress.One.Result);
        }
        protected void error()
        {
            OnActivityError?.Invoke();
        }
        protected void CP(CheckPoint cp)
        {
            switch (cp)
            {
                case CheckPoint.Start:
                    ActivityProgress.One.StartMeasureOfPlayingTime();
                    break;

                case CheckPoint.UserStart:
                    AffordanceMGR.One.StartMonitor(affTimeout);
                    break;

                case CheckPoint.UserFinish:
                    AffordanceMGR.One.StopMonitor();
                    break;

                case CheckPoint.Outro:
                    UIActivityCommon.One.VisibleBackButton = false;
                    UIActivityCommon.One.VisiblePauseButton = false;
                    UIActivityCommon.One.VisibleSpeakerButton = false;
                    break;

                case CheckPoint.Complete:
                    stopBGM();
                    AudioMGR.One.StopAmbient(true);
                    ActivityProgress.One.FinishMeasureOfPlayingTime();
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
        protected IEnumerator playTimelines(params PlayableDirector[] timelines)
        {
            foreach (var tl in timelines)
            {
                tl.time = 0;
                tl.Play();
                yield return new WaitForSeconds((float)tl.duration);
            }
        }
        protected IEnumerator playTimelines(PlayableDirector[] timelines, Action<int> onBetween = null)
        {
            foreach (var (tl, i) in timelines.ToList().Select((value, i) => (value, i)))
            {
                tl.time = 0;
                tl.Play();
                yield return new WaitForSeconds((float)tl.duration);

                if (i < timelines.Length - 1)
                    onBetween?.Invoke(i);
            }
            yield return null;
        }
        protected IEnumerator stopTimeline(PlayableDirector timeline)
        {
            timeline.time = timeline.duration;
            timeline.Evaluate();
            timeline.Stop();
            yield return null;
        }
        protected IEnumerator stopTimelines(params PlayableDirector[] timelines)
        {
            var isSkip = timelines.Any(t => t.state == PlayState.Playing);
            LOG.Info($"stopTimelines() | isSkip={isSkip}", this);

            foreach (var tl in timelines)
            {
                if (tl.state == PlayState.Playing)
                    tl.Stop();

                if (tl == timelines.Last())
                {
                    tl.time = tl.duration;

                    if (isSkip)
                        tl.Play(); // For Emiting Signal
                    else tl.Evaluate();
                }
            }
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
                case SystemButtonType.Speaker: onSpeaker(); break;

                case SystemButtonType.Debug_NextStep: onDebugNextStep(); break;
                case SystemButtonType.Debug_PrevProblem: onDebugPrevProblem(); break;
                case SystemButtonType.Debug_NextProblem: onDebugNextProblem(); break;
                case SystemButtonType.Debug_Next: onDebugNext(); break;
                case SystemButtonType.Debug_ForceFinish: onDebugForceFinish(); break;
            }
        }
        private void SystemManager_OnPause()
        {
            onPause();
        }
        private void SystemManager_OnResume()
        {
            onResume();
        }



        // Unity Inspectors
        [Header("¡Ú (ActivityBase) Sound")]
        [SerializeField] private AudioClip bgmCLIP = null;
        [Range(0, 100)]
        [SerializeField] int bgmVolume = 100;
        [Header("¡Ú (ActivityBase) Config")]
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private float affTimeout = 10f;

        // Unity Messages
        protected virtual void Awake()
        {
            onInitActivity();
        }
        protected virtual void Start()
        {

        }
        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1) && GetType().IsOverride("onDebugNext"))
                SystemEventManager.SystemButtonClick(SystemButtonType.Debug_Next);
            if (Input.GetKeyDown(KeyCode.F2) && GetType().IsOverride("onDebugNextStep"))
                SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextStep);
            if (Input.GetKeyDown(KeyCode.F3) && GetType().IsOverride("onDebugNextProblem"))
                SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextProblem);
            if (Input.GetKeyDown(KeyCode.F4) && GetType().IsOverride("onDebugPrevProblem"))
                SystemEventManager.SystemButtonClick(SystemButtonType.Debug_PrevProblem);
        }
        protected virtual void OnEnable()
        {
            SystemEventManager.OnSystemButtonClicked += systemEventManager_OnSystemButtonClicked;
            SystemManager.OnPause += SystemManager_OnPause;
            SystemManager.OnResume += SystemManager_OnResume;
        }
        protected virtual void OnDisable()
        {
            SystemEventManager.OnSystemButtonClicked -= systemEventManager_OnSystemButtonClicked;
            SystemManager.OnPause -= SystemManager_OnPause;
            SystemManager.OnResume -= SystemManager_OnResume;
        }
    }
}