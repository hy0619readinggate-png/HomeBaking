using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A06
{
    [RequireComponent(typeof(Animator))]
    public class Balloon : MonoBehaviour, IPointerDownHandler
    {
        // Methods
        public void Setup(ExampleData examData)
        {
            LOG.Info($"Setup() | {examData}", this);

            clip = examData.PhoneticCLIP;
            labelTXT.text = examData.Text;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            anim.SetTrigger("Show");
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            anim.SetTrigger("Hide");
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Function(this, $"{enable}");

            cg.blocksRaycasts = enable;
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private AudioClip clip = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI labelTXT = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }



        // IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            AudioMGR.One.PlayNarration(clip);
        }
    }
}