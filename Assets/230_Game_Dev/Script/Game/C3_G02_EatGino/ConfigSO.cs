using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    [CreateAssetMenu(fileName = "C3_G02_ConfigSO", menuName = "DoDoEng/ConfigSO/C3_G02_ConfigSO", order = 3)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public LevelConfigs[] LevelConfigs => levelConfigs;



        // Unity Inspectors
        [SerializeField] private LevelConfigs[] levelConfigs;
    }


    [System.Serializable]
    public class LevelConfigs
    {
        [AllowNesting][Label("블록 수")] public int jellyBlockCount = 0;
        [AllowNesting][Label("아이템 수")] public int itemCount = 0;
        [AllowNesting][Label("타이머")] public float Timer = 0.0f;

        [AllowNesting][Label("투시 아이템 시간")] public float scanTime = 0.0f;
        [AllowNesting][Label("시계 아이템 시간")] public float clockTime = 0.0f;

        [AllowNesting][Label("정답 베네핏 속도")] public float benefitVelocity = 0.0f;
        [AllowNesting][Label("오답 패널티 속도")] public float penaltyVelocity = 0.0f;
        [AllowNesting][Label("정오답 이펙트 시간")] public float effectTime = 0.0f;
    }
}