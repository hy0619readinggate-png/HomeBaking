using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using GameData = DoDoEng.Common.GameData_C2_G03;

namespace DoDoEng.Game.C2_G03
{
    public class C2_G03_ProblemMGR : GameProblemMGR<GameData, ProblemData>
    {
        // Propertes
        public int[] QuizCounts => Problems.Select(p => p.QuizCount).ToArray();
        public int TotalProblemCount => QuizCounts.Sum();

        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            System.Random random = new System.Random();

            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                // 문제 데이터 생성
                int round = 0;
                List<ProblemData> problemList = new List<ProblemData>();
                foreach (var config in config.RoundConfigs)
                {
                    tables = tables.OrderBy(x => random.Next()).ToArray();

                    string[] phonetics = tables.Select(t => t.Text).Distinct().ToArray();
                    GameData[] tablesPhonetics = tables.Where(t => phonetics.Contain(t.Text)).ToArray();

                    List<QuizData> datas = new List<QuizData>();
                    foreach (GameData t in tablesPhonetics)
                    {
                        datas.Add(new QuizData
                        {
                            Phonetic = t.Text,
                            Word = t.Word,
                            //PhoneticCLIP = await loadSound(t.SoundPhonetic),
                            WordSPR = await loadSprite(t.Image),
                            WordCLIP = await loadSound(t.SoundWord)
                        });
                    }

                    var quizs = new List<Quiz>();
                    int k = 0;
                    do
                    {
                        foreach (string p in phonetics)
                        {
                            QuizData[] words = new QuizData[2];
                            System.Array.ConstrainedCopy(datas.Where(t => t.Phonetic == p).OrderBy(x => random.Next()).ToArray(), 0, words, 0, 2);
                            quizs.Add(new Quiz
                            {
                                Phonetic = p,
                                Word = words[k].Word,
                                Words = words.OrderBy(x => random.Next()).ToArray()
                            });
                            if (quizs.Count() >= config.QuizCount) break;
                        }
                        k = k == 0 ? 1 : 0;
                    } while (quizs.Count() < config.QuizCount);

                    problemList.Add(new ProblemData
                    {
                        Round = ++round,
                        SlideSpeed = config.SlideSpeed,
                        Quizs = quizs.ToArray()
                    });
                    {
                        int n = 0;
                        foreach (var t in quizs.ToArray()) Debug.Log($"X({++n}) => {t}");
                    }
                }

                return problemList.ToArray();
            }
        }

        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO config = null;
    }

    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////////
    /// 데이타 구조
    /// </summary>
    public class QuizData
    {
        public string Phonetic;
        public string Word;
        //public AudioClip PhoneticCLIP;
        public Sprite WordSPR;
        public AudioClip WordCLIP;
        public override string ToString()
        {
            return Word;
        }
    }

    public class Quiz
    {
        public string Phonetic;
        public string Word;
        public QuizData[] Words;
        public override string ToString()
        {
            string str = string.Join(", ", Words.Select(t => t.Word));
            return $"{Word}=>({str})";
        }
    }

    public class ProblemData
    {
        public int Round;
        public float SlideSpeed;
        public Quiz[] Quizs;

        public int QuizCount => Quizs.Count();
        public Quiz DEV_GetBullet(string phonics)
        {
            return Quizs.Where(b => b.Phonetic == phonics).FirstOrDefault();
        }
        public override string ToString()
        {
            var str = string.Join(", ", Quizs.Select(b => b));
            return $"<color=red>ProblemData [{Round} | {str}]</color>";
        }
    }
}