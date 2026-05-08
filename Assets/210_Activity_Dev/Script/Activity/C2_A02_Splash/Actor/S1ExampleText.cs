using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A02
{
    [RequireComponent(typeof(Animator))]
    public class S1ExampleText : MonoBehaviour,
        IPointerDownHandler,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IID
    {
        // Properties
        public string Text => exam.Text;

        // Methods
        public void Init(Transform floatTR)
        {
            LOG.Info($"Init()", this);

            this.floatTR = floatTR;
        }
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup() | {exam.Text}", this);

            this.exam = exam;

            cg.alpha = 1f;
            cg.blocksRaycasts = true;

            anim.SetTrigger("Idle");

            examTXT.text = exam.Text;
        }
        public void ReturnAndHide()
        {
            LOG.Info($"ReturnAndHide()", this);

            cg.alpha = 0f;

            transform.SetParent(orizinParentTR);
            transform.position = originPosition;
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            anim.SetTrigger("Wrong");

            DOVirtual.DelayedCall(pongdangDelay, () => AudioMGR.One.PlayEffect(pongdangCLIP));

            vfxWrongGO.SetActive(false);
            vfxWrongGO.SetActive(true);
        }
        public void Respawn()
        {
            LOG.Info($"Respawn()", this);

            cg.blocksRaycasts = true;

            anim.SetTrigger("Idle");

            transform.SetParent(orizinParentTR);
            transform.position = originPosition;
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
        private Transform floatTR = null;
        private Vector2 originPosition = Vector2.zero;
        private Transform orizinParentTR = null;

        // Functions
        private void locate(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                transform.position = ptWorld;
        }
        private void returnToOrigin()
        {
            rt.DOJump(originPosition, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() =>
                {
                    cg.blocksRaycasts = true;
                    transform.SetParent(orizinParentTR);
                });
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI examTXT = null;
        [SerializeField] private GameObject vfxWrongGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pickupCLIP = null;
        [SerializeField] private AudioClip returnCLIP = null;
        [SerializeField] private AudioClip pongdangCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;
        [SerializeField] private float pongdangDelay = 0.2f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            vfxWrongGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            AudioMGR.One.PlayEffectLL(exam.PhoniceCLIP);
        }

        // Interface : IBeginDragHandler, IDragHandler, IEndDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            orizinParentTR = transform.parent.transform;
            originPosition = transform.position;
            cg.blocksRaycasts = false;

            transform.SetParent(floatTR);
            AudioMGR.One.PlayEffect(pickupCLIP);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            cg.blocksRaycasts = false;

            if (!eventData.used)
            {
                returnToOrigin();
                AudioMGR.One.PlayEffect(returnCLIP);
            }
        }

        // Interface : IID
        public int ID { get; set; }
    }
}