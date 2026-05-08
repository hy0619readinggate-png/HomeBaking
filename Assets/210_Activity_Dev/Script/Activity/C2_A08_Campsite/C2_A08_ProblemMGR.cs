using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A08;

namespace DoDoEng.Activity.C2_A08
{
    public class C2_A08_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();

            // 음가 추출 (음가 개수만큼 세트 반복)
            var phonetics = problemsPool.Select(p => p.Phonics).Distinct().ToArray();

            // 음가별 문제 수  (음가2 = 세트당 3문제, 음가3 = 세트당 2문제)
            var problemCountPerSet = phonetics.Length == 2 ? 3 : 2;
            UtilArray.Shuffle(phonetics);

            // 세트별로 문제 출제
            var problemList = new List<ProblemData>();
            for (var s = 0; s < phonetics.Length; s++)
            {
                var ph = phonetics[s];
                var problemsPH = problemsPool.Where(p => p.Phonics == ph).ToArray();
                var problems = UtilArray.Extract(problemsPH, problemCountPerSet);

                foreach (var (p, i) in problems.Select((v, i) => (v, i)))
                {
                    problemList.Add(new ProblemData
                    {
                        Index = p.Index,
                        Set = s + 1,
                        Text = p.Phonics,
                        LastBottle = problems.Last() == p,
                        PhoneticCLIP = await loadSound(p.SoundPhonetic),
                        WordSPR = await loadSprite(p.ImageWord),
                        Examples = await buildExamples(tables, p)
                    });
                }
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            // 정답 추가
            var exams = new List<ActivityData> { problem };

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
                    Word = ex.Word,
                    WordSPR = await loadSprite(ex.ImageWord),
                    WordCLIP = await loadSound(ex.SoundWord)
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
        public int Set;
        public string Text;
        public bool LastBottle;
        public AudioClip PhoneticCLIP;
        public Sprite WordSPR;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Set} | {Text} | {LastBottle} | {string.Join(",", Examples.Select(ex => ex.Word))}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Word;
        public Sprite WordSPR;
        public AudioClip WordCLIP;

        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";
            return $"<color=red>ExampleData[{Word} {ox}]</color>";
        }
    }
}