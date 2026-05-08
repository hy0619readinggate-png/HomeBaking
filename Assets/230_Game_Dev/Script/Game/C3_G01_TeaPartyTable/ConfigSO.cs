using UnityEngine;
using System;
using NaughtyAttributes;
using beyondi.Util;

namespace DoDoEng.Game.C3_G01
{
    [CreateAssetMenu(fileName = "C3_G01_ConfigSO", menuName = "DoDoEng/ConfigSO/C3_G01_ConfigSO", order = 3)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        [AllowNesting][Label("단어 최대 제한길이")] public int WordMaxLength =8;

        // 4자 이하는 전체 알파벳
        // 4자 초과는 4자만 랜덤을 표시
        // ex) 6자의 경우 4자만 뽑아서 표시
        [AllowNesting][Label("단어 규칙 제한길이")] public int WordLimitLength = 3;

        public RoundConfig[] RoundConfigs => roundConfigs;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private RoundConfig[] roundConfigs;
    }


    [Serializable]
    public class RoundConfig
    {
        [AllowNesting][Label("문제 수")] public int ProblemCount = 5;
        [AllowNesting][Label("동시에 등장하는 오브젝트 최대 개수")] public int ObjectCountAtOnceMax = 2;
        [AllowNesting][Label("오브젝트 생성 간격(초)")][Range(1, 5)] public float ObjectInterval;
        [AllowNesting][Label("오브젝트 속도")][Range(1, 5)] public float ObjectSpeed = 1f;
        [Space()]
        [AllowNesting][Label("빗자루 등장 최소 개수")] public int BroomMinCount;
        [Space()]
        [AllowNesting][Label("등장하는 장애물 종류")] public ObstacleType[] ObstacleTypes;
    }
}