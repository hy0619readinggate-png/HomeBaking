using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A08
{
    public class FireflyInBottle : MonoBehaviour
    {
        // Methods
        public void Setup(int count)
        {
            LOG.Info($"Setup() | {count}", this);

            int[] activeIndices = UtilArray.Random(0, fireflies.Length - 1);
            activeIndices.ForEach((i, a) => fireflies[i].SetActive(a < count));
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] fireflies = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}