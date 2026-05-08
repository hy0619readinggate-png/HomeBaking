using beyondi.Behaviour;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using FlexFramework.Excel;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DoDoEng.Common
{
    public class CourseTableLoader : BYDSingleton<CourseTableLoader>
    {
        // Properties
        public CourseTableLoaderResult Table { get; private set; }

        // Methods
        public async UniTask<CourseTableLoaderResult> LoadTable()
        {
            using (LOG.Coroutine($"LoadTable()", this))
            {
                if (Table == null)
                {
                    // 엑셀 로드
                    var hLoad = tableAR.LoadAssetAsync();
                    await hLoad.Task;

                    var xlsx = hLoad.Result;

                    Addressables.Release(hLoad);

                    // 버전 정보 읽기
                    var workbook = new WorkBook(xlsx.Bytes);
                    var sheetVersion = workbook["version"];
                    var versions = sheetVersion.Convert<TableVersion>();
                    var version = versions.LastOrDefault();
                    if (version == null)
                    {
                        var msg = $"No version info. is exist";
                        LOG.Warning(msg, this);
                        throw new NoVersionInfoException(msg);
                    }
                    LOG.Addressable($"VERSION : {version}", this);

                    // 코스 찾기
                    var courseSheet = workbook["course_info"];
                    var courseLists = courseSheet.Convert<CourseList>();
                    if (courseLists == null)
                    {
                        var msg = $"No course list is exist";
                        LOG.Warning(msg, this);
                        throw new NoListException(msg);
                    }
                    LOG.Addressable($"Course LIST : {courseLists}", this);

                    // 스테이지 찾기
                    var stageSheet = workbook["stage_info"];
                    var stageLists = stageSheet.Convert<StageList>();
                    if (stageLists == null)
                    {
                        var msg = $"No stage list is exist";
                        LOG.Warning(msg, this);
                        throw new NoListException(msg);
                    }
                    LOG.Addressable($"Stage LIST : {stageLists}", this);

                    Table = new CourseTableLoaderResult
                    {
                        Version = version,
                        CourseList = courseLists.ToArray(),
                        StageList = stageLists.ToArray(),
                    };
                }

                return Table;
            }
        }

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AssetReferenceT<XlsxAsset> tableAR = null;
    }

    // Table
    public class CourseTableLoaderResult
    {
        public TableVersion Version;
        public CourseList[] CourseList;
        public StageList[] StageList;
    }

    [Serializable, Table(1, SafeMode = true)]
    public class CourseList
    {
        [Column("A")] public int Course;
        [Column("B")] public string TitleKor;
        [Column("C")] public string TitleEng;
        [Column("D")] public string TitleVie;
        [Column("E")] public string InfoKor;
        [Column("F")] public string InfoEng;
        [Column("G")] public string InfoVie;
        [Column("H")] public string Keywords;

        public override string ToString()
        {
            return $"{Course} | {TitleKor} | {TitleEng} | {TitleVie} | {InfoKor} | {InfoEng} | {InfoVie} | {Keywords}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class StageList
    {
        [Column("A")] public int Course;
        [Column("B")] public int Stage;
        [Column("C")] public int DayCount;
        [Column("D")] public string InfoKor;
        [Column("E")] public string InfoEng;
        [Column("F")] public string InfoVie;
        [Column("G")] public string Keywords;

        public override string ToString()
        {
            return $"{Course} | {Stage} | {DayCount} | {InfoKor} | {InfoEng} | {InfoVie} | {Keywords}";
        }
    }
}