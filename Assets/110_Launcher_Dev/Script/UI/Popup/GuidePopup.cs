using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DoDoEng
{
    public class GuidePopup : MonoBehaviour
    {
        // Methods
        public void ShowPopup(bool first)
        {
            buttonTMP.text = LocalizationMGR.One.GetText(first ? "BUTTON_29" : "BUTTON_30");
            gameObject.SetActive(true);
        }

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text buttonTMP;
        [SerializeField] private Button closeBTN;

        // Unity Messages


        private void Awake()
        {
            closeBTN.onClick.AddListener(() =>
            {
                if (UserData.One.Child.HasSignedIn)
                    UserData.One.Child.IsGuideFirst = false;
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                gameObject.SetActive(false);
            });
        }
        private void Start()
        {
        }
    }
}