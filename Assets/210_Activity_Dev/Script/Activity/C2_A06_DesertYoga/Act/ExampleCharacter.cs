using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A06
{
    public class ExampleCharacter : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler,
        IPointerDownHandler
    {
        // Properties
        public String Word => exam.Word;
        public int CharacterID => character.CharacterID;

        // Methods
        public void Init(ExampleCharacterParam exampleCharacterParam)
        {
            LOG.Info($"Init()", this);

            param = exampleCharacterParam;
        }
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup() | {exam}", this);

            this.exam = exam;
            examIMG.sprite = exam.Image;

            rigGO.SetActive(true);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Idle()
        {
            LOG.Info($"Idle", this);

            character.Idle();
        }
        public Coroutine StartComeback()
        {
            LOG.Info($"StartComeback()", this);

            crComeback = StartCoroutine(coComeback());
            return crComeback;
        }
        public void FinishComeback()
        {
            LOG.Info($"FinishComeback()", this);

            this.StopCoroutineSafe(ref crComeback);

            rigGO.SetActive(true);
            comebackTL.time = comebackTL.duration;
            comebackTL.Evaluate();
            comebackTL.Stop();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Character character_ = null;
        private Character character => character_ ?? GetComponentInChildren<Character>(true);

        // Fields
        private ExampleData exam = null;
        private Coroutine crComeback = null;
        private ExampleCharacterParam param = null;

        // Functions
        private void returnToOrigin()
        {
            rt.DOJump(originTR.position, param.returnJumpPower, 1, param.returnJumpDuration)
                .OnComplete(() => cg.blocksRaycasts = true);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image examIMG = null;
        [SerializeField] private GameObject rigGO = null;
        [SerializeField] private Transform originTR = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector comebackTL = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coComeback()
        {
            using (LOG.Coroutine($"coComeback", this))
            {
                rigGO.SetActive(true);
                comebackTL.time = 0;
                comebackTL.Play();
                yield return new WaitForSeconds((float)comebackTL.duration);

                character.Idle();
            }
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            AudioMGR.One.PlayNarration(exam.WordCLIP);
        }

        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            transform.SetAsLastSibling();

            cg.blocksRaycasts = false;
            character.Drag();

            AudioMGR.One.PlayEffect(param.dragCLIP);
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
                AudioMGR.One.PlayEffect(param.returnCLIP);
                cg.blocksRaycasts = true;
                character.Idle();
            }
            else
            {
                transform.position = originTR.position;
                rigGO.SetActive(false);
            }
        }
    }
}