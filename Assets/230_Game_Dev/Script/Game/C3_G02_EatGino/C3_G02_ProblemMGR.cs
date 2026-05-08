using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameData = DoDoEng.Common.GameData_C3_G02;

namespace DoDoEng.Game.C3_G02
{
    public class C3_G02_ProblemMGR : GameProblemMGR<GameData, LevelData>
    {
        // Overrides
        protected override async UniTask<LevelData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                var levelList = new List<LevelData>();
                var wordList = new List<wordData>();
                int totalJellyNum = 0;

                var tableExtracted = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();

                foreach (var td in tableExtracted)
                {
                    var levelWord = new wordData()
                    {
                        Word = td.Word,
                        SoundClip = await loadSound(td.SoundWord),
                        WordIMG = await loadSprite(td.ImageWord),
                    };

                    wordList.Add(levelWord);
                }

                foreach (var config in config.LevelConfigs)
                    totalJellyNum += config.jellyBlockCount;

                foreach (var config in config.LevelConfigs)
                {
                    var rndIdx = UtilArray.Random(0, wordList.Count - 1);

                    List<wordData> RndWord = new();

                    for (int i = 0; i < config.jellyBlockCount; i++)
                        RndWord.Add(wordList[rndIdx[i]]);

                    levelList.Add(new LevelData
                    {
                        WordList = RndWord,
                        TotalJellyCount = totalJellyNum,
                        JellyBlockCount = config.jellyBlockCount,
                        ItemCount = config.itemCount,
                        Timer = config.Timer,

                        ScanTime = config.scanTime,
                        ClockTime = config.clockTime,

                        BenefitVelocity = config.benefitVelocity,
                        PenaltyVelocity = config.penaltyVelocity,
                        EffectTime = config.effectTime,
                    });

                }
                return levelList.ToArray();
            }
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO config = null;
    }

    public class LevelData
    {
        public List<wordData> WordList;

        public int JellyBlockCount;
        public int TotalJellyCount;
        public int ItemCount;
        public float Timer;

        public float ScanTime;
        public float ClockTime;

        public float BenefitVelocity;
        public float PenaltyVelocity;
        public float EffectTime;
    }

    public class wordData
    {
        public string Word;
        public AudioClip SoundClip;
        public Sprite WordIMG;

        public override string ToString()
        {
            return $"<color=red>WordData [{Word}]</color>";
        }
    }
}