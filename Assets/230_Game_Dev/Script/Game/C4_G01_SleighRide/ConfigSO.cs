using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Game.C4_G01
{
    [CreateAssetMenu(fileName = "C4_G01_ConfigSO", menuName = "DoDoEng/ConfigSO/C4_G01_ConfigSO", order = 2)]
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
        [AllowNesting][Label("슬라이드 속도")] public float SlideSpeed;
        [AllowNesting][Label("퀴즈 수")] public int QuizCount;
    }
}