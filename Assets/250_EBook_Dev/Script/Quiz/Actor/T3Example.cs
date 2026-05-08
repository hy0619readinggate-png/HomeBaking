using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class T3Example : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler,
        IPointerUpHandler,
        IPointerDownHandler
    {
        // Properties
        public int Sequence { get; private set; }
        public Sprite Sprite { get; private set; }
        public bool IsReady => examIMG.gameObject.activeSelf;

        // Methods
        public void Setup(Sprite sprite, AudioClip clip, int sequence)
        {
            LOG.Function(this);

            Sequence = sequence;
            Sprite = sprite;
            examCLIP = clip;

            examIMG.sprite = sprite;
            examIMG.gameObject.SetActive(true);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Function(this, $"{enable}");

            enableInteraction = enable;
            doEnableInteraction();
        }
        public void Return()
        {
            LOG.Function(this);

            examIMG.gameObject.SetActive(true);
            doEnableInteraction();
        }
        public void ResetExample()
        {
            LOG.Function(this);

            examIMG.gameObject.SetActive(true);
            doEnableInteraction();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private T3ExampleFloat sequenceFloat = null;
        private AudioClip examCLIP = null;
        private bool enableInteraction = false;
        private bool isDrag = false;

        // Functions
        private void doEnableInteraction()
        {
            cg.blocksRaycasts = enableInteraction && examIMG.gameObject.activeSelf;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private T3FloatMGR floatMGR = null;
        [SerializeField] private Image examIMG = null;

        // Unity Messages
        private void Awake()
        {
            sequenceFloat = floatMGR.Get();
            cg.blocksRaycasts = false;
        }
        private void Start()
        {

        }



        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            isDrag = true;

            sequenceFloat.Pickup(this, eventData);

            examIMG.gameObject.SetActive(false);
            doEnableInteraction();

            AudioMGR.One.StopNarration(); // 클릭 나레이션 중단
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (isDrag)
                sequenceFloat.Locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            var drop = eventData.used;
            if (drop)
                sequenceFloat.Drop();
            else sequenceFloat.ReturnTo();

            isDrag = false;
        }

        // IPointerUpHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Function(this);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Function(this);

            if (!isDrag)
                AudioMGR.One.PlayNarration(examCLIP);
        }
    }
}