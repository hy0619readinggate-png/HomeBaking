using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A05;

namespace DoDoEng.Activity.C1_A05
{
    public class C1_A05_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int COUNT_IN_SET = 2;

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount + curriculum.ExtraProblemCount == 4,
                        "curriculum.ProblemCount + curriculum.ExtraProblemCount must be 4", this);

            // 문제 데이터 추출 (1~2번 문제)
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var p in problems)
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Sound,
                    SoundCLIP = await loadSound(p.Sound),
                    Examples = await buildExamples(tables, p),
                    IsFirstInSet = problemList.Count % COUNT_IN_SET == 0,
                    IsLastInSet = problemList.Count % COUNT_IN_SET == 1
                });
            }

            // 문제 데이터 추출 (3~4번 문제)
            var extraProblemsPool = tables.Filter(curriculum.ExtraDataMin, curriculum.ExtraDataMax).ToArray();
            var extraProblems = UtilArray.Extract(extraProblemsPool, curriculum.ExtraProblemCount);

            // 문제 데이터 생성
            foreach (var p in extraProblems)
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Sound,
                    SoundCLIP = await loadSound(p.Sound),
                    Examples = await buildExamples(tables, p),
                    IsFirstInSet = problemList.Count % COUNT_IN_SET == 0,
                    IsLastInSet = problemList.Count % COUNT_IN_SET == 1
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count * 2;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            // 오답 보기
            var wrongPool = tables.Where(t => t.Index != problem.Index).ToArray();
            var wrong = UtilArray.Extract(wrongPool, 2);

            var examList = new List<ExampleData>();
            foreach (var w in wrong)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = false,
                    Text = w.Text1,
                    SoundCLIP = await loadSound(w.SoundText1)
                });

                examList.Add(new ExampleData
                {
                    IsAnswer = false,
                    Text = w.Text2,
                    SoundCLIP = await loadSound(w.SoundText2)
                });
            }
            examList.RemoveAt(Random.Range(0, examList.Count));

            // 정답 보기
            examList.Add(new ExampleData
            {
                IsAnswer = true,
                Text = problem.Text1,
                SoundCLIP = await loadSound(problem.SoundText1)
            });
            examList.Add(new ExampleData
            {
                IsAnswer = true,
                Text = problem.Text2,
                SoundCLIP = await loadSound(problem.SoundText2)
            });

            var exams = examList.ToArray();
            return UtilArray.Shuffle(exams);
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public AudioClip SoundCLIP;
        public ExampleData[] Examples;
        public bool IsFirstInSet;
        public bool IsLastInSet;

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