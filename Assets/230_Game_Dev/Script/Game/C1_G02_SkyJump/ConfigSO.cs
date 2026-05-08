using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    [CreateAssetMenu(fileName = "C1_G02_ConfigSO", menuName = "DoDoEng/ConfigSO/C1_G02_ConfigSO", order = 2)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public RoundConfig[] RoundConfigs => roundConfigs;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private RoundConfig[] roundConfigs;
    }


    [System.Serializable]
    public class RoundConfig
    {
        [AllowNesting][Label("문제 수")] public int ProblemCount = 5;
        [AllowNesting][Label("상승 구간 층수")] public int RisingFloor = 5;
        [AllowNesting][Label("등장 구름 유형")] public CloudStyle[] CloudStyles;
        [AllowNesting][Label("빠른 구름 확률")][Range(0, 1)] public float FastMoveProbability = 0f;
        [AllowNesting][Label("상승구간당 부스터 개수")] public int BoosterPerRising = 1;
    }
}