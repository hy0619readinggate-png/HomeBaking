using NaughtyAttributes;
using System;
using UnityEngine;

namespace DoDoEng.Activity.C2_A05
{
    [CreateAssetMenu(fileName = "C2_A05_ConfigSO", menuName = "DoDoEng/ConfigSO/C2_A05_ConfigSO", order = 2)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public ProblemConfig ProblemConfig => problemConfig;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private ProblemConfig problemConfig;
    }

    [Serializable]
    public class ProblemConfig
    {
        [AllowNesting][Label("전체 벌 수")] public int ExampleBeeCount;
        [AllowNesting][Label("정답 벌 수")] public int AnswerBeeCount;

        public int WrongBeeCount => ExampleBeeCount - AnswerBeeCount;
    }
}