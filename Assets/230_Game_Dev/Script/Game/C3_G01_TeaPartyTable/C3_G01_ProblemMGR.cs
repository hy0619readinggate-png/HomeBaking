using DoDoEng.Game.Framework;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using DoDoEng.Common;
using UnityEngine;
using System.Linq;
using beyondi.Util;
using System;

using GameData = DoDoEng.Common.GameData_C3_G01;


namespace DoDoEng.Game.C3_G01
{
    public class C3_G01_ProblemMGR : GameProblemMGR<GameData, ProblemData>
    {

        // Properties
        public ConfigSO Config => config;
        public int[] ProblemCounts => Problems.Select(p => p.WordDatas.Length).ToArray();
        public int TotalProblemCount => ProblemCounts.Sum();
        public bool HasAP => hasApostrophe;


        // Methods
        public int[] GetLetterOrderOfWord(int wNO)
        {
            // Apostrophe 제외
            var word = Current.WordDatas[wNO - 1].Word;
            var wordLength = Current.WordDatas[wNO - 1].Word.Length;

            int apostropheIdx = 99; // 나올 수 없는 index로 설정

            for (int i = 0; i < wordLength; i++) if (word[i].Equals('’')) apostropheIdx = i;

            if (wordLength <= config.WordLimitLength)
            {
                if (apostropheIdx != 99)
                    return UtilArray.Sequential(0, wordLength - 1).Where(n => n != apostropheIdx).ToArray();
                else return UtilArray.Sequential(0, wordLength - 1);
            }
            else if (wordLength - config.WordLimitLength == 1)
            {
                if (apostropheIdx != 99)
                    return UtilArray.Sequential(0, wordLength - 1).Where(n => n != apostropheIdx).ToArray();
                else
                {
                    int persentage = UnityEngine.Random.Range(0, 100);
                    if (persentage < 50)
                    {
                        // 앞
                        return UtilArray.Sequential(0, 2);
                    }
                    else
                    {
                        // 뒤
                        return UtilArray.Sequential(1, 3);
                    }


                    // ---------------------
                    //int[] orders = new int[config.WordLimitLength];
                    //var randArr = UtilArray.Random(0, wordLength - 1).Where(n => n != apostropheIdx).ToArray();
                    //for (int i = 0; i < config.WordLimitLength; i++)
                    //    orders[i] = randArr[i];

                    //Array.Sort(orders);
                    //return orders;
                }
            }
            else
            {
                int fill = wordLength - config.WordLimitLength;
                if (apostropheIdx != 99)
                    fill -= 1;


                var fillPick = UnityEngine.Random.Range(0, fill + 1);
                var arr = UtilArray.Sequential(0, wordLength - 1).Where(n => n != apostropheIdx).ToArray();


                int[] orders = new int[config.WordLimitLength];
                for (int i = 0; i < config.WordLimitLength; i++)
                {
                    orders[i] = arr[i + fillPick];
                }

                return orders;


                // ---------------------
                //int[] orders = new int[config.WordLimitLength];
                //var randArr = UtilArray.Random(0, wordLength - 1).Where(n => n != apostropheIdx).ToArray();
                //for (int i = 0; i < config.WordLimitLength; i++)
                //    orders[i] = randArr[i];

                //Array.Sort(orders);
                //return orders;
            }
        }
        public char[] GetLetters(int wNO)
        {
            char[] letters = new char[config.WordMaxLength];

            var lettersOfWord = Current.WordDatas[wNO - 1].Word.ToCharArray();
            var tmpLetters = lettersOfWord.Where(x => x != '’').ToArray();

            //lettersOfWord.CopyTo(letters, 0);
            tmpLetters.CopyTo(letters, 0);

            var otherCount = config.WordMaxLength - tmpLetters.Length;
            if (otherCount > 0)
            {
                List<char> alphabets = new List<char>(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' });
                alphabets.RemoveAll(x => tmpLetters.Contain(x));

                var others = UtilArray.Extract(alphabets.ToArray(), otherCount);
                //others.CopyTo(letters, lettersOfWord.Length);
                others.CopyTo(letters, tmpLetters.Length);
            }

            return letters = UtilArray.Shuffle(letters);
        }
        public int GetBroomAvailableCount(int wNO)
        {
            // Broom 사용 횟수
            // 단어 글자수 제한길이 보다 작으면 1회, 크면 3회
            return Current.WordDatas[wNO - 1].Word.Length < config.WordLimitLength ? 1 : 3;
        }



        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                var round = 1;
                var problemList = new List<ProblemData>();

                foreach (var config in config.RoundConfigs)
                {
                    // 각 라운드 마다 5문제
                    var tableExtracted = UtilArray.Extract(tables, config.ProblemCount);
                    var apostrophe = "’";
                    var wordList = new List<WordData>();
                    foreach (var td in tableExtracted)
                    {
                        if (td.Word.Contains(apostrophe)) hasApostrophe = true;
                        else hasApostrophe = false;

                        var id = new WordData
                        {
                            Word = td.Word.ToLower(),

                            SoundClip = await loadSound(td.SoundWord)
                        };


                        wordList.Add(id);
                    }


                    List<GameObject> obstacleList = new List<GameObject>();
                    foreach (var obstacleType in config.ObstacleTypes)
                    {
                        var pickObstacles = obstacles.Where(p => p.ObstacleType == obstacleType).Select(s => s.gameObject);
                        foreach (var pickObstacle in pickObstacles)
                            obstacleList.Add(pickObstacle);
                    }


                    problemList.Add(new ProblemData
                    {
                        Round = round++,
                        WordDatas = wordList.ToArray(),

                        ObjectCountMAX = config.ObjectCountAtOnceMax,
                        Speed = config.ObjectSpeed,
                        Interval = config.ObjectInterval,

                        BroomMinCount = config.BroomMinCount,

                        Alphabet = alphabet.gameObject,
                        Broom = broom.gameObject,
                        Obstacles = obstacleList.ToArray()
                    });

                }
                return problemList.ToArray();
            }
        }

        private bool hasApostrophe = false;

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TableObject alphabet = null;
        [SerializeField] private TableObject broom = null;
        [SerializeField] private TableObject[] obstacles = null;
        [Header("★ Configs")]
        [SerializeField] private ConfigSO config = null;
    }

    public class ProblemData
    {
        public int Round;
        public WordData[] WordDatas;

        public int ObjectCountMAX;
        public float Speed;
        public float Interval;

        public int BroomMinCount;

        public GameObject Alphabet;
        public GameObject Broom;
        public GameObject[] Obstacles;
    }
    public class WordData
    {
        public string Word;
        public AudioClip SoundClip;
    }
}