using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Common
{
    public class MessagePopup : PopupBase<SimplePopupResult>
    {
        // Methods
        public async UniTask<SimplePopupResult> ShowPopupOK(string idMessage, string idOK = "BUTTON_3", bool warn = false, bool dim = true)
        {
            LOG.Function(this, $"{idMessage} | {idOK} | {warn}");

            messageLT.SetText(idMessage);
            okLT.SetText(idOK);
            warningIconGO.SetActive(warn);
            yesBTN.gameObject.SetActive(false);
            noBTN.gameObject.SetActive(false);
            okBTN.gameObject.SetActive(true);
            dimGO.SetActive(dim);

            return await showPopup();
        }
        public async UniTask<SimplePopupResult> ShowPopupYesNo(string idMessage, string idYes = "BUTTON_3", string idNo = "BUTTON_4", bool warn = false, bool dim = true)
        {
            LOG.Function(this, $"{idMessage} | {idYes} | {idNo} | {warn}");

            messageLT.SetText(idMessage);
            yesLT.SetText(idYes);
            noLT.SetText(idNo);
            warningIconGO.SetActive(warn);
            yesBTN.gameObject.SetActive(true);
            noBTN.gameObject.SetActive(true);
            okBTN.gameObject.SetActive(false);
            dimGO.SetActive(dim);

            return await showPopup();
        }



        // Unity Inspectors
        [SerializeField] private GameObject warningIconGO = null;
        [SerializeField] private TMP_Text messageTMP = null;
        [SerializeField] private LocalizationText messageLT = null;
        [SerializeField] private Button yesBTN = null;
        [SerializeField] private Button noBTN = null;
        [SerializeField] private Button okBTN = null;
        [SerializeField] private LocalizationText yesLT = null;
        [SerializeField] private LocalizationText noLT = null;
        [SerializeField] private LocalizationText okLT = null;
        [SerializeField] private GameObject dimGO = null;

    }
}