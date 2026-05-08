using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C3_A04;

namespace DoDoEng.Activity.C3_A04
{
    public class C3_A04_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int PLANET_COUNT = 15;
        private const int PLANET_SHAPE_COUNT = 10;
        private const string ALPHABETS = "abcdefghijklmnopqrstuvwxyz";



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
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                var phrases = p.BlankWord.Split('|');
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Sentence = p.Sentence,
                    Phrases = phrases,
                    WordCLIP = await loadSound(p.SoundWord),
                    SentenceCLIP = await loadSound(p.SoundSentence),
                    SentenceSPR = await loadSprite(p.ImageSentence),
                    Examples = buildExamples(tables, phrases[1])
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Select(p => p.Answer.Length).Sum();

            return problemList.ToArray();
        }

        // Functions
        private ExampleData[] buildExamples(ActivityData[] tables, string problem)
        {
            var shapesIDs = UtilArray.Random(1, PLANET_SHAPE_COUNT, PLANET_COUNT);

            var list = new List<ExampleData>();

            // 문제 보기 추가
            foreach (var (p, i) in problem.Select((p, i) => (p, i)))
            {
                list.Add(new ExampleData
                {
                    Shape = shapesIDs[i],
                    Text = p.ToString()
                });
            }

            // 오답추가
            var wrongPool = ALPHABETS.Where(a => !problem.ToLower().Contains(a.ToString().ToLower())).ToArray();
            var wrong = UtilArray.Extract(wrongPool, PLANET_COUNT - problem.Length);
            foreach (var (w, i) in wrong.Select((w, i) => (w, i)))
            {
                list.Add(new ExampleData
                {
                    Shape = shapesIDs[i + problem.Length],
                    Text = w.ToString()
                });
            }

            var exams = list.ToArray();
            return UtilArray.Shuffle(exams);
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Sentence;
        public string[] Phrases;
        public AudioClip SentenceCLIP;
        public AudioClip WordCLIP;
        public Sprite SentenceSPR;
        public ExampleData[] Examples;

        public string Answer => Phrases[1];

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Sentence} | {Answer} | {string.Join(",", Examples.Select(ex => ex.Text))} ]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public int Shape;
        public string Text;

        public override string ToString()
        {
            return $"<color=red>ExampleData[{Text} | {Shape}</color>";
        }
    }
}