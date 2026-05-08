using DG.Tweening;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C4_A07
{
    [RequireComponent(typeof(Animator))]
    public class IceExample : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Properties
        public static IceExample CurrentDrag { get; private set; } = null;
        public bool IsAnswer => exam.IsAnswer;
        public string Text => exam.Text;

        // Methods
        public void Setup(ExampleData exam)
        {
            LOG.Function(this);

            this.exam = exam;
            chunkTXT.text = exam.Text;
        }
        public void Show(bool effect = false)
        {
            LOG.Function(this);

            if (effect)
                AudioMGR.One.PlayEffect(appearCLIP);
            anim.SetTrigger("Show");
        }
        public void Shown()
        {
            LOG.Function(this);

            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("AnswerPiece_Idle"))
                anim.SetTrigger("Idle");
        }
        public void Correct()
        {
            LOG.Function(this);

            anim.SetTrigger("Hidden");
        }
        public void Wrong()
        {
            LOG.Function(this);

            anim.SetTrigger("Hidden");
        }
        public void Respawn()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(appearCLIP);

            transform.position = returnPosition;
            anim.SetTrigger("Show");
            cg.blocksRaycasts = true;
        }
        public void Hide()
        {
            LOG.Function(this);

            anim.SetTrigger("Hide");
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private ExampleData exam = null;
        private bool isDrag = false;
        private Vector3 returnPosition;

        // Functions
        private void pickUp()
        {
            AudioMGR.One.PlayEffect(pickupCLIP);

            returnPosition = transform.position;

            anim.SetTrigger("Drag");

            cg.blocksRaycasts = false;
        }
        private void locate(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                transform.position = ptWorld - relativePos;
        }
        private void returnToOrigin()
        {
            rt.DOJump(returnPosition, returnJumpPower, 1, returnDuration)
                .OnComplete(() =>
                {
                    anim.SetTrigger("Reset");

                    cg.blocksRaycasts = true;

                });

        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI chunkTXT = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip appearCLIP = null;
        [SerializeField] private AudioClip pickupCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnDuration = 0.3f;
        [SerializeField] private Vector3 relativePos = new Vector3(0.0f, -0.4f, 0);

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }



        // Interface : IPointerDownHandler, IPointerUpHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            pickUp();
            locate(eventData);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp()", this);

            if (!isDrag)
                returnToOrigin();
        }

        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            isDrag = true;
            CurrentDrag = this;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            isDrag = false;
            if (!eventData.used)
                returnToOrigin();

            if (CurrentDrag == this)
                CurrentDrag = null;
        }
    }
}