using UnityEngine;
using TMPro;

namespace DoDoEng.Game.C4_G02
{
    public class QuestionText : MonoBehaviour
    {

        // Methods
        public void Setup(bool hasCover, string text)
        {
            _ScreenGO.SetActive(hasCover);

            _TextTMP.text = text;
            _TextTMP.color = hasCover ? _TextHighlightColor : _TextNormalColor;
        }
        public void HideCover()
        {
            _ScreenGO.SetActive(false);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject _ScreenGO = null;
        [SerializeField] private TextMeshProUGUI _TextTMP = null;
        [Header("★ Config")]
        [SerializeField] private Color _TextNormalColor = Color.black;
        [SerializeField] private Color _TextHighlightColor = Color.red;
    }
}