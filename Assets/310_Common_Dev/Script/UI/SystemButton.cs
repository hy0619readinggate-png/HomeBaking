using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(Button))]
    public class SystemButton : MonoBehaviour
    {
        // Unity Inspectors
        [Header("Config")]
        [SerializeField] private SystemButtonType type = SystemButtonType.None;
        [SerializeField] private SfxMoment sfxMoment = SfxMoment.System_Back;

        // Unity Messages
        private void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffectUI(sfxMoment);
                SystemEventManager.SystemButtonClick(type);
            });
        }
    }
}   