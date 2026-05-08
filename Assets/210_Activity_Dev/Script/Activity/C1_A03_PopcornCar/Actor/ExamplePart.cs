using beyondi.Coroutine;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A03
{
    public class ExamplePart : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISubmitable
    {
        // Definitions
        public ExampleData ExampleData { get; private set; }
        public bool IsAnswer => ExampleData.IsAnswer;


        // Methods
        public void Init(Transform tr, ExamplePartParam examplePartsParam)
        {
            LOG.Info($"Init()", this);

            floatTR = tr;
            param = examplePartsParam;
        }
        public void Setup(ExampleData exampleData, int pNO)
        {
            LOG.Info($"Setup()", this);

            ExampleData = exampleData;

            items.ForEach((i, item) =>
            {
                item.gameObject.SetActive(i == pNO - 1);
                item.GetComponentInChildren<TextMeshProUGUI>(true).text = ExampleData.Text;
            });
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;

            if (enable)
                isSubmit = false;
        }
        public void ReturnToOrizinNow()
        {
            LOG.Info($"ReturnToOrizinNow()", this);

            transform.SetParent(orizinParentTR);
            transform.localPosition = orizinLocalPos;
            sacleTw.Complete();
            rt.localScale = Vector3.one;
        }
        public void MoveTo(Transform transform, float scale, float duration)
        {
            LOG.Info($"MoveTo() | {transform}", this);

            rt.DOMove(transform.position, duration);
            rt.DOScale(scale, duration).SetEase(Ease.InOutBack);
        }


        // Fields : caching
        private Transform[] items_ = null;
        private Transform[] items => items_ ??= transform.GetChildren().ToArray();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();

        // Fields
        private Transform floatTR = null;
        private Tween moveTw = null;
        private Tween sacleTw = null;
        private bool isFirst = false;
        private Vector2 orizinPos = Vector2.zero;
        private Vector2 orizinLocalPos = Vector2.zero;
        private Transform orizinParentTR = null;
        private bool isSubmit = false;

        // Fields
        private ExamplePartParam param = null;

        // Functions
        private void returnTo(Vector2 returnTo)
        {
            moveTw = rt.DOJump(returnTo, param.returnJumpPower, 1, param.returnJumpDuration)
                .OnComplete(() => transform.SetParent(orizinParentTR));
        }
        private void cancelReturn()
        {
            moveTw.Complete(true);
        }
        private void locate(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                transform.position = ptWorld;
        }



        // Unity Messages
        private void Awake()
        {
            LOG.Info($"Awake()", this);

            cg.blocksRaycasts = false;
            orizinParentTR = transform.parent.transform;
            orizinLocalPos = transform.localPosition;

            isFirst = true;

            LOG.Info($"orizinPos {name}, {orizinPos}", this);
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            sacleTw = transform.DOScale(1.2f, 0.2f);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerExit2D() | {collision.gameObject.name}", this);

            sacleTw = transform.DOScale(1f, 0.2f);
        }



        // Interface : IBeginDragHandler, IDragHandler, IEndDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            cancelReturn();

            if (isFirst)
            {
                isFirst = false;
                orizinPos = transform.position;
            }

            AudioMGR.One.PlayEffect(param.pickupClip);

            cg.blocksRaycasts = false;

            transform.SetParent(floatTR, false);
            transform.localPosition = Vector2.zero;

            locate(eventData);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //LOG.Info($"OnDrag()", this);

            locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            if (eventData.used)
                isSubmit = true;

            if (!eventData.used || !ExampleData.IsAnswer)
            {
                AudioMGR.One.PlayEffect(param.returnCLIP);

                cg.blocksRaycasts = true;
                returnTo(orizinPos);
            }
        }

        // IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            AudioMGR.One.PlayNarration(ExampleData.PhoneticCLIP);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp()", this);

            // 드래그시에는 소리를 안 내려면, Down 위치를 저장하고, 이후 거리가 어느정도 내에 있을때만 소리를 내도록 구현
            //AudioMGR.One.PlayNarration(ExampleData.PhoneticCLIP);
        }



        // Interface : ISubmitable
        bool ISubmitable.IsSubmit => isSubmit;
    }
}