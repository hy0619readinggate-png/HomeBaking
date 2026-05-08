using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A11;

namespace DoDoEng.Activity.C2_A11
{
    public class C2_A11_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int WRONG_EXAMPLE_COUNT = 2;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 3, "curriculum.ProblemCount must be 3", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();

            // 음가별 문제 수 생성
            var phonetics = problemsPool.Select(p => p.Phonics).Distinct().ToArray();

            var shuffledPhonetics = UtilArray.Shuffled(phonetics);
            var copyProblemPool = problemsPool.Clone() as ActivityData[];

            // 음가별 문제 생성
            var problemList = new List<ProblemData>();
            for (var i = 0; i < phonetics.Length; i++)
            {
                var phoneticProblems = copyProblemPool.Where(p => p.Phonics == phonetics[i]).ToArray();
                var p = UtilArray.ExtractWithRemain(copyProblemPool, 1, out var remainProblemPool)[0];
                copyProblemPool = remainProblemPool;

                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Word = p.Word,
                    BlankWord = p.BlankWord,
                    WordSPR = await loadSprite(p.ImageWord),
                    WordCLIP = await loadSound(p.SoundWord),
                    PhonicsCLIP = await loadSound(p.SoundPhonetic),
                    Examples = buildExamples(tables, p)
                });
            }

            if (phonetics.Length == 2)
            {
                var phonetic = UtilArray.ExtractOne(phonetics);

                var phoneticProblems = copyProblemPool.Where(p => p.Phonics == phonetic).ToArray();
                var p = UtilArray.ExtractWithRemain(copyProblemPool, 1, out var remainProblemPool)[0];
                copyProblemPool = remainProblemPool;

                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Word = p.Word,
                    BlankWord = p.BlankWord,
                    WordSPR = await loadSprite(p.ImageWord),
                    WordCLIP = await loadSound(p.SoundWord),
                    PhonicsCLIP = await loadSound(p.SoundPhonetic),
                    Examples = buildExamples(tables, p)
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }

        // Functions
        private ExampleData[] buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ExampleData>();

            // 정답 추가
            list.Add(new ExampleData
            {
                IsAnswer = true,
                Text = problem.Phonics
            });

            // 오답 생성
            var phonetics = tables.Select(p => p.Phonics).Distinct().ToArray();
            var wrongPool = phonetics.Where(t => t != problem.Phonics).ToArray();
            var wrong = UtilArray.Extract(wrongPool, WRONG_EXAMPLE_COUNT);

            // 오답추가
            var wrongList = wrong.Select(w => new ExampleData
            {
                IsAnswer = false,
                Text = w
            });
            list.AddRange(wrongList);

            var examData = list.ToArray();
            UtilArray.Shuffle(examData);

            return examData;
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Word;
        public string BlankWord;
        public Sprite WordSPR;
        public AudioClip WordCLIP;
        public AudioClip PhonicsCLIP;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Word} | {BlankWord} | {string.Join(",", Examples.Select(ex => ex.Text))}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Text;
    }
}