using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A08;

namespace DoDoEng.Activity.C1_A08
{
    public class C1_A08_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 3, "curriculum.ProblemCount must be 3", this);

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
                    Text = p.Text,
                    AnswerSprite = await loadSprite(p.Image),
                    ExamSprite = await loadExamSprites(tables, p),
                    WordCLIP = await loadSound(p.SoundWord),
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count * Player.MAX_COUNT;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<Sprite[]> loadExamSprites(ActivityData[] tables, ActivityData problem)
        {
            var pool = tables
                .Where(t => t.Index != problem.Index)
                .Where(t => problem.Exams.Contain(t.Alphabet));

            var list = new List<Sprite>();
            foreach (var t in pool)
                list.Add(await loadSprite(t.Image));

            return list.ToArray();
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public Sprite AnswerSprite;
        public Sprite[] ExamSprite;
        public AudioClip WordCLIP;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Text}]" +
                $"</color>";
        }
    }
}