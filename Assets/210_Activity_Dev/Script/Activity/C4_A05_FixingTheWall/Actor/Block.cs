using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;

namespace DoDoEng.Activity.C4_A05
{
    public class Block : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IBYDPooledObject<Block>
    {
        // Properties
        public static Block CurrentDrag { get; private set; } = null;
        public bool IsComplete { get; set; } = false;
        public string Text => chunkTXT.text;
        public GameObject AffordanceGO => affordanceGO;

        // Methods
        public void Setup(string text)
        {
            LOG.Info($"Setup() | {text}", this);

            IsComplete = false;
            chunkTXT.text = text;
        }
        public void SetOriginPosition()
        {
            LOG.Info($"SetOriginPosition()", this);

            originPosition = transform.position;
        }
        public void SetPositionTo(Vector3 pos)
        {
            LOG.Info($"SetPositionTo() | {pos}", this);

            transform.position = pos;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            anim.SetTrigger("Show");
            shadowGO.SetActive(true);
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            anim.SetTrigger("Idle");
            cg.blocksRaycasts = true;
        }
        public void Return()
        {
            LOG.Info($"Return()", this);

            anim.SetTrigger("Idle");
            returnToOrigin();
        }
        public void WrongReturn()
        {
            LOG.Info($"WrongReturn()", this);

            crWrongReturn = StartCoroutine(coWrongReturn());
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            this.StopCoroutineSafe(ref crWrongReturn);

            anim.SetTrigger("Hide");
            cg.blocksRaycasts = false;
        }

        // Event
        public event Action<Block> onDropWrong;



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private Vector3 originPosition;
        private Coroutine crWrongReturn = null;

        // Functions
        private void returnToOrigin()
        {
            transform.DOJump(originPosition, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() =>
                {
                    cg.blocksRaycasts = true;
                    shadowGO.SetActive(true);
                });
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI chunkTXT = null;
        [SerializeField] private AudioClip dragCLIP = null;
        [SerializeField] private GameObject shadowGO = null;
        [SerializeField] private GameObject affordanceGO = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coWrongReturn()
        {
            using (LOG.Coroutine($"coWrongReturn()", this))
            {
                anim.SetTrigger("Wrong");
                yield return new WaitForSeconds(1f);

                returnToOrigin();
            }
        }



        // Interface : IBeginDragHandler, IDragHandler, IEndDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            cg.blocksRaycasts = false;
            shadowGO.SetActive(false);
            transform.SetAsLastSibling();
            AudioMGR.One.PlayEffect(dragCLIP);

            CurrentDrag = this;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //LOG.Info($"OnDrag()", this);

            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            var drop = eventData.used;
            if (drop)
            {
                if (IsComplete)
                    anim.SetTrigger("Hide");
                else onDropWrong?.Invoke(this);
            }
            else returnToOrigin();

            if (CurrentDrag == this)
                CurrentDrag = null;
        }

        // Interface : IBYDPooledObject<T>
        public IObjectPool<Block> Pool { get; set; }
    }
}