using DG.Tweening;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A05
{
    [RequireComponent(typeof(Animator))]
    public class Plate : MonoBehaviour,
        IPointerDownHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Definitions
        public enum OutDirection { L, R };

        // Properties
        public bool IsAnswer => exam?.IsAnswer ?? false;
        public bool IsAvaliable => isAvailable;
        public Transform AffPos => affPos;

        // Methods
        public void Init()
        {
            LOG.Info($"Init()", this);

            originPosition = transform.localPosition;
        }
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup() | {exam}", this);

            this.exam = exam;
            alphabetTXT.text = exam.Text;

            gameObject.SetActive(true);
            cg.blocksRaycasts = true;
            transform.localPosition = originPosition;
            isAvailable = true;
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            AudioMGR.One.PlayEffect(correctCLIP);
            gameObject.SetActive(false);
            isAvailable = false;
        }
        public void Wrong(Transform from, OutDirection dir)
        {
            LOG.Info($"Wrong() | {dir}", this);

            transform.position = from.position;
            transform.SetAsFirstSibling();

            anim.SetTrigger($"Out{dir}");
            AudioMGR.One.PlayEffect(wrongCLIP);
            isAvailable = false;
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private ExampleData exam = null;
        private Vector3 originPosition;
        private bool isAvailable = false;

        // Functions
        private void returnToOrigin()
        {
            rt.DOLocalJump(originPosition, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() => cg.blocksRaycasts = true);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI alphabetTXT = null;
        [SerializeField] private Transform affPos = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pickupCLIP = null;
        [SerializeField] private AudioClip returnCLIP = null;
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }



        // Interface : I, IBeginDragHandler, IEndDragHandler, IDragHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            AudioMGR.One.PlayNarration(exam.SoundCLIP);
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            cg.blocksRaycasts = false;
            transform.SetAsLastSibling();

            AudioMGR.One.PlayEffect(pickupCLIP);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            if (!eventData.used)
            {
                returnToOrigin();
                AudioMGR.One.PlayEffect(returnCLIP);
            }
        }


    }
}