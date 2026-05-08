using NaughtyAttributes;
using System;
using UnityEngine;

namespace DoDoEng.Activity.C1_A11
{
    [CreateAssetMenu(fileName = "C1_A11_ConfigSO", menuName = "DoDoEng/ConfigSO/C1_A11_ConfigSO", order = 2)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public ProblemConfig[] ProblemConfigs => problemConfigs;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private ProblemConfig[] problemConfigs;
    }

    [Serializable]
    public class ProblemConfig
    {
        [AllowNesting][Label("전체 꽃 수")] public int ExampleFlowerCount = 15;
        [AllowNesting][Label("정답 꽃 수")] public int AnswerFlowerCount = 8;

        public int WrongFlowerCount => ExampleFlowerCount - AnswerFlowerCount;
    }
}