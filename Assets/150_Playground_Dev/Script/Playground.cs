using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using DoDoEng.Playground.Behaviour;
using DoDoEng.Playground.UI;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;
using DoDoEng.Playground.Framework;
using SRDebugger;
using DG.Tweening;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;

namespace DoDoEng.Playground
{
    [RequireComponent(typeof(PlaygroundRunner))]
    [RequireComponent(typeof(PlaygroundTester))]
    public class Playground : MonoBehaviour
    {
        // Definitions
        private enum Complete_Score
        {
            Score0,
            Score1,
            Score2,
            Score3,
        }

        // Properties

        // Methods
        public async UniTask Prepare()
        {
            LOG.Info($"Prepare()", this);

            await onPrepare();
        }
        public void StartPlayground()
        {
            LOG.Info($"StartPlayground()", this);

#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            DOVirtual.DelayedCall(startDelay, () => { onStartGame(); });
        }
        public void FinishPlayground()
        {
            LOG.Info($"FinishPlayground()", this);

#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishGame();
        }



        // Virtual
        protected virtual void onInitPlayground()
        {
            LOG.Function(this);

            UserData.One.LoadLanguage();
            UIPlaygroundCommon.One.VisibleBackButton = true;
        }
        protected virtual async UniTask onPrepare()
        {
            await loadSlots();

            LOG.Function(this, $"| idxCurrentSlot={idxCurrentSlot}");

            var currentSlot = slots[idxCurrentSlot];
            float mapSize = mapRT.sizeDelta.x;
            float screenSize = scrollRect.GetComponent<RectTransform>().sizeDelta.x;
            float slotX = currentSlot.GetComponent<RectTransform>().localPosition.x + currentSlot.transform.parent.GetComponent<RectTransform>().localPosition.x;
            float dest = (slotX - (screenSize / 2.0f)) / mapSize;
            LOG.Info($"mapSize={mapSize}, screenSize={screenSize}, slotX ={slotX}, dest={dest}", this);
            scrollRect.horizontalScrollbar.value = dest;
        }
        protected virtual void onStartGame()
        {
            LOG.Function(this);
        }
        protected virtual void onFinishGame()
        {
            LOG.Function(this);
        }
#if !DISABLE_SRDEBUGGER
        protected virtual void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            var sort = 400;

            srOptionContainer.AddOption(OptionDefinition.Create("Score", () => scoreForCompleteDebug, (value) => scoreForCompleteDebug = value, "LMS", ++sort));
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Complete Current Slot", () =>
                {
                    completeCurrentSlot((int)scoreForCompleteDebug).Forget();
                }, "LMS", ++sort)
            );
            //srOptionContainer.AddOption(
            //    OptionDefinition.FromMethod("Clear Playground Scores", () =>
            //    {
            //        clearScores().Forget();
            //    }, "LMS", ++sort)
            //);
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Add Candy", () =>
                {
                    LMS.One.GetCandy().Forget();
                }, "LMS", ++sort)
            );
        }
#endif



        // Fields : caching

        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif
        int idxCurrentSlot;
        private Complete_Score scoreForCompleteDebug = Complete_Score.Score0;

        // Functions
        private async UniTask loadSlots()
        {
            LOG.Function(this);

            var data = await LMS.One.LoadPlaygroundData();
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i].Init(i + 1);
                if (i < data.Count)
                {
                    var courseId = data[i].Value<int>("courseId");
                    var contentIndex = data[i].Value<int>("contentIndex");
                    var curriculumId = data[i].Value<int>("curriculumId");
                    var isComplete = data[i].Value<bool>("isComplete");
                    var stars = data[i].Value<int>("stars");

                    slots[i].Course = courseId;
                    slots[i].Index = new GameIndex(contentIndex.ToString(), curriculumId);
                    slots[i].IsComplete = isComplete;
                    slots[i].Stars = stars;
                }
                else
                {
                    LOG.Warning($"PlaygroundCodes count is not match to stageButtons count", this);
                }
            }

            idxCurrentSlot = -1;
            var courseLearningHistory = UserData.One.Child.DayProgress.Value<JArray>("courseLearningHistory");
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].Course == UserData.One.Child.Course)
                {
                    if (idxCurrentSlot == -1)
                    {
                        if (slots[i].IsComplete && slots[i].Stars > 0)
                        {
                            slots[i].State = PlaygroundSlot.StateType.Enabled;
                            if (i + 1 == slots.Length || slots[i + 1].Course != UserData.One.Child.Course)
                            {
                                idxCurrentSlot = i;
                            }
                        }
                        else
                        {
                            idxCurrentSlot = i;
                            slots[i].State = PlaygroundSlot.StateType.Current;
                        }
                    }
                    else
                    {
                        slots[i].State = PlaygroundSlot.StateType.Disabled;
                    }
                    slots[i].Lock = false;
                }
                else
                {
                    if (courseLearningHistory[slots[i].Course - 1].Value<bool>("isComplete"))
                    {
                        if (slots[i].IsComplete && slots[i].Stars > 0)
                            slots[i].State = PlaygroundSlot.StateType.Enabled;
                        else if (i == 0 || (slots[i - 1].IsComplete && slots[i - 1].Stars > 0))
                            slots[i].State = PlaygroundSlot.StateType.Previous;
                        else
                            slots[i].State = PlaygroundSlot.StateType.Disabled;
                        slots[i].Lock = false;
                    }
                    else
                    {
                        slots[i].State = PlaygroundSlot.StateType.Disabled;
                        slots[i].Lock = true;
                    }
                }
            }
        }
        private async UniTask playSlot(PlaygroundSlot slot)
        {
            slots.ForEach(slot => slot.EnableInteract = false);
            AudioMGR.One.PlayEffect(slotTouchSFX);

            int candy = await LMS.One.LoadCandy();
            if (candy > 0)
            {
                if (await UIPlaygroundCommon.One.StartPU.ShowPopup(slot.Num, slot.Index) == SimplePopupResult.Yes)
                {
                    await LMS.One.UseCandy();
                    AudioMGR.One.PlayEffect(useCandyCLIP);
                    await UniTask.Delay(1000);
                    //await UIPlaygroundCommon.One.HowToPlayPopup.ShowPopup();
                    await downloadAndStart(slot);
                }
            }
            else
            {
                UIPlaygroundCommon.One.LimitPopup?.ShowPopup().Forget();
            }
            slots.ForEach(slot => slot.EnableInteract = true);
        }
        private async UniTask downloadAndStart(PlaygroundSlot slot)
        {
            try
            {
                var size = await DataDownloader.One.GetDataDownloadSize(slot.Index);
                if (size > 0)
                {
                    var result = await SystemUI.One.DownloadConfirmPU.ShowPopup(size);
                    if (result != SimplePopupResult.Yes)
                        return;

                    canvasGroup.blocksRaycasts = false;
                    await DataDownloader.One.DownloadData(slot.Index, SystemUI.One.DownloadPU);
                }

                canvasGroup.blocksRaycasts = false;

                RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
                RunnerParam.PlaygroundSlotNum = slot.Num;
                RunnerParam.SelectedIDX = slot.Index;
                RunnerParam.PlaygroundNexts = slots.Where((v, i) => v.Course == slot.Course && i >= slot.Num).Select(v => v.Index).ToArray();
                RunnerParam.PlaygroundNexts.ForEach(next => LOG.Info($"Next Index={next}", this));
                RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
                SceneLoader.One.LoadScene(slot.Index.SceneName);
                FinishPlayground();
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.Message}", this);
                LOG.Error($"Cannot run any more because of an error!!!!", this);

                await SystemUI.One.ErrorPU.ShowPopup(ex.Message);
            }
        }
        //private async UniTask clearScores()
        //{
        //    await LMS.One.ClearPlaygroundScores();
        //    await loadSlots();
        //}
        private async UniTask completeCurrentSlot(int score)
        {
            if (idxCurrentSlot != -1)
            {
                var slot = slots[idxCurrentSlot];
                await LMS.One.CompletePlaygroundGame(((GameIndex)slot.Index).CurriculumId, 10, score);
                await loadSlots();
            }
        }

        // Event Handlers
        private void slot_OnClick(PlaygroundSlot slot)
        {
            LOG.Info($"slot_OnClick({slot})", this);

            if (!slot.Lock)
            {
                if (slot.State == PlaygroundSlot.StateType.Disabled)
                    SystemUI.One.ShowPopupCannotPlayground().Forget();
                else
                    playSlot(slot).Forget();
            }
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CanvasGroup canvasGroup = null;
        [SerializeField] private PlaygroundSlot[] slots;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform mapRT;
        [Header("★ Sound")]
        [SerializeField] private AudioClip bgmCLIP = null;
        [SerializeField] private AudioClip useCandyCLIP = null;
        [SerializeField] private SfxMoment slotTouchSFX = SfxMoment.Common_Click;
        [Header("★ Config")]
        [SerializeField] private float startDelay = 0.5f;

        // Unity Messages
        private void Awake()
        {
            onInitPlayground();
        }
        private void Start()
        {
            AudioMGR.One.PlayBGM(bgmCLIP);
        }
        protected void OnEnable()
        {
            slots.ForEach(slot => slot.OnClick += slot_OnClick);
        }
        protected void OnDisable()
        {
            slots.ForEach(slot => slot.OnClick -= slot_OnClick);
        }

        // Unity Coroutine
    }
}