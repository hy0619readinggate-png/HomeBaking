using DoDoEng.Common;
using DoDoEng.Game.C1_G01;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng
{
    public class OrderItem : MonoBehaviour
    {
        // Methods
        public void Setup(IceCreamData icd)
        {
            LOG.Info($"Setup() | {icd}", this);

            iceCreamData = icd;

            alphabetTMP.text = icd.Alphabet.ToUpper();
            alphabetTMP.color = ColorTable.One.OrderTextColorDefault;
            iceCreameIMG.color = ColorTable.One.OrderIceCreamColorDefault;
            shadowIMG.color = ColorTable.One.OrderShadowColorDefault;
        }
        public void ShowAnswer()
        {
            LOG.Info($"ShowAnswer()", this);

            alphabetTMP.color = ColorTable.One.OrderTextColor(iceCreamData.ColorID);
            iceCreameIMG.color = ColorTable.One.OrderIceCreamColor(iceCreamData.ColorID);
            shadowIMG.color = ColorTable.One.OrderShadowColor(iceCreamData.ColorID);
        }



        // Fields
        private IceCreamData iceCreamData;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image iceCreameIMG = null;
        [SerializeField] private Image shadowIMG = null;
        [SerializeField] private TextMeshProUGUI alphabetTMP = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}