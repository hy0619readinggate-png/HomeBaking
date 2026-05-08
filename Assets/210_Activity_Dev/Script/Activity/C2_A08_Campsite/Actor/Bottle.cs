using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A08
{
    public class Bottle : MonoBehaviour
    {
        // Methods
        public void Setup(Sprite img, bool showIcon)
        {
            LOG.Info($"Setup()", this);

            examIMG.sprite = img;
            examIMG.gameObject.SetActive(!showIcon);
            decoIMG.sprite = img;
            decoIMG.gameObject.SetActive(showIcon);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image examIMG = null;
        [SerializeField] private Image decoIMG = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}