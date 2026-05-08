using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C4_G03
{
    public class Block : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Properties
        public bool IsDropped { get { return isDropped; } set { isDropped = value; } }
        public bool IsCorrect { get { return isCorrect; } set { isCorrect = value; } }
        public string Word => word;
        public Transform StartParent => startParent;
        // Methods
        public void Setup(string chunk, SentenceData sd) 
        {
            LOG.Info($"Setup() | {chunk}", this);

            word = chunk;

            if (sd != null)
            {
                var typeIdx = sd.BlockType * 3 + UnityEngine.Random.Range(0, 3); //타입별로 3개씩, 총 9개

                blockType[typeIdx].SetActive(true);
                blockChunk = blockTextBox.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (TextMeshProUGUI txt in blockChunk) txt.text = chunk;
            }
            else
                foreach (TextMeshProUGUI txt in blockChunk) txt.text = string.Empty;
        }
        public void Appear(Transform appearTR)
        {
            var block = Instantiate(this, appearTR);
            block.gameObject.SetActive(true);
        }
        public void Intro()
        {
            anim.SetTrigger(hashKeyIntro);
            enabled = false;
        }
        public void Drop()
        {
            LOG.Info($"Drop()", this);
            anim.SetTrigger(hashKeyDrop);
        }
        public void Glow()
        {
            LOG.Info($"Glow()", this);
            anim.SetTrigger(hashKeyCorrect);
        }
        public void Erase()
        {
            foreach (TextMeshProUGUI txt in blockChunk) txt.text = string.Empty;
        }
        public void InitialGlow()
        {
            anim.SetTrigger(hashKeyCorrectIdle);
        }
        public void HideText()
        {
            anim.SetTrigger(hashKeyCorrectHide);
        }
        public void ResetBlockPosition()
        {
            resetBlockPosition();
        }


        // Events
        public event Action<int> OnBlockReturned;


        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();



        // Fields
        private TextMeshProUGUI[] blockChunk;
        private Transform startParent;

        private bool isDropped = false;
        private bool isCorrect = false;
        private string word;


        // Fields : Anim
        private readonly int hashKeyIntro = Animator.StringToHash("Intro");
        private readonly int hashKeyCorrect = Animator.StringToHash("Correct");
        private readonly int hashKeyCorrectIdle = Animator.StringToHash("CorrectIdle");
        private readonly int hashKeyCorrectHide = Animator.StringToHash("TextHide");
        private readonly int hashKeyGrab = Animator.StringToHash("Grab");
        private readonly int hashKeyDrop = Animator.StringToHash("Drop");

        // Functions
        private void resetBlockPosition()
        {
            transform.SetParent(startParent);
            transform.localPosition = Vector3.zero;
            anim.SetTrigger(hashKeyDrop);

            startParent = null;
            anim.transform.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject blockTextBox = null;
        [SerializeField] private Animator anim = null;
        [SerializeField] private GameObject[] blockType = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip dragSFX = null;
        [SerializeField] private AudioClip dropSFX = null;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }

        // Unity Coroutine
        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            anim.SetTrigger(hashKeyGrab);
            AudioMGR.One.PlayEffect(dragSFX);

            isDropped = false;
            startParent = transform.parent;

            var cam = eventData.pressEventCamera;
            var pos = eventData.position;
            var currParent = transform.parent;

            transform.SetParent(currParent.parent);
            anim.transform.GetComponentInChildren<CanvasGroup>().blocksRaycasts = false;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
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
            StartCoroutine(crEndDrag());
        }

        // Coroutines
        IEnumerator crEndDrag()
        {
            anim.SetTrigger(hashKeyDrop);
            AudioMGR.One.PlayEffect(dropSFX);
            yield return new WaitForEndOfFrame();

            anim.transform.GetComponentInChildren<CanvasGroup>().blocksRaycasts = true;
            yield return null;

            if (!isDropped)
            {
                if (isCorrect)
                {
                    Glow();
                    OnBlockReturned?.Invoke(StartParent.GetComponent<Slot>().AnswerSlotIdx);
                }
                transform.SetParent(startParent);
                transform.localPosition = Vector3.zero;
            }
            startParent = null;
        }
    }
}