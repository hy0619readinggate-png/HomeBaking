using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C3_A05;

namespace DoDoEng.Activity.C3_A05
{
    public class C3_A05_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 4, "curriculum.ProblemCount must be 4", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            var ingredientIDs = UtilArray.Random(1, curriculum.ProblemCount);
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Sentence = p.Sentence,
                    IngredientID = ingredientIDs[i],
                    SentenceCLIP = await loadSound(p.SoundSentence),
                    SentenceSPR = await loadSprite(p.ImageSentence),
                    Examples = buildExamples(tables, p, ingredientIDs[i])
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }

        // Functions
        private ExampleData[] buildExamples(ActivityData[] tables, ActivityData problem, int ingredientID)
        {
            var list = new List<ActivityData>();

            // 정답 추가
            list.Add(problem);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Index != problem.Index).ToArray();
            var wrong = UtilArray.ExtractOne(wrongPool);
            list.Add(wrong);

            var examList = new List<ExampleData>();
            foreach (var ex in list)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Sentence = ex.Sentence,
                    IngredientID = ingredientID
                });
            }

            var examData = examList.ToArray();
            UtilArray.Shuffle(examData);

            return examData;
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Sentence;
        public AudioClip SentenceCLIP;
        public Sprite SentenceSPR;
        public int IngredientID;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Sentence} | {string.Join(",", Examples.Select(ex => ex.Sentence))} ]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Sentence;
        public int IngredientID;

        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";
            return $"<color=red>ExampleData[{Sentence} {ox}]</color>";
        }
    }
}