using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class CoinInfoInPopup : MonoBehaviour
    {
        // Methods
        public void AddCoin(int coinCount)
        {
            LOG.Function(this);

            if (coinCount > 0)
            {
                var fx = Instantiate(getFX, fxRT);
                fx.gameObject.SetActive(true);
                getTMP.text = $"+{coinCount}";
            }
            textTMP.text = $"{coinCount + int.Parse(textTMP.text)}";
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text textTMP = null;
        [SerializeField] private TMP_Text getTMP = null;
        [SerializeField] private GameObject getFX = null;
        [SerializeField] private Transform fxRT = null;
        [SerializeField] private Button coinBT = null;

        // Unity Messages
        private void Awake()
        {
            getFX.gameObject.SetActive(false);
            textTMP.text = $"{LMS.One.Coin}";

            coinBT.onClick.AddListener(() => SystemUI.One.CoinHistoryPU.ShowPopup().Forget());
        }
    }
}