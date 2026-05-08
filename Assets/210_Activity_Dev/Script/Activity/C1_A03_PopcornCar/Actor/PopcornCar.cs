
using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A03
{
    public class PopcornCar : MonoBehaviour
    {
        // Methods
        public void Setup(int id)
        {
            LOG.Info($"Setup() | {id}", this);

            repaireds.ForEach((i, go) => go.SetActive(id > i));
            brokens.ForEach((i, go) => go.SetActive(id == i));
        }
        public Transform ClearBroken(int id)
        {
            var broken = brokens[id - 1];
            broken.gameObject.SetActive(false);
            return broken.transform;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] repaireds = null;
        [SerializeField] private GameObject[] brokens = null;

        // Unity Messages
        private void Awake()
        {
            repaireds.ForEach((i, go) => go.SetActive(false));
            brokens.ForEach((i, go) => go.SetActive(0 == i));
        }
        private void Start()
        {
        }
    }
}