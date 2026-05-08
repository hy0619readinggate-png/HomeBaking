using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A09
{
    [RequireComponent(typeof(Animator))]
    public class ExampleStar : MonoBehaviour, IID,
        IPointerDownHandler,
        IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        // Properties
        public int ID { get; set; }
        public int ColorID { get; private set; }
        public ExampleData ExampleData { get; private set; }
        public bool IsAnswer => ExampleData.IsAnswer;

        // Methods
        public void Init(ExampleStarParam param)
        {
            LOG.Info($"Init()", this);

            this.param = param;
        }
        public void Setup(int colorID, ExampleData exam)
        {
            LOG.Info($"Setup() | {exam.Phonics}, {colorID}", this);

            ExampleData = exam;
            ColorID = colorID;

            children.ForEach((i, c) => c.gameObject.SetActive(i == colorID - 1));
            texts.ForEach(t => t.text = exam.Phonics);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Return()
        {
            LOG.Info($"Return()", this);

            returnToOrigin();
        }

        // Methods
        public void Appear()
        {
            LOG.Info($"Appear()", this);

            isAppeared = true;

            transform.localPosition = originPosition;

            anim.SetTrigger("Appear");
        }
        public void Disappear()
        {
            LOG.Info($"Disappear()", this);

            isAppeared = false;

            anim.SetTrigger("Disappear");
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            if (!isAppeared)
            {
                isAppeared = true;

                anim.SetTrigger("Idle");
            }
        }
        public void Hidden()
        {
            LOG.Info($"Hidden()", this);

            isAppeared = false;

            anim.SetTrigger("Hidden");
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private TextMeshProUGUI[] texts_ = null;
        private TextMeshProUGUI[] texts => texts_ ??= GetComponentsInChildren<TextMeshProUGUI>(true);

        // Fields
        private ExampleStarParam param = null;
        private bool isAppeared = false;
        private Vector2 originPosition = Vector2.zero;

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
            rt.DOLocalJump(originPosition, param.returnJumpPower, 1, param.returnJumpDuration)
                .OnComplete(() =>
                {
                    cg.blocksRaycasts = true;
                });
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform[] children = null;

        // Unity Messages
        private void Awake()
        {
            originPosition = transform.localPosition;

            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }



        // IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            transform.SetAsLastSibling();

            AudioMGR.One.PlayEffect(param.pickupClip);
            cg.blocksRaycasts = false;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            cg.blocksRaycasts = false;

            if (!eventData.used)
            {
                returnToOrigin();
                AudioMGR.One.PlayEffect(param.returnCLIP);
            }

        }

        // IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            AudioMGR.One.PlayNarration(ExampleData.PhoneticCLIP);
        }
    }
}