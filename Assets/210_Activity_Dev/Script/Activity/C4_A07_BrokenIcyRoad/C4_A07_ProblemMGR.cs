using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C4_A07;

namespace DoDoEng.Activity.C4_A07
{
    public class C4_A07_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Constants
        private const int WRONG_EXAMPLE_COUNT = 2;

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

                LOG.Info($"{p.Sentence}", this);
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Sentence = p.Sentence,
                    SentenceCLIP = await loadSound(p.SoundSentence),
                    SentenceSPR = await loadSprite(p.ImageSentence),
                    Subjects = buildSubjects(p),
                    Examples = buildExamples(tables, p)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }

        // Functions
        private SubjectData[] buildSubjects(ActivityData problem)
        {
            return problem.Texts.Select((t, i) => new SubjectData
            {
                IsBlank = (i + 1 == problem.BlankIndex),
                Text = t
            }).ToArray();
        }
        private ExampleData[] buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ExampleData>();

            // 정답 보기 추가
            list.Add(new ExampleData
            {
                IsAnswer = true,
                Text = problem.Texts[problem.BlankIndex - 1]
            });

            // 오답 보기 추가
            var wrongPool = tables.Where(t => t.Index != problem.Index).ToArray();
            var wrongList = new List<string>();
            foreach (var p in wrongPool)
            {
                var texts = p.Texts.Where(t => !problem.Texts.Contain(t));
                if (problem.BlankIndex == problem.Texts.Length) // 마지막 청크가 문제인 경우
                    texts = texts.TakeLast(1);

                wrongList.AddRange(texts);
            }
            var wrongs = wrongList.Distinct().ToArray();
            var wrong = UtilArray.Extract(wrongs, WRONG_EXAMPLE_COUNT);

            foreach (var w in wrong)
            {
                list.Add(new ExampleData
                {
                    IsAnswer = false,
                    Text = w
                });
            }

            return list.ToArray();
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Sentence;
        public AudioClip SentenceCLIP;
        public Sprite SentenceSPR;
        public SubjectData[] Subjects;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Sentence} | {string.Join(",", Examples.Select(ex => ex.Text))}]" +
                $"</color>";
        }

        // Properties
        public int SubjectCount => Subjects.Length;
    }
    public class SubjectData
    {
        public bool IsBlank;
        public string Text;
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Text;
    }
}