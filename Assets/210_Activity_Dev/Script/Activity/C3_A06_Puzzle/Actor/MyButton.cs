using beyondi.Coroutine;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C3_A06
{
    [RequireComponent(typeof(Button))]
    public class MyButton : MonoBehaviour, ISubmitable
    {
        // Properties
        public bool Enabled => cg.blocksRaycasts;
        public bool IsActive => btn.interactable;

        // Methods
        public void EnableInteraction(bool enable)
        {
            isPressed = false;

            cg.blocksRaycasts = enable;
        }
        public void Activate(bool isActive)
        {
            LOG.Info($"Activate() | {isActive}", this);

            btn.interactable = isActive;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private bool isPressed = false;



        // Unity Inspectors
        [Header("¡Ú Sound")]
        [SerializeField] private AudioClip clickCLIP = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            btn.onClick.AddListener(() =>
            {
                if (clickCLIP != null)
                    AudioMGR.One.PlayEffect(clickCLIP);
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