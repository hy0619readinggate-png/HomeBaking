using beyondi.Behaviour;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A12
{
    public class Example : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler,
        IBYDPooledObject<Example>
    {
        // Properties
        public static Example CurrentDrag { get; private set; } = null;

        // Properties
        public string Word => exam.Word;
        public AudioClip WordCLIP => exam.WordCLIP;

        // Methods
        public void Setup(ExampleData exam, float pickupScale, ExampleMGR exampleMGR)
        {
            //LOG.Info($"Setup()", this);

            this.exam = exam;
            this.exampleMGR = exampleMGR;
            this.pickupScale = pickupScale;
            wordIMG.sprite = exam.WordSPR;

            transform.localScale = originScale;
        }
        public void SetPosition(Transform parentTR, Transform dragTR, Vector3 startPos, Vector3 finishPos)
        {
            this.dragTR = dragTR;
            this.minX = startPos.x;
            this.maxX = finishPos.x;

            transform.position = startPos;
            transform.SetParent(parentTR);

            cg.blocksRaycasts = true;
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            AudioMGR.One.PlayEffect(wrongReturnCLIP);
        }
        public Coroutine ReturnToOrigin()
        {
            LOG.Info($"ReturnToOrigin()", this);

            return StartCoroutine(coReturnToOrigin());
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields`
        private ExampleData exam = null;
        private ExampleMGR exampleMGR = null;
        private Vector3 returnPosition;
        private Transform returnParentTR = null;
        private Transform dragTR = null;
        private float minX;
        private float maxX;
        private bool isDrag = false;
        private Vector2 originScale;
        private float pickupScale = 1f;

        // Functions
        private void pickUp()
        {
            AudioMGR.One.PlayEffect(pickupCLIP);

            returnPosition = transform.localPosition;
            returnParentTR = transform.parent;
            transform.SetParent(dragTR);

            cg.blocksRaycasts = false;

            rt.localScale = Vector3.one * pickupScale;
        }
        private void returnToOrigin()
        {
            transform.SetParent(returnParentTR);
            rt.DOLocalMove(returnPosition, returnDuration)
              .OnComplete(() =>
              {
                  cg.blocksRaycasts = true;
              });
            transform.DOScale(originScale, returnDuration);
        }
        private void locate(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                transform.position = ptWorld - relativePos;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image wordIMG = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pickupCLIP = null;
        [SerializeField] private AudioClip wrongReturnCLIP = null;
        [Header("★ Config")]
        [SerializeField] private Vector3 relativePos = new Vector3(0.6f, -0.6f, 0);
        [SerializeField] private float returnDuration = 0.3f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            originScale = transform.localScale;
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (isDrag) return;

            if (transform.position.x < minX ||
                transform.position.x > maxX)
                Pool.Release(this);
        }

        // Unity Coroutine
        IEnumerator coReturnToOrigin()
        {
            using (LOG.Coroutine($"coReturnToOrigin()", this))
            {
                returnToOrigin();
                yield return new WaitForSeconds(returnDuration);
            }
        }



        // Interface : IPointerDownHandler, IPointerUpHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            AudioMGR.One.PlayNarration(exam.WordCLIP);

            pickUp();
            locate(eventData);
            ExampleMGR.One.Pause();
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp()", this);

            if (!isDrag)
            {
                returnToOrigin();
                ExampleMGR.One.Resume();
            }
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
            else exampleMGR.ExcludeExam(exam);

            ExampleMGR.One.Resume();

            if (CurrentDrag == this)
                CurrentDrag = null;
        }

        // Interface : IBYDPooledObject<T>
        public IObjectPool<Example> Pool { get; set; }
    }
}