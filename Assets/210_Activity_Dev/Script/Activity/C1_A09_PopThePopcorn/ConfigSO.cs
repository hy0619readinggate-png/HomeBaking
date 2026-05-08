using NaughtyAttributes;
using System;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    [CreateAssetMenu(fileName = "C1_A09_ConfigSO", menuName = "DoDoEng/ConfigSO/C1_A09_ConfigSO", order = 2)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public ProblemConfig[] ProblemConfigs => problemConfigs;

        // Methods
        public int GetExtraPopcornCount(int loadCornCount)
        {
            return extraPopcornConfigs.SingleOrDefault(c => c.LoadCornCount == loadCornCount)?.ExtraPopcornCount ?? 0;
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private ProblemConfig[] problemConfigs;
        [SerializeField] private ExtraPopcornConfig[] extraPopcornConfigs;
    }

    [Serializable]
    public class ProblemConfig
    {
        [AllowNesting][Label("보기 팝콘 수")] public int ExamplePopcornCount;
        [AllowNesting][Label("정답 팝콘 수(C3_A07에서는 무시)")] public int AnswerPopcornCount;

        public int WrongPopcornCount => ExamplePopcornCount - AnswerPopcornCount;
    }

    [Serializable]
    public class ExtraPopcornConfig
    {
        [AllowNesting][Label("옥수수 추가 횟수")] public int LoadCornCount;
        [AllowNesting][Label("엑스트라 팝콘 수")] public int ExtraPopcornCount;
    }
}