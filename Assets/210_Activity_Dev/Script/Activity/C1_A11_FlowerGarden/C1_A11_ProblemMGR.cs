using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A11;

namespace DoDoEng.Activity.C1_A11
{
    public class C1_A11_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 2, "curriculum.ProblemCount must be 2", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Text1,
                    PhoneticCLIP = await loadSound(p.SoundPhonetic),
                    Examples = buildExamples(tables, p, config.ProblemConfigs[i])
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = config.ProblemConfigs
                                    .Take(problemList.Count())
                                    .Select(c => c.AnswerFlowerCount)
                                    .Sum();

            return problemList.ToArray();
        }

        // Functions
        private ExampleData[] buildExamples(ActivityData[] tables, ActivityData problem, ProblemConfig config)
        {
            var exams = new List<ActivityData>();

            // 정답 추가
            var correct = Enumerable.Repeat(problem, config.AnswerFlowerCount);
            exams.AddRange(correct);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Text1 != problem.Text1).ToArray();
            var wrong = UtilArray.Extract(wrongPool, config.WrongFlowerCount);
            exams.AddRange(wrong);

            // 보기 만들기
            var examList = new List<ExampleData>();
            foreach (var ex in exams)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Text = ex.Text1,
                });
            }
            var examData = examList.ToArray();
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
        public AudioClip PhoneticCLIP;
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
    }
}