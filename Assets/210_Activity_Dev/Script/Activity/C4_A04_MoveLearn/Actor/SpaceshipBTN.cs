using beyondi.Coroutine;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C4_A04
{
    [RequireComponent(typeof(Button))]
    public class SpaceshipBTN : MonoBehaviour, ISubmitable
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            isPressed = false;
            btn.interactable = enable;
        }



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private bool isPressed = false;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private AudioClip touchCLIP = null;

        // Unity Messages
        private void Awake()
        {
            btn.interactable = false;
            btn.onClick.AddListener(() =>
            {
                if (touchCLIP != null)
                    AudioMGR.One.PlayEffect(touchCLIP);
                isPressed = true;
            });
        }
        private void Start()
        {
        }



        // Interface : ISubmitable
        public bool IsSubmit => isPressed;
    }
}