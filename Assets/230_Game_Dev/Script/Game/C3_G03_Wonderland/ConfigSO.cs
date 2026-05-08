using NaughtyAttributes;
using UnityEngine;

namespace DoDoEng.Game.C3_G03
{
    [CreateAssetMenu(fileName = "C3_G03_ConfigSO", menuName = "DoDoEng/ConfigSO/C3_G03_ConfigSO", order = 3)]
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