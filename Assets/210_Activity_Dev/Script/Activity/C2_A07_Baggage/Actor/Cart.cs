using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A07
{
    public class Cart : MonoBehaviour, IDropHandler
    {
        // Properties
        public bool IsFilled { get; private set; } = false;
        public Luggage DroppedLuggage { get; private set; }
        public bool IsEmpty => !IsFilled && gameObject.activeSelf;
        public Transform AffPos => affPos;
        // Methods
        public void Setup()
        {
            LOG.Info($"Setup()", this);

            cg.blocksRaycasts = false;
            luggage.gameObject.SetActive(false);
            wordTMP.text = "";
            effect.SetActive(false);
            IsFilled = false;

            correctStar.SetActive(false);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }

        // Events
        public event Action<Cart> OnSubmit;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject luggage = null;
        [SerializeField] private GameObject[] cases = null;
        [SerializeField] private Image image = null;
        [SerializeField] private TextMeshProUGUI wordTMP = null;
        [SerializeField] private GameObject effect = null;
        [SerializeField] private GameObject correctStar = null;
        [SerializeField] private Transform affPos = null;

        // Unity Messages
        private void Awake()
        {
            Setup();
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            if (!IsFilled)
                effect.SetActive(true);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerExit2D() | {collision.gameObject.name}", this);

            effect.SetActive(false);
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var luggage = eventData.pointerDrag.GetComponent<Luggage>();
            if (luggage != null && !IsFilled)
            {
                if (luggage.IsAnswer)
                {
                    effect.SetActive(false);

                    for (var i = 0; i < cases.Length; i++)
                        cases[i].SetActive(luggage.CaseType - 1 == i);
                    image.sprite = luggage.Sprite;
                    this.luggage.gameObject.SetActive(true);

                    wordTMP.text = luggage.Word;

                    correctStar.SetActive(true);

                    IsFilled = true;

                    cg.blocksRaycasts = false;


                    LOG.Info($"OnDrop() | {luggage.Word}", this);
                }

                DroppedLuggage = luggage;

                eventData.Use();

                OnSubmit?.Invoke(this);
            }
        }
    }
}