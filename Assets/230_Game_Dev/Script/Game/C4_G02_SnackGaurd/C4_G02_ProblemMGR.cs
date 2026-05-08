using UnityEngine;
using DoDoEng.Game.Framework;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections.Generic;
using beyondi.Util;
using System.Linq;

using GameData = DoDoEng.Common.GameData_C4_G02;


namespace DoDoEng.Game.C4_G02
{
    public class C4_G02_ProblemMGR : GameProblemMGR<GameData, ProblemData>
    {
        public int[] CustomerCounts => Problems.Select(p => p.QuestionDatas.Length).ToArray();
        public int TotalProblemCount => CustomerCounts.Sum();


        // Properties
        public ConfigSO Config => config;



        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                var round = 1;
                var problemList = new List<ProblemData>();


                foreach (var config in config.RoundConfigs)
                {
                    var tableExtracted = UtilArray.Extract(tables, config.ProblemCount);
                    var questionWordDatas = new List<WordData>();

                    foreach (var td in tableExtracted)
                    {
                        int answerIndex = td.Blank_Index - 1;

                        List<string> list = new List<string>();
                        string[] questionTexts = new string[] { td.Chunk1, td.Chunk2, td.Chunk3, td.Chunk4, td.Chunk5 };

                        WordData wordData = new WordData();
                        wordData.AnswerTextIndex = answerIndex;
                        wordData.AnswerText = questionTexts[answerIndex];
                        wordData.QuestionTexts = questionTexts;
                        wordData.SoundClip = await loadSound(td.SoundSentence);

                        questionWordDatas.Add(wordData);
                    }


                    var exampleWords = new List<string>();
                    foreach (var data in questionWordDatas)
                    {
                        foreach (var text in data.QuestionTexts)
                        {
                            if (text.Equals(data.AnswerText) == false && exampleWords.Contains(text) == false)
                            {
                                exampleWords.Add(text);
                            }
                        }
                    }


                    problemList.Add(new ProblemData
                    {
                        Round = round++,

                        // 
                        QuestionDatas = questionWordDatas.ToArray(),
                        ExampleTexts = exampleWords.ToArray(),


                        // Generator 
                        BubbleGeneratorConfig = new BubbleGeneratorConfig
                        {
                            AvailableGeneratorCount = config.BubbleGeneratorCount,

                            SpawnIntervalMIN = config.BubbleIntervalMIN,
                            SpawnIntervalMAX = config.BubbleIntervalMAX,

                            BubbleSpeeds = config.BubbleSpeeds
                        },


                        BubbleGeneratorRule = new BubbleGeneratorRule
                        {
                            MonsterBubbleCountMIN_BeforeText = config.MonsterMinCount,
                            MonsterBubbleCountMAX_BeforeText = config.MonsterMaxCount,

                            ProbabilityText = config.ContinueTextPercentage,
                            ProbabilityAnswer = config.AnswerBubble_Ratio
                        },

                    });
                }

                return problemList.ToArray();
            }
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private ConfigSO config = null;

    }

    public class ProblemData
    {
        public int Round;

        public WordData[] QuestionDatas;
        public string[] ExampleTexts;

        public BubbleGeneratorConfig BubbleGeneratorConfig;
        public BubbleGeneratorRule BubbleGeneratorRule;
    }
    public class WordData
    {
        public int AnswerTextIndex;
        public string AnswerText;
        public string[] QuestionTexts;
        public AudioClip SoundClip;
    }


    public class BubbleGeneratorConfig
    {
        public int AvailableGeneratorCount = 0;

        public float SpawnIntervalMIN = 0;
        public float SpawnIntervalMAX = 0;

        public float[] BubbleSpeeds = null;
    }
    public class BubbleGeneratorRule
    {
        public int MonsterBubbleCountMIN_BeforeText;
        public int MonsterBubbleCountMAX_BeforeText;

        public int ProbabilityText = 0;
        public int ProbabilityAnswer = 0;
    }
}