using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(Button))]
    public class SimplePopupButton : MonoBehaviour
    {
        // Event Handlers
        private void onButtonClicked()
        {
            if (result != SimplePopupResult.NA)
            {
                AudioMGR.One.PlayEffectUI(sfxMoment);

                var popup = GetComponentInParent<PopupBase<SimplePopupResult>>();
                LOG.Assert(
                    popup != null,
                    $"SimplePopupButton must be in SimplePopup", this);
                popup.CloseWithResult(result);
            }
        }



        // Unity Inspectors
        [Header("Config")]
        [SerializeField] private SimplePopupResult result = SimplePopupResult.NA;
        [SerializeField] private SfxMoment sfxMoment = SfxMoment.Common_Click;

        // Unity Messages
        private void Awake()
        {
            var btn = GetComponent<Button>();
            btn.onClick.AddListener(onButtonClicked);
        }
        private void Start()
        {
        }
    }
}