using System;
using UnityEngine;

namespace DoDoEng.Game.C1_G00
{
    [CreateAssetMenu(fileName = "C1_G00_ConfigSO", menuName = "DoDoEng/ConfigSO/C1_G00_ConfigSO", order = 2)]
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
        //public int IceCreamCount;
        //public int AlphabetMin;
        //public int AlphabetMax;
        //public int CustomerCount;
        //public float CustomerInterval;
        //public float CustomerDuration;
        //public int ThiefCount;
    }
}