using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng
{
    [RequireComponent(typeof(Button))]
    public class ButtonPlayAudio : MonoBehaviour
    {
        // Fields : caching
        private Button btn => GetComponent<Button>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AudioClip audioClip = null;

        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(() => AudioMGR.One.PlayEffect(audioClip));
        }
        private void Start()
        {

        }
    }
}