using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C3_A09
{
    [RequireComponent(typeof(Animator))]
    public class Bread : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Properties
        public static Bread CurrentDrag { get; private set; } = null;
        public string Text => exam.Text;
        public bool IsIdle => isIdle;
        public bool IsAvailable => exam.ID > 0;

        // Methods
        public void Init(Transform dragTR)
        {
            LOG.Info($"Init()", this);

            this.dragTR = dragTR;

            returnPosition = dragTargetTR.localPosition;
            returnParentTR = dragTargetTR.parent;
            returnSiblingIndex = dragTargetTR.GetSiblingIndex();
        }
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup()", this);

            this.StopCoroutineSafe(ref crWrong);

            dragTargetTR.SetParent(returnParentTR);
            dragTargetTR.SetSiblingIndex(returnSiblingIndex);
            dragTargetTR.localPosition = returnPosition;
            dragTargetTR.localScale = Vector3.one;

            this.exam = exam;
            skins.SetActiveOnly(exam.SkinID - 1);
            examTXT.text = exam.Text;

            isIdle = false;
            anim.SetTrigger("Hidden");

            cg.blocksRaycasts = true;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            AudioMGR.One.PlayEffect(appearCLIP);

            isIdle = true;
            anim.SetTrigger("Show");
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            AudioMGR.One.PlayEffect(disappearCLIP);

            isIdle = false;
            anim.SetTrigger("Hide");
        }
        public void Correct(Transform parentTR)
        {
            LOG.Info($"Correct()", this);

            dragTargetTR.SetParent(parentTR);
            dragTargetTR.localPosition = Vector3.zero;

            AudioMGR.One.PlayEffect(dropCLIP);

            isIdle = false;
            anim.SetTrigger("Correct");

        }
        public Coroutine Wrong(Transform newParentTR)
        {
            LOG.Info($"Wrong()", this);

            crWrong = StartCoroutine(coWrong(newParentTR));
            return crWrong;
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            isIdle = true;
            anim.SetTrigger("Idle");
        }
        public void Hidden()
        {
            LOG.Info($"Hidden()", this);
            if (!IsIdle)
                return;

            isIdle = false;
            anim.SetTrigger("Hidden");
        }
        public void HideText()
        {
            LOG.Info($"HideText()", this);

            examTXT.text = "";
        }



        // Fields
        private ExampleData exam = null;
        private Vector3 returnPosition;
        private Transform returnParentTR = null;
        private int returnSiblingIndex = 0;
        private Transform dragTR = null;
        private bool isDrag = false;
        private bool isIdle = false;
        private Coroutine crWrong = null;

        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= dragTargetTR.GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Transform[] skins_ = null;
        private Transform[] skins => skins_ ??= skinTR.GetChildren().ToArray();

        // Functions
        private void pickUp()
        {
            AudioMGR.One.PlayEffect(pickupCLIP);

            dragTargetTR.SetParent(dragTR);

            cg.blocksRaycasts = false;
        }
        private void locate(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                dragTargetTR.position = ptWorld - relativePos;
        }
        private void returnToOrigin()
        {
            AudioMGR.One.PlayEffect(returnCLIP);

            dragTargetTR.SetParent(returnParentTR);
            dragTargetTR.SetSiblingIndex(returnSiblingIndex);
            rt.DOLocalMove(returnPosition, returnDuration)
              .OnComplete(() =>
              {
                  cg.blocksRaycasts = true;
              });
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI examTXT = null;
        [SerializeField] private Transform dragTargetTR = null;
        [SerializeField] private Transform skinTR = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip appearCLIP = null;
        [SerializeField] private AudioClip pickupCLIP = null;
        [SerializeField] private AudioClip returnCLIP = null;
        [SerializeField] private AudioClip dropCLIP = null;
        [SerializeField] private AudioClip disappearCLIP = null;
        [Header("★ Config")]
        [SerializeField] private Vector3 relativePos = new Vector3(0.0f, -0.4f, 0);
        [SerializeField] private float returnDuration = 0.3f;

        // Unity Messages
        private void Awake()
        {
            anim.SetTrigger("Hidden");

            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coWrong(Transform newParentTR)
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                dragTargetTR.SetParent(newParentTR);
                dragTargetTR.localPosition = Vector3.zero;

                AudioMGR.One.PlayEffect(dropCLIP);
                yield return new WaitForSeconds(0.3f);

                anim.SetTrigger("Wrong");
                yield return new WaitForSeconds(0.3f);

                returnToOrigin();
                yield return new WaitForSeconds(returnDuration);
            }
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