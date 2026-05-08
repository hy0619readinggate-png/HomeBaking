using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.EBook.PlayAll
{
    public class BookLoader
    {
        // Properties
        public EBookSingleIndex EBIndex { get; private set; }
        public TableVersion VERSION { get; private set; }
        public EbookList CURRICULUM { get; private set; }
        public TableVersion PAGE_VERSION { get; private set; }
        public LayerData[] LAYERS { get; private set; }
        public RecordData[] RECORDS { get; private set; }
        public GameObject BOOK_PB { get; private set; }

        // Methods
        public void SetAssetLoadFlag(AssetLoadFlag flag)
        {
            loadFlag = flag;
        }
        public async UniTask LoadBook(EBookSingleIndex ebIDX)
        {
            LOG.Info($"LoadBook() | {ebIDX}", this);

            EBIndex = ebIDX;
            await onLoad(ebIDX);
        }



        // Fields
        private AssetLoadFlag loadFlag = AssetLoadFlag.BookPrefab | AssetLoadFlag.LayerNarration;

        // Functions
        private async UniTask onLoad(EBookSingleIndex ebIDX)
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

            // book prefab load
            if (loadFlag.HasFlag(AssetLoadFlag.BookPrefab))
                BOOK_PB = await ebIDX.LoadPrefab("Pages");
        }
        private async UniTask<LayerData[]> buildLayerData(EBookSingleIndex ebIDX, LayerRow[] layers, SequenceRow[] sequences)
        {
            var loadSound = loadFlag.HasFlag(AssetLoadFlag.LayerNarration);
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
                    LayerSPR = await ebIDX.LoadSprite(layer.Image),
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
    }
}