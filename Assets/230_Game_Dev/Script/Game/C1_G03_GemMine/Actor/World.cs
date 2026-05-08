using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public class World : MonoBehaviour
    {
        // Methods
        public void Setup(int variation)
        {
            LOG.Function(this, $"{variation}");

            gate1.ForEach((i, go) => go.SetActive(i + 1 == variation));
            gate2.ForEach((i, go) => go.SetActive(i + 1 == variation));
            foreground.ForEach((i, go) => go.SetActive(i + 1 == variation));
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] gate1 = null;
        [SerializeField] private GameObject[] gate2 = null;
        [SerializeField] private GameObject[] foreground = null;
    }
}