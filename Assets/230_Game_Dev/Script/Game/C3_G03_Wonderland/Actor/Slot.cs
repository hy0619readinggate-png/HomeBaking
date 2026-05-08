using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C3_G03
{
    [RequireComponent(typeof(Collider2D))]
    public class Slot : MonoBehaviour, IDropHandler
    {
        // Properties
        public string AnswerChunk { get { return answerChunk; } set { answerChunk = value; } }
        // Methods
        public void SetSlot(bool isOn)
        {
            col.enabled = isOn;
        }
        public void CheckDrop() { checkDrop(); }
        // Events
        public event Action<bool> OnDrop;



        // Fields : caching
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();

        // Fields
        private bool isAvailable = false;
        private string answerChunk;
        // Functions
        private void checkDrop()
        {
            LOG.Important("CHECK DROP", this);
            if (upper != null)
            {
                if (upper.GetComponentInChildren<Block>() != null)
                {
                    if (GetComponentInChildren<Block>() == null)
                    {
                        var upperBlock = upper.GetComponentInChildren<Block>();
                        upperBlock.gameObject.SetActive(false);
                        upperBlock.transform.SetParent(transform);
                        upperBlock.transform.localPosition = new Vector3(0, 0, 0);
                        upperBlock.gameObject.SetActive(true);

                        upperBlock.Drop();
                        SetSlot(false);
                    }
                }
            }
            if (lower != null)
            {
                if (lower.GetComponentInChildren<Block>() == null)
                {
                    if (GetComponentInChildren<Block>() != null)
                    {
                        var Block = GetComponentInChildren<Block>();

                        Block.gameObject.SetActive(false);

                        Block.transform.SetParent(lower.transform);
                        Block.transform.localPosition = new Vector3(0, 0, 0);
                        Block.gameObject.SetActive(true);

                        Block.Drop();
                        SetSlot(false);
                    }
                }
            }
        }
        // Event Handlers
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (lower != null)
            {
                if (lower.GetComponentInChildren<Block>() != null)
                {
                    img.SetActive(true);
                    isAvailable = true;
                }
                else
                {
                    img.SetActive(false);
                    isAvailable = false;
                }
            }
            else
            {
                if (GetComponentInChildren<Block>() == null)
                {
                    img.SetActive(true);
                    isAvailable = true;
                }
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            img.SetActive(false);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Slot upper = null;
        [SerializeField] private Slot lower = null;
        [SerializeField] private GameObject img = null;

        // Unity Messages
        private void Awake()
        {
            col.enabled = false;
        }
        private void Start()
        {

        }
        private void Update()
        {
            if (GetComponentInChildren<Block>() != null) col.enabled = false;
            else col.enabled = true;
        }
        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);
            var block = eventData.pointerDrag.GetComponent<Block>();

            if (GetComponentInChildren<Block>() != null) isAvailable = false;

            if (isAvailable)
            {
                block.transform.SetParent(transform);
                block.transform.localPosition = new Vector3(0, 0, 0);

                col.enabled = false;
                block.IsDropped = true;
            }
            else
                block.IsDropped = false;

            OnDrop?.Invoke(block.IsDropped);
        }
    }
}