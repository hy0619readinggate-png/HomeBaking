using NaughtyAttributes;
using System;
using UnityEngine;

namespace DoDoEng.Activity.C1_A00
{
    [CreateAssetMenu(fileName = "C1_A00_ConfigSO", menuName = "DoDoEng/ConfigSO/C1_A00_ConfigSO", order = 2)]
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
        //[AllowNesting][Label("전체 꽃 수")] public int ExampleFlowerCount;
        //[AllowNesting][Label("정답 꽃 수")] public int AnswerFlowerCount;

        //public int WrongFlowerCount => ExampleFlowerCount - AnswerFlowerCount;
    }
}