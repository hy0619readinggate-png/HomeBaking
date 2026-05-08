using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoDoEng.Common
{
    public class ActivityTableLoader : BYDSingleton<ActivityTableLoader>
    {
        // Methods
        public async UniTask<ActivityTableLoaderResult<T>> LoadTable<T>(ActivityIndex actIDX) where T : ActivityData, new()
        {
            LOG.Assert(actIDX.ActivityID != ActivityID.NA, "ActivityID must not be [NA]", this);

            using (LOG.Coroutine($"LoadTable() | {actIDX}", this))
            {
                // 엑셀 로드
                int index = actIDX.Course - 1;
                LOG.Info($"Course: {actIDX.Course}, Index: {index}, tablesAR.Length: {tablesAR.Length}", this);
                
                if (index < 0 || index >= tablesAR.Length)
                {
                    LOG.Error($"Invalid Course index: {actIDX.Course} (adjusted to {index}), tablesAR length: {tablesAR.Length}", this);
                    throw new IndexOutOfRangeException($"Course {actIDX.Course} is out of range for tablesAR");
                }
                
                var tableAR = tablesAR[index];
                if (tableAR == null)
                {
                    LOG.Error($"tableAR at index {index} is null", this);
                    throw new NullReferenceException($"tableAR at index {index} is null");
                }
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
                    var msg = $"No version info. is exist for {actIDX}";
                    LOG.Warning(msg, this);
                    throw new NoVersionInfoException(msg);
                }
                LOG.Addressable($"VERSION : {version}", this);

                // 리스트 찾기
                var sheet = workbook["activity_list"];
                var lists = sheet.Convert<ActivityList>();
                var list = lists.SingleOrDefault(c =>
                    c.ActivityType == actIDX.ActivityType &&
                    c.ActivityNum == actIDX.ActivityNum);

                if (list == null)
                {
                    var msg = $"No list is exist for {actIDX}";
                    LOG.Warning(msg, this);
                    throw new NoListException(msg);
                }
                LOG.Addressable($"LIST : {list}", this);

                // 문제 데이터 찾기
                var sheetAct = workbook[$"activity_data_{actIDX.ActivityType}"];
                var tablesAll = sheetAct.Convert<T>();

                var tablesInList = tablesAll.Filter(list.DataMin, list.DataMax);
                var groups = tablesInList.Select(t => t.Group).Distinct().ToArray();
                var tables = tablesAll.Where(t => groups.Contains(t.Group)).ToList();

                // 추가 문제 데이터 찾기
                var extraTables = tablesAll.Filter(list.ExtraDataMin, list.ExtraDataMax);
                tables.AddRange(extraTables);

                // 문제와 추가 문제까지 합쳐서 중복제거
                tables = tables.DistinctBy(t => t.Index).ToList(); 

                if (tables.Count() == 0)
                {
                    var msg = $"No data is exist for {actIDX}";
                    LOG.Warning(msg, this);
                    throw new NoDataException(msg);
                }
                foreach (var t in tables)
                    LOG.Addressable($"DATA : {t}", this);

                return new ActivityTableLoaderResult<T>
                {
                    Version = version,
                    List = list,
                    Tables = tables.ToArray(),
                };
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AssetReferenceT<XlsxAsset>[] tablesAR = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
    }

    public class ActivityTableLoaderResult<T>
    {
        public TableVersion Version;
        public ActivityList List;
        public T[] Tables;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class ActivityList
    {
        [Column("A")] public int Index;
        [Column("B")] public int ActivityType;
        [Column("C")] public int ActivityNum;
        [Column("D")] public int DataMin;
        [Column("E")] public int DataMax;
        [Column("F", 0)] public int ProblemCount;
        [Column("G")] public int ExtraDataMin;
        [Column("H")] public int ExtraDataMax;
        [Column("I", 0)] public int ExtraProblemCount;

        public override string ToString()
        {
            return $"{Index} | {ActivityType} | {ActivityNum} | {DataMin} | {DataMax} | {ProblemCount} | " +
                    $"{ExtraDataMin} | {ExtraDataMax} | {ExtraProblemCount}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData
    {
        [Column("A")] public int Index;
        [Column("B")] public int Group;

        public static string LOG_COLOR = "#AAAAFF";
    }

    public static class ActivityData_Extension
    {
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> tables, int min, int max) where T : ActivityData
        {
            return tables.Where(d => d.Index >= min && d.Index <= max);
        }
    }
}