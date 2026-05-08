using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public class ProgressItem : MonoBehaviour
    {
        // Methods
        public void Switch(bool on)
        {
            LOG.Info($"Switch() | {on}", this);

            normalGO.SetActive(!on);
            activeGO.SetActive(on);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject normalGO = null;
        [SerializeField] private GameObject activeGO = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}