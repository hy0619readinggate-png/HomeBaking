using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class ErrorPopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(string message)
        {
            LOG.Function(this, $"{message}");

            if (message == "ChainOperation failed because dependent operation failed")
                errorTXT.text = LocalizationMGR.One.GetText("ERROR_1");
            else if (message == "Object reference not set to an instance of an object.")
                errorTXT.text = LocalizationMGR.One.GetText("ERROR_2");
            else if (message == "Failed to load child's info.")
                errorTXT.text = LocalizationMGR.One.GetText("ERROR_3");
            else
                errorTXT.text = message;
            //errorTXT.text = LocalizationMGR.One.GetText("POPUP_38") + $"\n({message})";
            return await showPopup();
        }



        // Unity Inspectors
        [SerializeField] private Text errorTXT = null;
    }
}