using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A08
{
    public class PachinkoSlot : MonoBehaviour
    {
        // Methods
        public void Setup(Sprite sprite)
        {
            LOG.Info($"Setup()", this);

            examIMG.sprite = sprite;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image examIMG = null;
        [SerializeField] private GameObject sampleGO = null;

        // Unity Messages
        private void Awake()
        {
            examIMG.gameObject.SetActive(true);
            sampleGO.SetActive(false);
        }
        private void Start()
        {
        }
    }
}