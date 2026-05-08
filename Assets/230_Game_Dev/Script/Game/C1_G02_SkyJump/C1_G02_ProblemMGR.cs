using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameData = DoDoEng.Common.GameData_C1_G02;

namespace DoDoEng.Game.C1_G02
{
    public class C1_G02_ProblemMGR : GameProblemMGR<GameData, ProblemData>
    {
        // Properties
        public RoundConfig[] RoundConfigs => config.RoundConfigs;
        public int TotalProblemCount => Problems.Length;



        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            config = GameIDX.GameMode == GameMode.Playground ? playGroundConfig : reviewConfig;

            var problemCount = config.RoundConfigs.Sum(p => p.ProblemCount);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, problemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var p in problems)
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Text,
                    SoundCLIP = await loadSound(p.SoundPhonetic),
                    Examples = buildExamples(tables, p)
                });
            }

            return problemList.ToArray();
        }



        // Fields
        private ConfigSO config = null;

        // Functions
        private ExampleData[] buildExamples(GameData[] tables, GameData problem)
        {
            var examList = new List<ExampleData>();

            // 오답 보기
            var wrongPool = tables
                                .Where(t => t.Index != problem.Index
                                            && !problem.AvoidTexts.Contain(t.Text))
                                .ToArray();
            var wrongs = UtilArray.Extract(wrongPool, 2);
            foreach (var w in wrongs)
                examList.Add(new ExampleData { IsAnswer = false, Text = w.Text });

            // 정답 보기
            examList.Add(new ExampleData { IsAnswer = true, Text = problem.Text, });

            var exams = examList.ToArray();
            return UtilArray.Shuffle(exams);
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO playGroundConfig = null;
        [SerializeField] private ConfigSO reviewConfig = null;
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public AudioClip SoundCLIP;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Text} | {string.Join(",", Examples.Select(ex => ex.Text))}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Text;

        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";
            return $"<color=red>ExampleData[{Text} {ox}]</color>";
        }
    }
}