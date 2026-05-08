using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SRDebugger;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.EBook.Framework
{
    [RequireComponent(typeof(EBookQuizRunner))]
    [RequireComponent(typeof(EBookQuizProgress))]
    public abstract class EBookQuizBase : MonoBehaviour
    {
        // Definition
        private static string PATH_DEBUG_VIEWER = "prefab/DebugTableViewer";

        // Properties
        public bool Show_TableDebugViewer
        {
            get => tableDebugViewer != null;
            set => showTables(value);
        }

        // Methods
        public async UniTask Prepare(EBookQuizIndex ebIDX)
        {
            LOG.Info($"Prepare() | {ebIDX}", this);

            EBIndex = ebIDX;
            await onPrepare(ebIDX);
        }
        public void StartEBookQuiz()
        {
            LOG.Info($"StartEBookQuiz()", this);

#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            onStartEBookQuiz();
        }
        public void FinishEBookQuiz()
        {
            LOG.Info($"FinishEBookQuiz()", this);

#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishEBookQuiz();
        }

        // Events
        public event Action<EBookQuizResult> OnEBookQuizComplete;



        // Virtual
        protected virtual async UniTask onPrepare(EBookQuizIndex ebIDX)
        {
            var json = await ebIDX.LoadQuizJson();
            var ds = JsonConvert.DeserializeObject<EBookQuizDataWrapper>(json);

            foreach (var d in ds.QuizData)
                LOG.Info($"{d}", this);

            foreach (var qd in ds.QuizData)
                await qd.LoadResource(ebIDX);

            QUIZ_DATA = ds.QuizData.ToArray();
        }
        protected virtual void onStartEBookQuiz()
        {
            EBookQuizProgress.One.StartMeasureOfPlayingTime();
        }
        protected virtual void onFinishEBookQuiz()
        {
            DataLoader.One.ReleaseHandles();
        }
        protected virtual void onDebugNext() { }
        protected virtual void onDebugNextProblem() { }
        protected virtual void onDebugForceFinish() { }
#if !DISABLE_SRDEBUGGER
        protected virtual void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            var sort = 300;

            srOptionContainer.AddOption(
                OptionDefinition.Create("Show Table",
                () => this.Show_TableDebugViewer,
                (newValue) => this.Show_TableDebugViewer = newValue,
                "EBookQuiz", sort++));

            if (GetType().IsOverride("onDebugForceFinish"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Force Finish", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_ForceFinish);
                    }, "EBookQuiz", ++sort));
            }

            //if (GetType().IsOverride("onDebugNext"))
            //{
            //    srOptionContainer.AddOption(
            //        OptionDefinition.FromMethod("Next(F1)", () =>
            //        {
            //            SystemEventManager.SystemButtonClick(SystemButtonType.Debug_Next);
            //        }, "EBookQuiz", ++sort));
            //}
            //if (GetType().IsOverride("onDebugNextProblem"))
            //{
            //    srOptionContainer.AddOption(
            //        OptionDefinition.FromMethod("NextProblem(F3)", () =>
            //        {
            //            SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextProblem);
            //        }, "EBookQuiz", ++sort));
            //}
        }
#endif

        // Properties : for concrete
        protected EBookSingleIndex EBIndex { get; private set; }
        protected EBookQuizData[] QUIZ_DATA { get; private set; }

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
        protected void complete()
        {
            // CP가 따로 없어서, 완료시 호출
            EBookQuizProgress.One.FinishMeasureOfPlayingTime();

            OnEBookQuizComplete?.Invoke(EBookQuizProgress.One.Result);
        }



        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif
        private DebugTableViewer tableDebugViewer;

        // Functions
        private void showTables(bool show)
        {
            if (show)
            {
                var pb = Resources.Load<GameObject>(PATH_DEBUG_VIEWER);
                var go = Instantiate(pb, transform);
                tableDebugViewer = go.GetComponent<DebugTableViewer>();
                tableDebugViewer.ShowTable(QUIZ_DATA);
            }
            else Destroy(tableDebugViewer.gameObject);
        }

        // Event Handlers
        private void systemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Debug_NextProblem: onDebugNextProblem(); break;
                case SystemButtonType.Debug_Next: onDebugNext(); break;
                case SystemButtonType.Debug_ForceFinish: onDebugForceFinish(); break;
            }
        }



        // Unity Messages
        protected virtual void Awake()
        {
        }
        protected virtual void Start()
        {
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


    public class EBookQuizDataWrapper
    {
        [JsonProperty("quiz")] public List<EBookQuizData> QuizData;
    }

    [JsonConverter(typeof(EBookQuizDataConverter))]
    public abstract class EBookQuizData
    {
        [JsonProperty("Index")] public int Index;
        [JsonProperty("eBook_Index")] public int EBookIndex;
        [JsonProperty("eBook_Quiz_Num")] public int EBookQuizNum;

        // Gray Columns
        [JsonProperty("course")] public int Course;
        [JsonProperty("Type")] public int Type;
        [JsonProperty("Num")] public int Num;
        [JsonProperty("책 제목")] public string Title;

        // Methods
        public async UniTask LoadResource(EBookQuizIndex ebIDX)
        {
            await onLoadResource(ebIDX);
        }

        // Virtual
        protected abstract UniTask onLoadResource(EBookQuizIndex ebIDX);
    }
    public class EBookQuizData_Type1 : EBookQuizData
    {
        // Properties - JSON
        [JsonProperty("Question")] public string Question;
        [JsonProperty("Sound_Question")] public string SoundQuestion;
        [JsonProperty("Image_Correct")] public string ImageCorrect;
        [JsonProperty("Image_Incorrect")] public string ImageWrong;

        // Properties
        public AudioClip QuestionCLIP { get; private set; }
        public Sprite CorrectSPR { get; private set; }
        public Sprite WrongSPR { get; private set; }



        // Overrides
        protected override async UniTask onLoadResource(EBookQuizIndex ebIDX)
        {
            QuestionCLIP = await ebIDX.LoadQuizSound(SoundQuestion);
            CorrectSPR = await ebIDX.LoadQuizSprite(ImageCorrect);
            WrongSPR = await ebIDX.LoadQuizSprite(ImageWrong);
        }
        public override string ToString()
        {
            return $"<color=red>EBookQuizData_Type1 " +
                $"[{Index} | {Type} | {EBookIndex}:({Title}) | {EBookQuizNum} | " +
                $"{Question}" +
                $"]" +
                $"</color>";
        }
    }
    public class EBookQuizData_Type2 : EBookQuizData
    {
        // Properties - JSON
        [JsonProperty("Question")] public string Question;
        [JsonProperty("Image_Question")] public string ImageQuestion;
        [JsonProperty("Sound_Question")] public string SoundQuestion;
        [JsonProperty("Text_Correct")] public string TextCorrect;
        [JsonProperty("Text_Incorrect")] public string TextWrong;

        // Properties
        public Sprite QuestionSPR { get; private set; }
        public AudioClip QuestionCLIP { get; private set; }



        // Overrides
        protected override async UniTask onLoadResource(EBookQuizIndex ebIDX)
        {
            QuestionSPR = await ebIDX.LoadQuizSprite(ImageQuestion);
            QuestionCLIP = await ebIDX.LoadQuizSound(SoundQuestion);
        }
        public override string ToString()
        {
            return $"<color=red>EBookQuizData_Type2 " +
                $"[{Index} | {Type} | {EBookIndex}:({Title}) | {EBookQuizNum} | " +
                $"{Question}" +
                $"]" +
                $"</color>";
        }
    }
    public class EBookQuizData_Type3 : EBookQuizData
    {
        // Properties - JSON
        [JsonProperty("Text_Story_1")] public string TextStory1;
        [JsonProperty("Text_Story_2")] public string TextStory2;
        [JsonProperty("Text_Story_3")] public string TextStory3;
        [JsonProperty("Image_Story_1")] public string ImageStory1;
        [JsonProperty("Image_Story_2")] public string ImageStory2;
        [JsonProperty("Image_Story_3")] public string ImageStory3;
        [JsonProperty("Sound_Story_1")] public string SoundStory1;
        [JsonProperty("Sound_Story_2")] public string SoundStory2;
        [JsonProperty("Sound_Story_3")] public string SoundStory3;

        // Properties
        public string[] StoryText => new string[] { TextStory1, TextStory2, TextStory3 };
        public Sprite[] StorySPR => storySPR;
        public AudioClip[] StoryCLIP => storyCLIP;



        // Fields
        private Sprite[] storySPR = new Sprite[3];
        private AudioClip[] storyCLIP = new AudioClip[3];

        // Overrides
        protected override async UniTask onLoadResource(EBookQuizIndex ebIDX)
        {
            storySPR[0] = await ebIDX.LoadQuizSprite(ImageStory1);
            storySPR[1] = await ebIDX.LoadQuizSprite(ImageStory2);
            storySPR[2] = await ebIDX.LoadQuizSprite(ImageStory3);
            storyCLIP[0] = await ebIDX.LoadQuizSound(SoundStory1);
            storyCLIP[1] = await ebIDX.LoadQuizSound(SoundStory2);
            storyCLIP[2] = await ebIDX.LoadQuizSound(SoundStory3);
        }
        public override string ToString()
        {
            return $"<color=red>EBookQuizData_Type3 " +
                $"[{Index} | {Type} | {EBookIndex}:({Title}) | {EBookQuizNum} | " +
                $"{TextStory1} | {TextStory2} | {TextStory3}" +
                $"]" +
                $"</color>";
        }
    }
    public class EBookQuizData_Type4 : EBookQuizData
    {
        // Properties - JSON
        [JsonProperty("Text_Story_1")] public string TextStory1;
        [JsonProperty("Text_Story_2")] public string TextStory2;
        [JsonProperty("Text_Story_3")] public string TextStory3;
        [JsonProperty("Text_Story_4")] public string TextStory4;
        [JsonProperty("Image_Story_1")] public string ImageStory1;
        [JsonProperty("Image_Story_2")] public string ImageStory2;
        [JsonProperty("Image_Story_3")] public string ImageStory3;
        [JsonProperty("Image_Story_4")] public string ImageStory4;
        [JsonProperty("Sound_Story_1")] public string SoundStory1;
        [JsonProperty("Sound_Story_2")] public string SoundStory2;
        [JsonProperty("Sound_Story_3")] public string SoundStory3;
        [JsonProperty("Sound_Story_4")] public string SoundStory4;

        // Properties
        public string[] StoryText => new string[] { TextStory1, TextStory2, TextStory3, TextStory4 };
        public Sprite[] StorySPR => storySPR;
        public AudioClip[] StoryCLIP => storyCLIP;



        // Fields
        private Sprite[] storySPR = new Sprite[4];
        private AudioClip[] storyCLIP = new AudioClip[4];

        // Overrides
        protected override async UniTask onLoadResource(EBookQuizIndex ebIDX)
        {
            storySPR[0] = await ebIDX.LoadQuizSprite(ImageStory1);
            storySPR[1] = await ebIDX.LoadQuizSprite(ImageStory2);
            storySPR[2] = await ebIDX.LoadQuizSprite(ImageStory3);
            storySPR[3] = await ebIDX.LoadQuizSprite(ImageStory4);
            storyCLIP[0] = await ebIDX.LoadQuizSound(SoundStory1);
            storyCLIP[1] = await ebIDX.LoadQuizSound(SoundStory2);
            storyCLIP[2] = await ebIDX.LoadQuizSound(SoundStory3);
            storyCLIP[3] = await ebIDX.LoadQuizSound(SoundStory4);
        }
        public override string ToString()
        {
            return $"<color=red>EBookQuizData_Type4 " +
                $"[{Index} | {Type} | {EBookIndex}:({Title}) | {EBookQuizNum} | " +
                $"{TextStory1} | {TextStory2} | {TextStory3} | {TextStory4}" +
                $"]" +
                $"</color>";
        }
    }
    public class EBookQuizData_Type5 : EBookQuizData
    {
        // Properties - JSON
        [JsonProperty("Question")] public string Question;
        [JsonProperty("Image_Question")] public string ImageQuestion;
        [JsonProperty("Sound_Question")] public string SoundQuestion;
        [JsonProperty("True&False")] public bool IsTrue;

        // Properties
        public Sprite QuestionSPR { get; private set; }
        public AudioClip QuestionCLIP { get; private set; }



        // Overrides
        protected override async UniTask onLoadResource(EBookQuizIndex ebIDX)
        {
            QuestionSPR = await ebIDX.LoadQuizSprite(ImageQuestion);
            QuestionCLIP = await ebIDX.LoadQuizSound(SoundQuestion);
        }
        public override string ToString()
        {
            return $"<color=red>EBookQuizData_Type5 " +
                $"[{Index} | {Type} | {EBookIndex}:({Title}) | {EBookQuizNum} | " +
                $"{Question} | {IsTrue}" +
                $"]" +
                $"</color>";
        }
    }
    public class EBookQuizData_Type6 : EBookQuizData
    {
        // Properties - JSON
        [JsonProperty("Text_Cause")] public string TextCause;
        [JsonProperty("Text_Effect")] public string TextEffect;
        [JsonProperty("Image_Cause")] public string ImageCause;
        [JsonProperty("Image_Effect")] public string ImageEffect;
        [JsonProperty("Sound_Cause")] public string SoundCause;
        [JsonProperty("Sound_Effect")] public string SoundEffect;

        // Properties
        public Sprite[] StorySPR => storySPR;
        public AudioClip[] StoryCLIP => storyCLIP;



        // Fields
        private Sprite[] storySPR = new Sprite[2];
        private AudioClip[] storyCLIP = new AudioClip[2];

        // Overrides
        protected override async UniTask onLoadResource(EBookQuizIndex ebIDX)
        {
            storySPR[0] = await ebIDX.LoadQuizSprite(ImageCause);
            storySPR[1] = await ebIDX.LoadQuizSprite(ImageEffect);
            storyCLIP[0] = await ebIDX.LoadQuizSound(SoundCause);
            storyCLIP[1] = await ebIDX.LoadQuizSound(SoundEffect);
        }
        public override string ToString()
        {
            return $"<color=red>EBookQuizData_Type6 " +
                $"[{Index} | {Type} | {EBookIndex}:({Title}) | {EBookQuizNum} | " +
                $"{TextCause} | {TextEffect}" +
                $"]" +
                $"</color>";
        }
    }


    public class EBookQuizDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(EBookQuizData).IsAssignableFrom(objectType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObj = JObject.Load(reader);
            var type = (int)jObj["Type"];

            EBookQuizData data = null;
            switch (type)
            {
                case 1: data = new EBookQuizData_Type1(); break;
                case 2: data = new EBookQuizData_Type2(); break;
                case 3: data = new EBookQuizData_Type3(); break;
                case 4: data = new EBookQuizData_Type4(); break;
                case 5: data = new EBookQuizData_Type5(); break;
                case 6: data = new EBookQuizData_Type6(); break;
            }

            serializer.Populate(jObj.CreateReader(), data);
            return data;
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }
    }
}