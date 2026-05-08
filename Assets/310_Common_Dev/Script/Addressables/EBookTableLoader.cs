using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoDoEng.Common
{
    public class EBookTableLoader : BYDSingleton<EBookTableLoader>
    {
        // Methods
        public async UniTask<EbookTableLoadResult> LoadTable(EBookSingleIndex ebookIDX)
        {
            using (LOG.Coroutine($"LoadTable() | {ebookIDX}", this))
            {
                // 엑셀 로드
                var hLoad = tableAR.LoadAssetAsync();
                await hLoad.Task;

                var xlsx = hLoad.Result;

                Addressables.Release(hLoad);

                // 버전 정보 일기
                var workbook = new WorkBook(xlsx.Bytes);
                var sheetVersion = workbook["version"];
                var versions = sheetVersion.Convert<TableVersion>();
                var version = versions.LastOrDefault();
                if (version == null)
                {
                    var msg = $"No version info. is exist for {ebookIDX}";
                    LOG.Warning(msg, this);
                    throw new NoVersionInfoException(msg);
                }
                LOG.Addressable($"VERSION : {version}", this);

                // 리스트 찾기
                var sheet = workbook["ebook_list"];
                var lists = sheet.Convert<EbookList>();
                var list = lists.SingleOrDefault(c => c.Index == ebookIDX.Index);
                if (list == null)
                {
                    var msg = $"No list is exist for {ebookIDX}";
                    LOG.Warning(msg, this);
                    throw new NoListException(msg);
                }
                LOG.Addressable($"LIST : {list}", this);

                return new EbookTableLoadResult
                {
                    Version = version,
                    List = list,
                };
            }
        }
        public async UniTask<EBookLayerLoadResult> LoadLayer(EBookSingleIndex ebIDX)
        {
            using (LOG.Coroutine($"LoadLayer() | {ebIDX}", this))
            {
                // 엑셀 로드
                var address = ebIDX.DownloadDataPath; // example : 20000001/20000001_pages.xlsx
                var hLoad = Addressables.LoadAssetAsync<XlsxAsset>(address);
                await hLoad.Task;

                var xlsx = hLoad.Result;

                Addressables.Release(hLoad);

                // 버전 정보 일기
                var workbook = new WorkBook(xlsx.Bytes);
                var sheetVersion = workbook["version"];
                var versions = sheetVersion.Convert<TableVersion>();
                var version = versions.LastOrDefault();
                if (version == null)
                {
                    var msg = $"No version info. is exist for {ebIDX}";
                    LOG.Warning(msg, this);
                    throw new NoVersionInfoException(msg);
                }
                LOG.Addressable($"VERSION : {version}", this);

                // 페이지 데이터 로드
                var sheetLayer = workbook["page"];
                var layers = sheetLayer.Convert<LayerRow>();

                if (layers.Count() == 0)
                {
                    var msg = $"No page table is exist for {ebIDX}";
                    LOG.Warning(msg, this);
                    throw new NoPageException(msg);
                }
                foreach (var l in layers)
                    LOG.Addressable($"LAYER : {l}", this);

                // 시퀀스 데이터 로드
                var sheetSeq = workbook["sequence"];
                var sequences = sheetSeq.Convert<SequenceRow>();

                if (sequences.Count() == 0)
                {
                    var msg = $"No sequence table is exist for {ebIDX}";
                    LOG.Warning(msg, this);
                    throw new NoSequenceException(msg);
                }
                foreach (var s in sequences)
                    LOG.Addressable($"SEQ : {s}", this);

                // 녹음용 데이터 로드
                var sheetRec = workbook["record"];
                var records = sheetRec.Convert<RecordRow>();

                if (records.Count() == 0)
                {
                    var msg = $"No record table is exist for {ebIDX}";
                    LOG.Warning(msg, this);
                    throw new NoRecordException(msg);
                }
                foreach (var r in records)
                    LOG.Addressable($"REC : {r}", this);

                return new EBookLayerLoadResult
                {
                    Version = version,
                    Layers = layers.ToArray(),
                    Sequences = sequences.ToArray(),
                    Records = records.ToArray()
                };
            }
        }

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AssetReferenceT<XlsxAsset> tableAR = null;
    }

    // Table
    public class EbookTableLoadResult
    {
        public TableVersion Version;
        public EbookList List;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class EbookList
    {
        [Column("A")] public string Index;
        [Column("B")] public int CategoryMain;
        [Column("C")] public int CategorySub;
        [Column("D")] public int EbookNum;
        [Column("E")] public string Title;

        public override string ToString()
        {
            return $"{Index} | {Title}";
        }
    }

    // Layer
    public class EBookLayerLoadResult
    {
        public TableVersion Version;
        public LayerRow[] Layers;
        public SequenceRow[] Sequences;
        public RecordRow[] Records;
    }
    [Serializable, Table(1, SafeMode = true)]
    public class LayerRow
    {
        [Column("A")] public int LayerNo;
        [Column("B")] public int SentenceNo;
        [Column("C")] public string Sentence;
        [Column("D")] public string Image;
        [Column("E")] public string Sound;

        public override string ToString()
        {
            return $"{LayerNo} | {SentenceNo} | {Sentence} | {Sound}";
        }
    }
    [Serializable, Table(1, SafeMode = true)]
    public class SequenceRow
    {
        [Column("A")] public int LayerNo;
        [Column("B")] public int SentenceNo;
        [Column("C")] public int SequenceNo;
        [Column("D")] public int StartTime;

        public override string ToString()
        {
            return $"{LayerNo} | {SentenceNo} | {SequenceNo} | {StartTime}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class RecordRow
    {
        [Column("A")] public int LayerNo;
        [Column("B")] public int SentenceNo;
        [Column("C")] public int SequenceNo;
        [Column("D")] public string Sentence;
        [Column("E")] public string SoundSentence;
        [Column("F")] public string SoundRecord;

        public override string ToString()
        {
            return $"{LayerNo} | {SentenceNo} | {SequenceNo} | {Sentence}";
        }
    }
}