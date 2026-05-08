using System;
using UnityEngine;

namespace DoDoEng.Common
{
    [CreateAssetMenu(fileName = "SfxCollectionSO", menuName = "DoDoEng/SfxCollectionSO", order = 2)]
    public class SfxCollectionSO : ScriptableObject
    {
        // Properties
        public SfxConfig[] SfxConfigs => sfxConfigs;


        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private SfxConfig[] sfxConfigs;
    }

    [Serializable]
    public class SfxConfig
    {
        public SfxMoment moment;
        public AudioClip clip;
    }

    public enum SfxMoment
    {
        Common_Click = 1000,

        System_Back = 2000,

        Activity_NextProblem = 3000,
        Activity_Click,
        Activity_Correct,
        Activity_Wrong,
        Activity_Complete
    }
}