using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


using GameData = DoDoEng.Common.GameData_C4_G03;


namespace DoDoEng.Game.C4_G03
{
    public class C4_G03_ProblemMGR : GameProblemMGR<GameData, ProblemData>
    {
        // Overrides
        protected override async UniTask<ProblemData[]> onBuild(GameList curriculum, GameData[] tables)
        {
            using (LOG.Coroutine($"Build() | {GameIDX}", this))
            {
                var problemList = new List<ProblemData>();

                var tableExtracted = tables.Filter(curriculum.DataMin, curriculum.DataMax).ToArray();
                var sentenceList = new List<SentenceData>();
                var tmpList = new List<SentenceData>();

                foreach (var td in tableExtracted)
                {
                    var chunkNum = 0;
                    var chunkList = new List<string>
                    {
                        td.Chunk1, td.Chunk2, td.Chunk3, td.Chunk4, td.Chunk5
                    };

                    var blockTypeNum = Random.Range(0, 3);
                    if (td.Chunk5 == "")
                    {
                        if (td.Chunk4 == "")
                            chunkNum = 3;

                        else chunkNum = 4;
                    }
                    else chunkNum = 5;

                    var sd = new SentenceData
                    {
                        ChunkNum = chunkNum,
                        Chunk = chunkList,
                        BlockType = blockTypeNum,

                        Sentence = td.Sentence,
                        SoundCLIP = await loadSound(td.SoundSentence),
                        sentenceIMG = await loadSprite(td.ImageSentence),
                    };
                    tmpList.Add(sd);
                }

                IEnumerable<SentenceData> query = tmpList.OrderBy(tmp => tmp.ChunkNum);
                foreach (SentenceData sentence in query) sentenceList.Add(sentence);

                //문제는 총 6개
                int dataNum = tmpList.Count;
                var rndIdx = UtilArray.Random(0, dataNum - 1);

                var selectRndIdx = rndIdx.Skip(0).Take(6).ToArray();
                var restRndIdx = rndIdx.Skip(6).Take(dataNum - 6).ToArray();

                var selectedIdx = selectRndIdx.OrderBy(n => n).ToArray();

                int problemIdx = 0;
                var selectSentenceList = new List<SentenceData>();
                var restSentenceList = new List<SentenceData>();

                restSentenceList = sentenceList;


                foreach (var config in config.ProblemConfigs)
                {
                    selectSentenceList.Add(sentenceList[selectedIdx[problemIdx]]);

                    selectSentenceList[problemIdx].BlockCount = config.BlockCount;
                    selectSentenceList[problemIdx].CorrectBlockCount = config.CorrectBlockCount;

                    problemList.Add(new ProblemData
                    {
                        Sentence = selectSentenceList[problemIdx],
                        RestSentences = restSentenceList.ToArray(),
                    });
                    problemIdx++;
                };

                return problemList.ToArray();
            }
        }



        // Unity Inspectors
        [Header("★ Configs")]
        [SerializeField] private ConfigSO config = null;
    }

    public class ProblemData
    {
        public SentenceData Sentence;
        public SentenceData[] RestSentences;

        public override string ToString()
        {
            var sentenceStr = string.Join(",", Sentence);
            return $"<color=red>ProblemData [({sentenceStr})]</color>";
        }

    }
    public class SentenceData
    {
        public string Sentence;
        public int ChunkNum; //청크 길이 : 3-5
        public int BlockType; //0. 호박, 1. 바위, 2.수풀

        public List<string> Chunk;
        public int BlockCount; // 추가될 블럭 수
        public int CorrectBlockCount; // 정답 위치에 놓일 블럭 수

        public AudioClip SoundCLIP;
        public Sprite sentenceIMG;

        public override string ToString()
        {
            return $"<color=red>ProblemData [{ChunkNum} | {BlockType}| {Sentence}]</color>";
        }

    }
}