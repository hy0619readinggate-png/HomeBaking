using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class Progress : MonoBehaviour
    {
        // Methods
        public void Setup(int gemVariation)
        {
            LOG.Function(this, $"{gemVariation}");

            gemVariationGO.ForEach((i, go) => go.SetActive(i + 1 == gemVariation));
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] gemVariationGO = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}