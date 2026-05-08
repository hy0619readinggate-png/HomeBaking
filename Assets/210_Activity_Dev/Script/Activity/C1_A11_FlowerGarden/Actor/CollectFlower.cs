using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C1_A11
{
    public class CollectFlower : MonoBehaviour
    {
        // Properties
        public bool IsEmpty { get; private set; }

        // Methods
        public void Init(FlowerNormalParam param)
        {
            LOG.Info($"Init() | {param}", this);

            this.param = param;
        }
        public void Clear()
        {
            LOG.Info($"Clear()", this);

            clear();
        }
        public void Show(int typeIdx, int colorIdx, string alphabet)
        {
            LOG.Info($"Show() | {typeIdx}, {colorIdx}, {alphabet}", this);

            var idx = typeIdx * 5 + colorIdx;
            foreach (var (skin, i) in flowSkins.Select((p, i) => (p, i)))
                skin.SetActive(typeIdx * param.colorCount + colorIdx == i);

            if (typeIdx == 0)
                alphabetTXT.color = param.petalTextColor;
            if (typeIdx == 1)
            {
                var textColor = colorIdx switch
                {
                    0 => param.blueTulipTextColor,
                    1 => param.redTulipTextColor,
                    2 => param.purpleTulipTextColor,
                    3 => param.yellowTulipTextColor,
                    4 => param.pinkTulipTextColor,
                    _ => Color.white
                };
                alphabetTXT.color = textColor;
            }
            alphabetTXT.text = alphabet;

            IsEmpty = false;
        }



        // Fields
        private FlowerNormalParam param = null;

        // Functions
        private void clear()
        {
            IsEmpty = true;

            flowSkins.ForEach(f => f.SetActive(false));
            alphabetTXT.text = "";
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] flowSkins = null;
        [SerializeField] private TextMeshProUGUI alphabetTXT = null;

        // Unity Messages
        private void Awake()
        {
            clear();
        }
        private void Start()
        {
        }


    }
}