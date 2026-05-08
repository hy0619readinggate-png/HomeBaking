using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

using ActivityData = DoDoEng.Common.ActivityData_C1_A04;

namespace DoDoEng.Activity.C1_A04
{
    public class C1_A04_ProblemMGR : ActivityProblemMGR<ActivityData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(ActivityList curriculum, ActivityData[] tables)
        {
            using (LOG.Coroutine($"onBuild() | {ActIDX}", this))
            {
                LOG.Assert(curriculum.ProblemCount == 1, "problem count must be 1", this);

                // 정답 랜덤 선택
                var answerIdx = UtilArray.RandomOne(0, 1);

                // 테이블 데이터 추출
                var problemsPool = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
                var problems = UtilArray.Extract(problemsPool, 2);

                // 리소스 로딩
                var paintingPB = new GameObject[]
                {
                    await loadPrefab(problems[0].PrefabsPainting),
                    await loadPrefab(problems[1].PrefabsPainting)
                };
                var phoneticCLIP = await loadSound(problems[answerIdx].SoundPhonetic);
                var wordCLIP = new AudioClip[]
                {
                    await loadSound(problems[0].SoundWord),
                    await loadSound(problems[1].SoundWord)
                };

                // 문제 데이터 생성
                var problem = new ProblemData
                {
                    Index = problems[answerIdx].Index,
                    Text = problems[answerIdx].Text,
                    Word = problems[answerIdx].Word,
                    Alphabet = problems[answerIdx].Alphabet,
                    TrimWord = problems[answerIdx].TrimWord,
                    Examples = new ExampleData[]
                    {
                        new ExampleData { IsAnswer = answerIdx == 0, WordPB = paintingPB[0], WordCLIP = wordCLIP[0] },
                        new ExampleData { IsAnswer = answerIdx == 1, WordPB = paintingPB[1], WordCLIP = wordCLIP[1] }
                    },
                    PaintPB = paintingPB[answerIdx],
                    PhoneticCLIP = phoneticCLIP,
                    WordCLIP = wordCLIP[answerIdx],
                    AnswerID = answerIdx + 1
                };

                // LMS(ActivityProgress) 빈칸수 설정
                BlanksCount = 1;

                return new ProblemData[] { problem };
            }
        }
    }

    public class ProblemData
    {
        public int Index;
        public string Text;
        public string Word;
        public string Alphabet;
        public string TrimWord;
        public ExampleData[] Examples;
        public GameObject PaintPB;
        public AudioClip PhoneticCLIP;
        public AudioClip WordCLIP;
        public int AnswerID;
    }
    public class ExampleData
    {
        public bool IsAnswer;
        public GameObject WordPB;
        public AudioClip WordCLIP;
    }
}