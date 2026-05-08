using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng
{
    [RequireComponent(typeof(Button))]
    public class PlaySfxUIButton : MonoBehaviour
    {
        // Fields : caching
        private Button btn => GetComponent<Button>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SfxMoment sfxMoment = SfxMoment.Common_Click;

        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(() => AudioMGR.One.PlayEffectUI (sfxMoment));
        }
        private void Start()
        {

        }
    }
}