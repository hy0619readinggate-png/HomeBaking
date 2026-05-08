using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C2_A03;

namespace DoDoEng.Activity.C2_A03
{
    public class C2_A03_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Definitions
        private const int WRONG_EXAMPLE_COUNT = 2;
        private const int BOAT_VARIATION = 3;
        private const int DOLPHIN_VARIATION = 3;


        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            LOG.Info($"onBuild() | {ActIDX}", this);
            LOG.Assert(curriculum.ProblemCount == 6, "curriculum.ProblemCount must be 6", this);

            // 문제 데이터 추출
            var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
            var problems = UtilArray.Extract(problemsPool, curriculum.ProblemCount);

            var others = tables.Where(t => !problems.Contain(t)).ToArray();
            foreach(var o in others)
                LOG.Info($"{o.Phonics}", this);
            
            var boatTypes = UtilArray.Sequential(1, BOAT_VARIATION);
            var boatVariation = UtilArray.Extract(boatTypes, curriculum.ProblemCount);

            var dolphinTypes = UtilArray.Sequential(1, DOLPHIN_VARIATION);
            var dolphinVariation = UtilArray.Extract(dolphinTypes, curriculum.ProblemCount);

            // 문제 데이터 생성
            var problemList = new List<ProblemData>();
            foreach (var (p, i) in problems.Select((p, i) => (p, i)))
            {
                problemList.Add(new ProblemData
                {
                    Index = p.Index,
                    Phonics = p.Phonics,
                    Word = p.Word,
                    BlankWord = p.BlankWord,
                    WordSPR = await loadSprite(p.ImageWord),
                    PhonicsCLIP = await loadSound(p.SoundPhonetic),
                    WordCLIP = await loadSound(p.SoundWord),
                    Examples = await buildExamples(p, others),
                    BoatType = boatVariation[i],
                    DolphinType = dolphinVariation[i]
                });
            }

            // LMS(ActivityProgress) 빈칸수 설정
            BlanksCount = problemList.Count;

            return problemList.ToArray();
        }

        // Functions
        private async UniTask<ExampleData[]> buildExamples(ActivityData problem, ActivityData[] others)
        {
            var list = new List<ActivityData>();

            // 정답 추가
            list.Add(problem);

            // 오답 추가
            var phonetics = others
                            .Where(t => t.Phonics != problem.Phonics && t.Group == problem.Group)
                            .Select(p => p.Phonics)
                            .Distinct()
                            .ToArray();
            var shuffledPhonetics = UtilArray.Shuffle(phonetics);
            var extractedPhonetics = UtilArray.Extract(shuffledPhonetics, WRONG_EXAMPLE_COUNT);

            for (var i = 0; i < extractedPhonetics.Length; i++)
            {
                var phoneticExamples = others.Where(p => p.Phonics == extractedPhonetics[i]).ToArray();
                var ex = UtilArray.ExtractOne(phoneticExamples);
                list.Add(ex);
            }

            // 보기 생성
            var examList = new List<ExampleData>();
            foreach (var ex in list)
            {
                examList.Add(new ExampleData
                {
                    IsAnswer = ex.Index == problem.Index,
                    Phonics = ex.Phonics,
                    PhonicsCLIP = await loadSound(ex.SoundPhonetic)
                });
            }

            return UtilArray.Shuffle(examList.ToArray());
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Phonics;
        public string Word;
        public string BlankWord;
        public Sprite WordSPR;
        public AudioClip PhonicsCLIP;
        public AudioClip WordCLIP;
        public ExampleData[] Examples;
        public int BoatType;
        public int DolphinType;

        public override string ToString()
        {
            return $"<color=red>ProblemData" +
                $"[{Index} | {Phonics} | {Word} | {BlankWord} | {string.Join(",", Examples.Select(ex => ex.Phonics))} | {BoatType} | {DolphinType}]" +
                $"</color>";
        }
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public string Phonics;
        public AudioClip PhonicsCLIP;
    }
}