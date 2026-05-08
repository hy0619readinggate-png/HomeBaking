using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C3_A07;

namespace DoDoEng.Activity.C1_A09
{
    public class C3_A07_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Properties
        public ConfigSO Config => config;



        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 3, "curriculum.ProblemCount must be 3", this);
            LOG.Assert(config.ProblemConfigs.Length == 3, "config.ProblemConfigs.Length must be 3", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            // 문제 데이터 생성
            var blanksCount = 0;
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((v, i) => (v, i)))
            {
                var problemLocations = buildProblemLocations(p.Word);

                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Text = p.Word,
                    AlphbetLocations = problemLocations,
                    WordCLIP = await loadSound(p.SoundWord),
                    Examples = buildExamples(tables, p, problemLocations, config.ProblemConfigs[i])
                });

                blanksCount += problemLocations.Length;
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = blanksCount;

            return problemList.ToArray();
        }

        // Functions
        private int[] buildProblemLocations(string word)
        {
            var wordNoApo = word.Replace("’", ""); // 어퍼스트로피 없는 텍스트
            var indexApo = word.IndexOf("’"); // 어퍼스트로피는 하나라고 가정, 없으면 -1

            if (wordNoApo.Length <= 3)
            {
                var locations = UtilArray.Sequential(1, word.Length);
                return locations.Where(l => l != indexApo + 1).ToArray();
            }
            else
            {
                var lastLimit = word.Length - 3 + 1;
                if (lastLimit < indexApo)
                    lastLimit -= 1;
                var startLocation = new RangeInteger(1, lastLimit).RandomOne();
                var locations = UtilArray.Sequential(startLocation, startLocation + 3 - 1);
                if (indexApo != -1)
                    locations = locations.Select(l => l > indexApo ? l + 1 : l).ToArray();

                return locations.OrderBy(d => d).ToArray();
            }
        }
        private ExampleData[] buildExamples(ActivityData[] tables, ActivityData problem, int[] problemLocations, ProblemConfig config)
        {
            var list = new List<ExampleData>();

            // 정답 팝콘 추가
            foreach (var loc in problemLocations)
            {
                list.Add(new ExampleData
                {
                    IsAnswer = true,
                    Text = (problem.Word[loc - 1]).ToString()
                });
            }

            // 오답 팝콘 추가
            var answers = list.Select(ex => ex.Text).ToArray();
            var answerCount = list.Count();
            var wrongPool = Enumerable.Range('a', 26)
                                      .Select(x => ((char)x).ToString())
                                      .Where(x => !answers.Contains(x))
                                      .ToArray();
            var wrong = UtilArray.Extract(wrongPool, config.ExamplePopcornCount - answerCount);
            foreach (var w in wrong)
            {
                list.Add(new ExampleData
                {
                    IsAnswer = false,
                    Text = w
                });
            }

            var examData = list.ToArray();
            UtilArray.Shuffle(examData);

            return examData;
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO config = null;
    }
}