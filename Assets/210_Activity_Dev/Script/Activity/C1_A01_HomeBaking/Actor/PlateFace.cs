using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A01
{
    public class PlateFace : MonoBehaviour
    {
        // Methods
        public void Normal()
        {
            LOG.Info($"Normal()", this);

            normalGO.SetActive(true);
            smileGO.SetActive(false);
        }
        public void Smile()
        {
            LOG.Info($"Smile()", this);

            normalGO.SetActive(false);
            smileGO.SetActive(true);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject normalGO = null;
        [SerializeField] private GameObject smileGO = null;

        // Unity Messages
        private void Awake()
        {
            normalGO.SetActive(true);
            smileGO.SetActive(false);
        }
        private void Start()
        {
        }
    }
}