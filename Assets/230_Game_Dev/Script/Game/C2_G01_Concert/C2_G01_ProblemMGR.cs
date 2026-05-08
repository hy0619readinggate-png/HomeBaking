using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

using GameData = DoDoEng.Common.GameData_C2_G01;

namespace DoDoEng.Game.C2_G01
{
    public class C2_G01_ProblemMGR : GameProblemMGR<GameData, RoundData>
    {
        // Propertes
        public int[] ProblemCounts => Problems.Select(p => p.Problems.Length).ToArray();
        public int TotalProblemCount => ProblemCounts.Sum();

        // Overrides
        protected override async UniTask<RoundData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                // 문제 데이터 생성
                var round = 1;
                var rounds = new List<RoundData>();
                var problemCount = config.ProblemCount;

                foreach (var config in config.RoundConfigs)
                {
                    var tableExtracted = UtilArray.Extract(tables, problemCount);
                    var problems = new List<ProblemData>();
                    foreach (var td in tableExtracted)
                    {
                        var pd = new ProblemData
                        {
                            Index = td.Index,
                            Word = td.Word,
                            Phrases = buildPhrases(td),
                            WordCLIP = await loadSound(td.SoundWord),
                            PhonicsCLIP = await loadSound(td.SoundPhonetic),
                            Examples = await buildExamples(tables, td)
                        };
                        problems.Add(pd);
                    }

                    rounds.Add(new RoundData
                    {
                        Round = round++,
                        Problems = problems.ToArray(),
                        Music = UtilArray.ExtractOne(config.Musics),
                    });
                }

                foreach (var r in rounds)
                {
                    LOG.Info($"Round {r.Round}", this);
                    r.Problems.ForEach(p => LOG.Info($"{p}", this));
                }

                return rounds.ToArray();
            }
        }



        // Functions
        private string[] buildPhrases(GameData problem)
        {
            var blankWord = Regex.Replace(problem.BlankWord, "_+", "_");
            var phrases = blankWord.Split("_");
            return new string[] { phrases[0], problem.Phonics, phrases[1] };
        }
        private async UniTask<ExampleData[]> buildExamples(GameData[] tables, GameData problem)
        {
            var exams = new List<GameData>();

            // 정답 추가
            exams.Add(problem);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Phonics != problem.Phonics).ToArray();
            var wrong = UtilArray.Extract(wrongPool, 2);
            exams.AddRange(wrong);

            // 보기 만들기
            var examList = new List<ExampleData>();
            foreach (var ex in exams)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Phonics = ex.Phonics,
                    PhonicsCLIP = await loadSound(ex.SoundPhonetic)
                });
            }
            var examData = examList.ToArray();
            UtilArray.Shuffle(examData);

            return examData;
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO config = null;
    }

    public class RoundData
    {
        public int Round;
        public ProblemData[] Problems;
        public Music Music;
    }
    public class ProblemData
    {
        public int Index;
        public string Word;
        public string[] Phrases;  // 항상 3개, 가운데가 정답
        public AudioClip WordCLIP;
        public AudioClip PhonicsCLIP;
        public ExampleData[] Examples;

        public string Answer => Phrases[1];

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Word} " +
                $"| {Phrases[0]}({Phrases[1]}){Phrases[2]} " +
                $"| {string.Join(",", Examples.Select(ex => ex.Phonics))}" +
                $"]</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Phonics;
        public AudioClip PhonicsCLIP;

        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";
            return $"<color=red>ExampleData[{Phonics} {ox}]</color>";
        }
    }
}