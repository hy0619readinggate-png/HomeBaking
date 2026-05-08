using UnityEngine;
using NaughtyAttributes;
using System;

namespace DoDoEng.Game.C4_G02
{
    [CreateAssetMenu(fileName = "C4_G02_ConfigSO", menuName = "DoDoEng/ConfigSO/C4_G02_ConfigSO", order = 4)]
    public class ConfigSO : ScriptableObject
    {
        public RoundConfig[] RoundConfigs => roundConfigs;

        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private RoundConfig[] roundConfigs;
    }

    [Serializable]
    public class RoundConfig
    {
        [AllowNesting][Label("문제 수")] public int ProblemCount = 5;
        [Space()]
        [AllowNesting][Label("물방울 생성기 개수")] public int BubbleGeneratorCount = 3;
        [Space()]
        [AllowNesting][Label("물방울 생성 최소시간")] public float BubbleIntervalMIN = 2;
        [AllowNesting][Label("물방울 생성 최대시간")] public float BubbleIntervalMAX = 4.5f;
        //[AllowNesting][Label("물방울 생성 시간단위")] public float BubbleIntervalUnit = 0.5f;

        // 물방울 생성기 마다 속도가 다름
        // 0번인경우 가장 왼쪽
        // 1번인 경우 중간
        // 2번인 경우 가장 오른쪽
        [Space()]
        [AllowNesting][Label("물방울 속도")] public float[] BubbleSpeeds;
        [Space()]
        [AllowNesting][Label("텍스트 보기 앞 등장 몬스터 수(최소)")] public int MonsterMinCount = 4;
        [AllowNesting][Label("텍스트 보기 앞 등장 몬스터 수(최대)")] public int MonsterMaxCount = 8;

        // 50%
        [Space()]
        [AllowNesting][Label("텍스트 보기 후 텍스트가 이어서 등장할 확률"), Range(0, 100)] public int ContinueTextPercentage = 100;

        // Round1 : 80% | Round2 : 70% | Round3 : 60%
        [Space()]
        [AllowNesting][Label("텍스트 정답 보기일 확률"), Range(0, 100)] public int AnswerBubble_Ratio = 100;

    }
}