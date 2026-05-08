using beyondi.Util;
using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    [CreateAssetMenu(fileName = "C2_G02_ConfigSO", menuName = "DoDoEng/ConfigSO/C2_G02_ConfigSO", order = 2)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public RoundConfig[] RoundConfigs => roundConfigs;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private RoundConfig[] roundConfigs;
    }


    public enum MonsterType
    {
        Turtle = 1,
        HermitCrab,
        Crab
    }

    [System.Serializable]
    public class RoundConfig
    {
        [AllowNesting][Label("사용할 음가 개수")] public int PhoneticValueCount = 4;
        [AllowNesting][Label("컨베이어벨트 속도")] public float ConveyorSpeed = 2;
        [AllowNesting][Label("몬스터 수")] public int MonsterCount = 10;
        [AllowNesting][Label("몬스터 출현 간격")] public Range MonsterInterval = new Range(5, 6);
        [AllowNesting][Label("몬스터 종류")] public MonsterType MonsterType = MonsterType.Turtle;
        [AllowNesting][Label("몬스터 속도")] public float MonsterSpeed = 2;
    }
}