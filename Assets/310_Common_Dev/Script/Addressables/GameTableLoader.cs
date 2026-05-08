using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoDoEng.Common
{
    public class GameTableLoader : BYDSingleton<GameTableLoader>
    {
        // Methods
        public async UniTask<GameTableLoaderResult<T>> LoadTable<T>(GameIndex gameIDX) where T : GameData, new()
        {
            using (LOG.Coroutine($"LoadTable() | {gameIDX}", this))
            {
                // 엑셀 로드
                var tableAR = tablesAR[gameIDX.Course - 1];
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
                    var msg = $"No version info. is exist for {gameIDX}";
                    LOG.Warning(msg, this);
                    throw new NoVersionInfoException(msg);
                }
                LOG.Addressable($"VERSION : {version}", this);

                // 리스트 찾기
                var sheetName = gameIDX.GameMode == GameMode.Review
                    ? "reviewgame_list"
                    : "playground_list";
                var sheet = workbook[sheetName];
                var lists = sheet.Convert<GameList>();
                var list = lists.SingleOrDefault(c =>
                    c.GameType == gameIDX.GameType &&
                    c.GameNum == gameIDX.GameNum);

                if (list == null)
                {
                    var msg = $"No list is exist for {gameIDX}";
                    LOG.Warning(msg, this);
                    throw new NoListException(msg);
                }
                LOG.Addressable($"LIST : {list}", this);

                // 문제 데이터 찾기
                var sheetAct = workbook[$"game_data_{gameIDX.GameType}"];
                var tablesAll = sheetAct.Convert<T>();
                var tables = tablesAll.Filter(list.DataMin, list.DataMax).ToArray();
                if (tables.Length == 0)
                {
                    var msg = $"No data is exist for {gameIDX}";
                    LOG.Warning(msg, this);
                    throw new NoDataException(msg);
                }
                foreach (var t in tables)
                    LOG.Addressable($"DATA : {t}", this);

                return new GameTableLoaderResult<T>
                {
                    Version = version,
                    List = list,
                    Tables = tables
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

    public class GameTableLoaderResult<T>
    {
        public TableVersion Version;
        public GameList List;
        public T[] Tables;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class GameList
    {
        [Column("A")] public int Index;
        [Column("B")] public int GameType;
        [Column("C")] public int GameNum;
        [Column("D")] public int DataMin;
        [Column("E")] public int DataMax;

        public override string ToString()
        {
            return $"{Index} | {GameType} | {GameNum} | {DataMin} | {DataMax}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class GameData
    {
        [Column("A")] public int Index;

        public static string LOG_COLOR = "#AAAAFF";
    }

    public static class GameData_Extension
    {
        public static IEnumerable<T> Filter<T>(this IEnumerable<T> tables, int min, int max) where T : GameData
        {
            return tables.Where(d => d.Index >= min && d.Index <= max);
        }
    }
}