using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ActivityData = DoDoEng.Common.ActivityData_C4_A08;

namespace DoDoEng.Activity.C3_A09
{
    public class C4_A08_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Constants
        private const int EXAMPLE_COUNT = 6;

        // Properties
        public bool IsIntroDataExist => introDatas.Length > 1;
        public IntroData[] IntroDatas => introDatas;



        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 4, "curriculum.ProblemCount must be 4", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var customerIDs = UtilArray.Random(1, curriculum.ProblemCount);
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    CustomerID = customerIDs[i],
                    Sentence = p.Sentence,
                    SentenceCLIP = await loadSound(p.SoundSentence),
                    Subjects = buildSubjects(p),
                    Examples = buildExamples(tables, p)
                });
            }

            // 인트로 데이터 생성
            var words = problemsPool
                        .Where(p => !string.IsNullOrEmpty(p.Word))
                        .Select(p => p.Word)
                        .Distinct()
                        .ToArray();
            var introList = new List<IntroData>();
            foreach (var word in words)
            {
                var introWords = problemsPool.Where(p => p.Word == word).ToArray();
                introList.Add(new IntroData
                {
                    Word = introWords[0].Word,
                    WordCLIP = await loadSound(introWords[0].SoundWord),
                    WordSPR = await loadSprite(introWords[0].ImageWord),
                    Sentence1 = introWords[0].Sentence,
                    Sentence2 = introWords[1].Sentence,
                    SentenceCLIP1 = await loadSound(introWords[0].SoundSentence),
                    SentenceCLIP2 = await loadSound(introWords[1].SoundSentence)
                });

            }
            introDatas = introList.ToArray();

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList
                            .Sum(p => p.Subjects.Count());

            return problemList.ToArray();
        }

        // Fields
        private IntroData[] introDatas = null;

        // Functions
        private SubjectData[] buildSubjects(ActivityData problem)
        {
            return problem.Texts.Select((t, i) => new SubjectData
            {
                ID = i + 1,
                Text = t
            }).ToArray();
        }
        private ExampleData[] buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ExampleData>();

            // 문제의 보기
            foreach (var (t, i) in problem.Texts.Select((t, i) => (t, i)))
            {
                list.Add(new ExampleData
                {
                    ID = i + 1,
                    Text = t,
                });
            }

            // 오답 추가
            var wrongList = new List<string>();
            var wrongPool = tables.Where(t => t.Index != problem.Index).Select(t => t.Texts);

            foreach (var texts in wrongPool)
            {
                foreach (var text in texts)
                {
                    if (!problem.Texts.Exists(t => t == text))
                        wrongList.Add(text);
                }
            }

            var wrongArr = wrongList.Distinct().ToArray();
            var extractCount = EXAMPLE_COUNT - problem.Texts.Length;
            var wrongTexts = UtilArray.Extract(wrongArr, extractCount);

            foreach (var t in wrongTexts)
            {
                list.Add(new ExampleData
                {
                    ID = -1,
                    Text = t
                });
            }
            var randomIDs = UtilArray.Random(1, EXAMPLE_COUNT);
            foreach (var (exam, i) in list.Select((exam, i) => (exam, i)))
            {
                exam.SkinID = randomIDs[i];
            }
            return UtilArray.Shuffled(list.ToArray());
        }
    }

    public class IntroData
    {
        public string Word;
        public AudioClip WordCLIP;
        public Sprite WordSPR;
        public string Sentence1;
        public string Sentence2;
        public AudioClip SentenceCLIP1;
        public AudioClip SentenceCLIP2;

        public override string ToString()
        {
            return $"<color=red>IntroData" +
                $"[{Word} | {Sentence1} | {Sentence2}]" +
                $"</color>";
        }
    }
}