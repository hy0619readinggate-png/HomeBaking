using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using FlexFramework.Excel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameData = DoDoEng.Common.GameData_C1_G03;

namespace DoDoEng.Game.C1_G03
{
    public class C1_G03_ProblemMGR : GameProblemMGR<GameData, RoundData>
    {
        // Definition
        private const int MAP_VARIATION = 3;
        private const int GEM_VARIATION = 3;

        // Propertes
        public int[] ProblemCounts => Problems.Select(p => p.ProblemCount).ToArray();
        public int TotalProblemCount => ProblemCounts.Sum();



        // Definitions
        private const int Count_Gems = 8;
        private const int Count_MapVariations = 4;
        private const int Count_Sections = 4;

        // Overrides
        protected override async UniTask<RoundData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                config = GameIDX.GameMode == GameMode.Playground ? playGroundConfig : reviewConfig;

                // 맵 생성
                var roundCount = config.RoundConfigs.Length;
                var maps = generateMaps(roundCount);
                var entrance = UtilArray.Random(1, Map.COLS, roundCount);
                var mapTypes = UtilArray.Random(1, MAP_VARIATION, roundCount);
                var gemTypes = UtilArray.Random(1, GEM_VARIATION, roundCount);

                // 라운드 데이터 생성
                var round = 1;
                var rounds = new List<RoundData>();
                foreach (var (config, i) in config.RoundConfigs.Select((config, i) => (config, i)))
                {
                    var problemCount = config.ProblemCount;
                    var tableGems = UtilArray.Extract(tables, Count_Gems);

                    // 보석 데이터 생성
                    var gems = new List<GemData>();
                    foreach (var data in tableGems)
                    {
                        gems.Add(new GemData
                        {
                            Word = data.ImageWord,
                            WordSprite = await loadSprite(data.ImageWord),
                            Variation = gemTypes[i],
                        });
                    }

                    // 보석 재배치 (섹션 위치 반영)
                    var startSection = entrance[i] <= Map.COLS / 2 ? 0 : 1;
                    var gemPositions = randomGemPositionForSections(startSection);
                    var gemsReordered = reorderGems(gems.ToArray(), gemPositions);

                    DEV_SetGemsOrder(gemsReordered, gemPositions, problemCount);

                    LOG.VeryImportant($"POSITIONS : {string.Join(",", gemPositions)}", this);
                    LOG.VeryImportant($"GEMS : {string.Join(",", gems.Select(g => g.Word))}", this);
                    LOG.VeryImportant($"GEMS REORDERED : {string.Join(",", gemsReordered.Select(g => g.Word))}", this);

                    // 문제 데이터 생성
                    var problemExtracted = tableGems.Take(problemCount).ToArray();
                    var problems = new List<ProblemData>();
                    foreach (var data in problemExtracted)
                    {
                        problems.Add(new ProblemData
                        {
                            Word = data.ImageWord,
                            WordCLIP = await loadSound(data.SoundWord)
                        });
                    }

                    // 라운드 생성
                    rounds.Add(new RoundData
                    {
                        Round = round,
                        Gems = gemsReordered,
                        Problems = problems.ToArray(),
                        Maps = maps[i],

                        Enterance = entrance[i],
                        Duration = config.Duration,
                        JackSpeed = config.JackSpeed,
                        MapVariation = mapTypes[i],
                        GemVariation = gemTypes[i]
                    });
                    round++;
                }

                return rounds.ToArray();
            }
        }



        // Fields
        private ConfigSO config = null;

        // Functions
        private static int[] randomSections(int startSection)
        {
            // 시작은 하단의 섹션(2,3)에서 시작
            // 대각선을 제외한 상하좌우에 인접한 섹션을 다음 정답 섹션으로 배정
            //  0  1
            //  2  3
            var list = new List<int>();
            var sections = new List<int> { 0, 0, 1, 1, 2, 2, 3, 3 };

            var first = UtilArray.ExtractOne(nextSections(startSection));
            sections.Remove(first);
            list.Add(first);

            var current = first;
            while (sections.Count > 0)
            {
                var available = nextSections(current);

                var next = UtilArray.ExtractOne(available, out available);
                if (!sections.Exists(s => s == next))
                    next = available[0];

                sections.Remove(next);
                list.Add(next);
                current = next;
            }

            return list.ToArray();
        }
        private static int[] nextSections(int current)
        {
            return current switch
            {
                0 => new int[] { 1, 2 },
                1 => new int[] { 0, 3 },
                2 => new int[] { 0, 3 },
                3 => new int[] { 1, 2 },
                _ => null
            };
        }
        private static int[] randomGemPositionForSections(int startSection)
        {
            // 섹션의 위치을 반영해 보석의 위치를 최대한 멀리 배치
            // 섹션은 다음과 같이 있다고 가정 / 각 섹션별로 2개의 보석을 배치
            //  0  1
            //  2  3
            var positions = new int[][] {
                new int[] { 0, 1 },
                new int[] { 2, 3 },
                new int[] { 4, 5 },
                new int[] { 6, 7 }
            };
            var list = new List<int>();

            var sections = randomSections(startSection);
            foreach (var s in sections)
            {
                var position = UtilArray.ExtractOne(positions[s], out positions[s]);
                list.Add(position);
            }

            return list.ToArray();
        }
        private static T[] reorderGems<T>(T[] arr, int[] toIndices)
        {
            if (arr == null) return null;
            if (arr.Length < 2) return arr;

            var reordered = new T[arr.Length];
            for (var i = 0; i < toIndices.Length; i++)
            {
                var idx = toIndices[i];
                reordered[idx] = arr[i];
            }

            return reordered;
        }
        private static int[][] generateMaps(int roundCount)
        {
            var seq = UtilArray.Random(1, Count_MapVariations, Count_Sections);
            var maps = new int[Count_Sections][];

            for (var i = 0; i < maps.Length; i++)
            {
                maps[i] = seq.Skip(i)
                             .Concat(seq.Take(i))
                             .ToArray();  // Shift(i)
            }

            return UtilArray.Extract(maps, roundCount);
        }

        // Functions : dev
        private void DEV_SetGemsOrder(GemData[] arr, int[] toIndices, int problemCount)
        {
            for (var i = 0; i < arr.Length; i++)
            {
                if (i < problemCount)
                    arr[toIndices[i]].DEV_Order = (i + 1).ToString();
                else
                    arr[toIndices[i]].DEV_Order = "X";
            }
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO playGroundConfig = null;
        [SerializeField] private ConfigSO reviewConfig = null;
    }

    public class RoundData
    {
        public int Round;
        public GemData[] Gems;
        public ProblemData[] Problems;
        public int[] Maps;

        // Properties
        public int Enterance;
        public float Duration;
        public float JackSpeed;
        public int MapVariation;
        public int GemVariation;

        // Properties : computed
        public int ProblemCount => Problems.Length;

        public override string ToString()
        {
            return $"<color=red>"
                + $"RoundData [{Round} "
                + $"| {string.Join(",", Problems.Select(p => p.Word))} "
                + $"| {string.Join(",", Gems.Select(g => g.Word))} "
                + $"| {string.Join(",", Maps.Select(m => m.ToString()))} "
                + $"</color>";
        }
    }
    public class ProblemData
    {
        public string Word;
        public AudioClip WordCLIP;

        public override string ToString()
        {
            return $"<color=red>ProblemData [{Word}]</color>";
        }
    }
    public class GemData
    {
        public string Word;
        public Sprite WordSprite;
        public int Variation;

        public string DEV_Order;

        public override string ToString()
        {
            return $"<color=red>GemData [{Word}]</color>";
        }
    }
}