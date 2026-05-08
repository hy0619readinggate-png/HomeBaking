using beyondi.Util;
using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Game.C1_G01
{
    [CreateAssetMenu(fileName = "C1_G01_ConfigSO", menuName = "DoDoEng/ConfigSO/C1_G01_ConfigSO", order = 2)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public int InitialHP => initialHP;
        public RoundConfig[] RoundConfigs => roundConfigs;



        // Unity Inspectors
        [SerializeField] private int initialHP = 5;
        [SerializeField] private RoundConfig[] roundConfigs;
    }


    [System.Serializable]
    public class RoundConfig
    {
        [AllowNesting][Label("사용할 아이스크림 개수")][Range(4, 6)] public int IceCreamCount = 4;
        [AllowNesting][Label("주문 알파벳 수")] public RangeInteger OrderAlphabet = new RangeInteger(1, 3);
        [AllowNesting][Label("손님 수")] public int CustomerCount = 10;
        [AllowNesting][Label("손님 방문 간격")] public float CustomerInterval = 5;
        [AllowNesting][Label("손님 대기 시간")] public float CustomerDuration = 8;
        [AllowNesting][Label("도둑 등장 횟수")] public int ThiefCount = 0;
        [AllowNesting][Label("도둑 대기 시간")] public int ThiefDuration = 3;
        [AllowNesting][Label("도둑 출현 순서")] public RangeInteger ThiefOrder = new RangeInteger(4, 8);
    }
}