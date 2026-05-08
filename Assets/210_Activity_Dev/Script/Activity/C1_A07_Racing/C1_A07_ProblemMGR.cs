using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A07;

namespace DoDoEng.Activity.C1_A07
{
    public class C1_A07_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 6, "curriculum.ProblemCount must be 6", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var p in problems)
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Subject = p.Text,
                    SubjectCLIP = await loadSound(p.SoundText),
                    WordClip = await loadSound(p.SoundWord),

                    Examples = await buildExamples(tables, p)
                });
            }

            await UniTask.Yield();

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count();

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData[] tables, ActivityData problem)
        {
            var exams = new List<ActivityData>();

            // 정답 추가
            exams.Add(problem);

            // 오답 추가
            var wrongPool = tables.Where(t => t.Text != problem.Text).ToArray();
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
                    Image = await loadSprite(ex.ImageWord),
                    WordCLIP = await loadSound(ex.SoundWord),
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
        public string Subject;
        public AudioClip SubjectCLIP;
        public AudioClip WordClip;
        public ExampleData[] Examples;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Subject} | {string.Join(",", Examples.Select(ex => ex.Word))}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Word;
        public Sprite Image;
        public AudioClip WordCLIP;
    }
}