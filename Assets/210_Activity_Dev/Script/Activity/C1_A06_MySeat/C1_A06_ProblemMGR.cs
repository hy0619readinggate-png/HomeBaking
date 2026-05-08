using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A06;

namespace DoDoEng.Activity.C1_A06
{
    public class C1_A06_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount + curriculum.ExtraProblemCount == 6,
                        "curriculum.ProblemCount + curriculum.ExtraProblemCount must be 6", this);

            // 문제 데이터 추출 (1~3번 문제)
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Text,
                    TextCLIP = await loadSound(p.SoundText),
                    PhoneticCLIP = await loadSound(p.SoundPhonetic),
                    Examples = await buildExamples(tables, p, (problems.Length - i - 1) % 3)
                });
            }

            // 문제 데이터 추출 (4~6번 문제)
            if (curriculum.ExtraProblemCount > 0)
            {
                // 문제 데이터 추출 (4~6번 문제) - 2문제 이상 이전 차시에서 출제
                var prevProblemsPool = tables.Filter(curriculum.ExtraDataMin, curriculum.ExtraDataMax).ToArray();
                var prevProblemCount = UtilRandom.RandomSuccess(0.5f) ? 2 : curriculum.ExtraProblemCount;
                var prevProblems = UtilArray.Extract(prevProblemsPool, prevProblemCount);

                // 문제 데이터 추출 (4~6번 문제) - 나머지 문제
                var otherProblemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax)
                                        .Concat(tables.Filter(curriculum.ExtraDataMin, curriculum.ExtraDataMax))
                                        .Where(p => !prevProblems.Contains(p)).ToArray();
                var otherProblemCount = curriculum.ExtraProblemCount - prevProblems.Length;
                var otherProblems = UtilArray.Extract(otherProblemsPool, otherProblemCount);

                // 섞어서 4~6번문제로 셋팅
                var extraProblems = prevProblems.Concat(otherProblems).ToArray();
                var shuffledExtraProblems = UtilArray.Shuffle(extraProblems);

                // 문제 데이터 생성
                foreach (var (p, i) in shuffledExtraProblems.Select((v, i) => (v, i)))
                {
                    problemList.Add(new ProblemData
                    {
                        Index = p.Index,
                        Text = p.Text,
                        TextCLIP = await loadSound(p.SoundText),
                        PhoneticCLIP = await loadSound(p.SoundPhonetic),
                        Examples = await buildExamples(tables, p, (problems.Length - i - 1) % 3)
                    });
                }
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem, int exampleCount)
        {
            var exams = new List<ActivityData>();

            // 정답 추가
            exams.Add(problem);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Index != problem.Index).ToArray();
            var wrong = UtilArray.Extract(wrongPool, exampleCount);
            exams.AddRange(wrong);

            // 보기 만들기
            var examList = new List<ExampleData>();
            foreach (var ex in exams)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Text = ex.Text,
                    PhoneticCLIP = await loadSound(ex.SoundPhonetic)
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
        public string Text;
        public AudioClip PhoneticCLIP;
        public AudioClip TextCLIP;
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
        public AudioClip PhoneticCLIP;

        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";
            return $"<color=red>ExampleData[{Text} {ox}]</color>";
        }
    }
}