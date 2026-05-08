using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A09;

namespace DoDoEng.Activity.C1_A09
{
    public class C1_A09_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Properties
        public ConfigSO Config => config;



        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 3, "curriculum.ProblemCount must be 3", this);
            LOG.Assert(config.ProblemConfigs.Length == 3, "config.ProblemConfigs.Length must be 3", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var odd = problemsPool.Where((t, i) => i % 2 == 0).ToArray();
            var even = problemsPool.Where((t, i) => i % 2 != 0).ToArray();
            var p1 = UtilArray.ExtractOne(odd);
            var p2 = UtilArray.ExtractOne(even);
            var p3 = UtilArray.ExtractOne(problemsPool);
            var problems = new ActivityData[] { p1, p2, p3 };

            // 문제 데이터 생성
            var blanksCount = 0;
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Text,
                    WordCLIP = await loadSound(p.SoundWord),
                    Examples = buildExamples(p, p.ExampleTexts, config.ProblemConfigs[i])
                });

                blanksCount += config.ProblemConfigs[i].AnswerPopcornCount;
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = blanksCount;

            return problemList.ToArray();
        }

        // Functions
        private ExampleData[] buildExamples(ActivityData problem, string[] wrongExampleTexts, ProblemConfig config)
        {
            var list = new List<string>();

            // 정답 팝콘 추가
            var correct = Enumerable.Repeat(problem.Text, config.AnswerPopcornCount);
            list.AddRange(correct);

            // 오답 팝콘 추가
            var wrong = UtilArray.Extract(wrongExampleTexts, config.WrongPopcornCount);
            list.AddRange(wrong);

            var examData = list.Select(ex => new ExampleData
            {
                IsAnswer = ex == problem.Text,
                Text = ex
            }).ToArray();
            UtilArray.Shuffle(examData);

            return examData;
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO config = null;
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public AudioClip WordCLIP;
        public ExampleData[] Examples;

        // C3_A07 only
        public bool C3_A07 => AlphbetLocations != null;
        public int[] AlphbetLocations;
        public string[] Alphabets => AlphbetLocations.Select(loc => Text[loc - 1].ToString()).ToArray();
        public int EmptyTextCount => AlphbetLocations.Length;



        // Overrides
        public override string ToString()
        {
            if (AlphbetLocations == null)
            {
                return $"<color=red>ProblemData" +
                    $"[{Index} | {Text} | {string.Join(",", Examples.Select(ex => ex.Text))}]" +
                    $"</color>";
            }
            else
            {
                var textBlank = Text;
                foreach (var loc in AlphbetLocations)
                    textBlank = textBlank.Remove(loc - 1, 1).Insert(loc - 1, "_");

                return $"<color=red>ProblemData" +
                    $"[{Index} | {Text} | {textBlank} | {string.Join(",", Examples.Select(ex => ex.Text))}]" +
                    $"</color>";
            }
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