using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Game.C4_G03
{
    [CreateAssetMenu(fileName = "C4_G03_ConfigSO", menuName = "DoDoEng/ConfigSO/C4_G03_ConfigSO", order = 3)]
    public class ConfigSO : ScriptableObject
    {
        // Properties
        public ProblemConfig[] ProblemConfigs => problemConfigs;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private ProblemConfig[] problemConfigs;
    }


    [System.Serializable]
    public class ProblemConfig
    {
        [AllowNesting][Label("추가 될 블록 수")] public int BlockCount = 0;
        [AllowNesting][Label("반드시 놓일 정답 블록 수")] public int CorrectBlockCount = 0;
    }
}