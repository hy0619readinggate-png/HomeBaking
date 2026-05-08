using NaughtyAttributes;
using System;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    [CreateAssetMenu(fileName = "C1_G03_ConfigSO", menuName = "DoDoEng/ConfigSO/C1_G03_ConfigSO", order = 2)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public RoundConfig[] RoundConfigs => roundConfigs;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private RoundConfig[] roundConfigs;
    }


    [Serializable]
    public class RoundConfig
    {
        [AllowNesting][Label("문제 수")] public int ProblemCount = 5;
        [AllowNesting][Label("제한 시간(초)")] public int Duration = 300;
        [AllowNesting][Label("잭 이동 속도")] public int JackSpeed = 5;
    }
}