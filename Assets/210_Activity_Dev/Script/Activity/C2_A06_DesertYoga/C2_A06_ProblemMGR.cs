using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A06;

namespace DoDoEng.Activity.C2_A06
{
    public class C2_A06_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();

            // 문제 음가 선택
            var phonetics = problemsPool.Select(p => p.Phonics).Distinct().ToArray();
            var problemPhonetics = UtilArray.Extract(phonetics, curriculum.ProblemCount);
            var yogaposes = UtilArray.Sequential(1, yogaPoseCount);
            var problemYogaPoses = UtilArray.Extract(yogaposes, curriculum.ProblemCount);

            // 음가별 보기풀을 미리 생성
            var activityDataDic = new Dictionary<string, Queue<ActivityData>>();
            foreach (var phonetic in phonetics)
            {
                var pool = tables.Where(t => t.Phonics == phonetic).ToArray();
                var extracted = UtilArray.Extract(pool, problemPhonetics.Count(p => p == phonetic) * 2);
                activityDataDic[phonetic] = new Queue<ActivityData>(extracted);
            }

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problemPhonetics.Select((v, i) => (v, i)))
            {
                var exampleQueue = activityDataDic[p];
                var exam1 = exampleQueue.Dequeue();
                var exam2 = exampleQueue.Dequeue();
                var examples = await buildExamples(exam1, exam2);

                problemList.Add(new ProblemData
                {
                    Text = p,
                    YogaPose = problemYogaPoses[i],
                    Examples = examples,
                    CharacterExamples = UtilArray.Shuffled(examples)
                });
            }

            // 같은 Text의 보기가 같은 경우, 보기 순서를 변경
            for (var i = 1; i < problemList.Count; i++)
            {
                var problem = problemList[i];
                var duplicatedProblem = problemList
                                            .Take(i)
                                            .Where(p => p.Text == problem.Text)
                                            .FirstOrDefault();

                if (duplicatedProblem != null)
                {
                    var p1Words = problem.Examples.Select(e => e.Word);
                    var p2Words = duplicatedProblem.Examples.Select(e => e.Word);
                    var equalExamples = p1Words.SequenceEqual(p2Words);

                    if (equalExamples)
                    {
                        var first = duplicatedProblem.Examples.Take(1);
                        var from2 = duplicatedProblem.Examples.Skip(1);
                        duplicatedProblem.Examples = from2.Concat(first).ToArray();
                    }
                }
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count * 2;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData exam1, ActivityData exam2)
        {
            // 보기 추가
            var examActs = new ActivityData[] { exam1, exam2 };

            var examList = new List<ExampleData>();
            foreach (var ex in examActs)
            {
                examList.Add(new ExampleData
                {
                    Text = ex.Phonics,
                    Word = ex.Word,
                    Image = await loadSprite(ex.ImageWord),
                    WordCLIP = await loadSound(ex.SoundWord)
                });
            }

            return examList.ToArray();
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private int yogaPoseCount = 2;
    }

    public class ProblemData
    {
        public string Text;
        public int YogaPose;
        public ExampleData[] Examples;
        public ExampleData[] CharacterExamples;

        public override string ToString()
        {
            var exams = string.Join(",", Examples.Select(ex => ex.Word));
            return $"<color=red>ProblemData </color>" +
                $"[{Text} | {exams}]";
        }
    }
    public class ExampleData
    {
        public string Text;
        public string Word;
        public Sprite Image;
        public AudioClip WordCLIP;
    }
}