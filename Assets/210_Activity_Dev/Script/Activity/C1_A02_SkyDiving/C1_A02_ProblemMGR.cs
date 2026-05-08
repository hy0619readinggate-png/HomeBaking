using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A02;

namespace DoDoEng.Activity.C1_A02
{
    public class C1_A02_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 3, "curriculum.ProblemCount must be 3", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);
            var characterIDs = UtilArray.Random(1, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                var isUpper = checkUppercase(i);

                // 보기 생성
                var examData = extractExamples(tables, p);
                var exams = await examData.Select(async ex => new ExampleData
                {
                    IsAnswer = ex.Index == p.Index,
                    Text = !isUpper
                        ? ex.Text1
                        : ex.Text2,
                    SoundCLIP = !isUpper
                        ? await loadSound(ex.SoundText1)
                        : await loadSound(ex.SoundText2)
                }).ToArray();
                UtilArray.Shuffle(exams);

                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = isUpper
                        ? p.Text1
                        : p.Text2,
                    TextAnswer = p.Text3,
                    SoundCLIP = await loadSound(p.SoundText3),
                    // #897 [C1A2] 문제 음원 변경 요청 by veramocor
                    //SoundCLIP = isUpper
                    //    ? await loadSound(p.SoundText1)
                    //    : await loadSound(p.SoundText2),
                    SoundAnswerCLIP = await loadSound(p.SoundText3),
                    Examples = exams,
                    CharacterID = characterIDs[i]
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }

        // Functions
        private bool checkUppercase(int idx)
        {
            if (idx == 0) return true;
            if (idx == 1) return false;
            return UtilRandom.RandomSuccess(0.5f);
        }
        private ActivityData[] extractExamples(ActivityData[] tables, ActivityData problem)
        {
            var list = new List<ActivityData>();

            // 정답 추가
            list.Add(problem);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Index != problem.Index).ToArray();
            var wrong = UtilArray.Extract(wrongPool, 2);
            list.AddRange(wrong);

            return list.ToArray();
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public string TextAnswer;
        public AudioClip SoundCLIP;
        public AudioClip SoundAnswerCLIP;
        public ExampleData[] Examples;
        public int CharacterID;

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
        public AudioClip SoundCLIP;

        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";
            return $"<color=red>ExampleData[{Text} {ox}]</color>";
        }
    }
}