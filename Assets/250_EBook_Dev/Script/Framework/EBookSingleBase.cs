using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using FlexFramework.Excel;
using SRDebugger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.EBook.Framework
{
    public enum Direction { Next, Prev };
    [Flags]
    public enum AssetLoadFlag : int
    {
        None = 0,
        BookPrefab = 1 << 1,
        LayerNarration = 1 << 2,
        LayerImage = 1 << 3,
        NatvieNarration = 1 << 4,

        All = int.MaxValue
    }
    [RequireComponent(typeof(EBookSingleRunner))]
    [RequireComponent(typeof(EBookSingleProgress))]
    public abstract class EBookSingleBase : MonoBehaviour
    {
        // Definition
        public static string PATH_Avatar_TMP_SpritePath = "EBook_Avatars/Avatars"; // For Resources
        private string PATH_DEBUG_VIEWER = "prefab/DebugTableViewer";

        // Properties
        public bool Show_TableDebugViewer
        {
            get => tableDebugViewer != null;
            set => showTables(value);
        }
        public string Title => CURRICULUM.Title;

        // Methods
        public async UniTask Prepare(EBookSingleIndex ebIDX)
        {
            LOG.Info($"Prepare() | {ebIDX}", this);

            EBIndex = ebIDX;
            await onPrepare(ebIDX);
        }
        public void StartEBook()
        {
            LOG.Info($"StartEBook()", this);

#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();
            onBuildOption(srOptionContainer);
            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif

            onStartEBook();
        }
        public void FinishEBook()
        {
            LOG.Info($"FinishEBook()", this);

#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif

            onFinishEBook();
        }

        // Events
        public event Action<EBookSingleResult> OnEBookComplete;
        public event Action OnEBookError;



        // Virtual
        protected virtual async UniTask onPrepare(EBookSingleIndex ebIDX)
        {
            // table load
            var tableResult = await EBookTableLoader.One.LoadTable(ebIDX);
            VERSION = tableResult.Version;
            CURRICULUM = tableResult.List;

            // page load
            var pageResult = await EBookTableLoader.One.LoadLayer(ebIDX);
            PAGE_VERSION = pageResult.Version;
            LAYERS = await buildLayerData(ebIDX, pageResult.Layers, pageResult.Sequences);
            RECORDS = await buildRecordData(ebIDX, pageResult.Records);

            if (loadFlag.HasFlag(AssetLoadFlag.BookPrefab))
                BOOK_PB = await ebIDX.LoadPrefab("Pages");
        }
        protected virtual void onStartEBook()
        {
            EBookSingleProgress.One.StartMeasureOfPlayingTime();
        }
        protected virtual void onFinishEBook()
        {
            DataLoader.One.ReleaseHandles();
        }
        protected virtual void onDebugForceFinish() { }
        protected virtual void onDebugMoveLayer(Direction direction) { LOG.Important($"NOT Implemented!!", this); }
#if !DISABLE_SRDEBUGGER
        protected virtual void onBuildOption(DynamicOptionContainer srOptionContainer)
        {
            var sort = 300;

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Prev Layer", () =>
                {
                    SystemEventManager.SystemButtonClick(SystemButtonType.Debug_PrevLayer);
                }, "EBook", sort++));

            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Next Layer", () =>
                {
                    SystemEventManager.SystemButtonClick(SystemButtonType.Debug_NextLayer);
                }, "EBook", sort++));

            srOptionContainer.AddOption(
                OptionDefinition.Create("Show Table",
                () => this.Show_TableDebugViewer,
                (newValue) => this.Show_TableDebugViewer = newValue,
                "EBook", 400));

            if (GetType().IsOverride("onDebugForceFinish"))
            {
                srOptionContainer.AddOption(
                    OptionDefinition.FromMethod("Force Finish", () =>
                    {
                        SystemEventManager.SystemButtonClick(SystemButtonType.Debug_ForceFinish);
                    }, "EBook", ++sort));
            }
        }
#endif

        // Properties : for concrete
        protected EBookSingleIndex EBIndex { get; private set; }
        protected TableVersion VERSION { get; private set; }
        protected EbookList CURRICULUM { get; private set; }
        protected TableVersion PAGE_VERSION { get; private set; }
        protected LayerData[] LAYERS { get; private set; }
        protected RecordData[] RECORDS { get; private set; }
        protected GameObject BOOK_PB { get; private set; }

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
        protected void setAssetLoadFlag(AssetLoadFlag flag)
        {
            loadFlag = flag;
        }
        protected void complete()
        {
            // CP가 따로 없어서, 완료시 호출
            EBookSingleProgress.One.FinishMeasureOfPlayingTime();

            OnEBookComplete?.Invoke(EBookSingleProgress.One.Result);
        }
        protected void error()
        {
            OnEBookError?.Invoke();
        }



        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif
        private DebugTableViewer tableDebugViewer;
        private AssetLoadFlag loadFlag = AssetLoadFlag.BookPrefab | AssetLoadFlag.LayerNarration;

        // Functions
        private void showTables(bool show)
        {
            if (show)
            {
                var pb = Resources.Load<GameObject>(PATH_DEBUG_VIEWER);
                var go = Instantiate(pb, transform);
                tableDebugViewer = go.GetComponent<DebugTableViewer>();
                tableDebugViewer.ShowTable(VERSION, CURRICULUM, PAGE_VERSION, LAYERS);
            }
            else Destroy(tableDebugViewer.gameObject);
        }
        private async UniTask<LayerData[]> buildLayerData(EBookSingleIndex ebIDX, LayerRow[] layers, SequenceRow[] sequences)
        {
            var loadSound = loadFlag.HasFlag(AssetLoadFlag.LayerNarration);
            var loadImage = loadFlag.HasFlag(AssetLoadFlag.LayerImage);

            var layerNos = layers.Select(p => p.LayerNo).Distinct();

            var layerDataList = new List<LayerData>();
            foreach (var layerNo in layerNos)
            {
                var setenceDataList = new List<SentenceData>();

                var sentences = layers.Where(l => l.LayerNo == layerNo);
                foreach (var sentence in sentences)
                {
                    var seq = sequences
                        .Where(s => s.LayerNo == layerNo && s.SentenceNo == sentence.SentenceNo)
                        .Select(s => new SequenceData
                        {
                            SequenceNo = s.SequenceNo,
                            StartTimeSec = s.StartTime / 1000f
                        });

                    setenceDataList.Add(new SentenceData
                    {
                        LayerNo = layerNo,
                        SentenceNo = sentence.SentenceNo,
                        Sentence = sentence.Sentence,
                        SoundCLIP = loadSound ? await ebIDX.LoadSound(sentence.Sound) : null,
                        Sequences = seq.ToArray()
                    }); ;
                }

                var layer = layers.First(l => l.LayerNo == layerNo);
                layerDataList.Add(new LayerData
                {
                    LayerNo = layerNo,
                    LayerSPR = loadImage ? await ebIDX.LoadSprite(layer.Image) : null,
                    Sentences = setenceDataList.ToArray()
                });
            }

            return layerDataList.ToArray();
        }
        private async UniTask<RecordData[]> buildRecordData(EBookSingleIndex ebIDX, RecordRow[] records)
        {
            var loadSound = loadFlag.HasFlag(AssetLoadFlag.NatvieNarration);

            var recordList = new List<RecordData>();
            foreach (var record in records)
            {
                recordList.Add(new RecordData
                {
                    LayerNo = record.LayerNo,
                    SentenceNo = record.SentenceNo,
                    SequenceNo = record.SequenceNo,
                    Sentence = record.Sentence,
                    SentenceCLIP = loadSound ? await ebIDX.LoadSound(record.SoundSentence) : null,
                    RecordSoundName = record.SoundRecord
                });
            }

            return recordList.ToArray();
        }

        // Event Handlers
        private void systemEventManager_OnSystemButtonClicked(SystemButtonType buttonType)
        {
            switch (buttonType)
            {
                case SystemButtonType.Debug_PrevLayer: onDebugMoveLayer(Direction.Prev); break;
                case SystemButtonType.Debug_NextLayer: onDebugMoveLayer(Direction.Next); break;
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
        protected virtual void Update()
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

    public class LayerData
    {
        public int LayerNo;
        public Sprite LayerSPR;
        public SentenceData[] Sentences;

        public override string ToString()
        {
            return $"<color=red>LayerDATA" +
                $"[{LayerNo} | {string.Join(",", Sentences.Select(s => s.Sentence))}]" +
                $"</color>";
        }
    }
    public class SentenceData
    {
        public int LayerNo;
        public int SentenceNo;
        public string Sentence;
        public AudioClip SoundCLIP;
        public SequenceData[] Sequences;

        public override string ToString()
        {
            return $"<color=red>SentenceDATA" +
                $"[{SentenceNo} | {Sentence} | {string.Join(",", Sequences.Select(s => s.StartTimeSec))}]" +
                $"</color>";
        }
    }
    public class SequenceData
    {
        public int SequenceNo;
        public float StartTimeSec;
    }

    public class RecordData
    {
        public int LayerNo;
        public int SentenceNo;
        public int SequenceNo;
        public string Sentence;
        public AudioClip SentenceCLIP;
        public string RecordSoundName;

        public override string ToString()
        {
            return $"<color=red>RecordDATA" +
                $"[{LayerNo} | {SentenceNo} | {Sentence}]" +
                $"</color>";
        }
    }

    public class PageData
    {
        public int Layer;
        public int Page;
        public string Sentence;
        public AudioClip SoundCLIP;
        public SequenceData[] Sequences;

        public override string ToString()
        {
            return $"{Layer} | {Page} | {Sentence} | {SoundCLIP.name}";
        }
    }
}