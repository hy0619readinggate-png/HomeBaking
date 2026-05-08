using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C4_A05
{
    public class IntroCard : MonoBehaviour
    {
        // Methods
        public void Setup(IntroData data)
        {
            LOG.Function(this);

            wordTXT.text = data.Word;
            wordIMG.sprite = data.WordSPR;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI wordTXT = null;
        [SerializeField] private Image wordIMG = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}