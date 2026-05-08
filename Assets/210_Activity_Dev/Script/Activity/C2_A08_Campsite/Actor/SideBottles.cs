using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A08
{
    public class SideBottles : MonoBehaviour
    {
        // Methods
        public void Init()
        {
            bottleGOs.ForEach(go => go.SetActive(false));
            index = 0;
        }
        public void Setup(Sprite image, int count)
        {
            LOG.Info($"Setup()", this);

            bottleGOs[index].gameObject.SetActive(true);
            decoIMG[index].sprite = image;
            bottlesFirefly[index].Setup(count);
            index++;
        }



        // Fields
        private int index = 0;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] bottleGOs = null;
        [SerializeField] private Image[] decoIMG = null;
        [SerializeField] private FireflyInBottle[] bottlesFirefly = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}