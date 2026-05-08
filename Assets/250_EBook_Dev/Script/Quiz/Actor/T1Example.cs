using beyondi.Coroutine;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class T1Example : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Properties
        public bool IsAnswer { get; private set; }
        public Sprite Sprite { get; private set; }

        // Methods
        public void Setup(Sprite sprite, bool isAnswer)
        {
            LOG.Info($"Setup() | {isAnswer}", this);

            IsAnswer = isAnswer;
            Sprite = sprite;

            examIMG.sprite = sprite;
            examIMG.gameObject.SetActive(true);
            answerGO.SetActive(false);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            enableInteraction = enable;
            doEnableInteraction();
        }
        public void ShowAnswer()
        {
            LOG.Info($"ShowAnswer()", this);

            AudioMGR.One.PlayEffect(answerSFX);
            answerGO.SetActive(true);
        }
        public void Return()
        {
            LOG.Info($"Return()", this);

            examIMG.gameObject.SetActive(true);
            doEnableInteraction();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private bool enableInteraction = false;

        // Functions
        private void doEnableInteraction()
        {
            cg.blocksRaycasts = enableInteraction && examIMG.gameObject.activeSelf;
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image examIMG = null;
        [SerializeField] private GameObject answerGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip answerSFX = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            answerGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            T1ExampleFloat.One.Pickup(this, eventData);

            examIMG.gameObject.SetActive(false);
            doEnableInteraction();
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            T1ExampleFloat.One.Locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            var drop = eventData.used;
            if (drop)
                T1ExampleFloat.One.Drop();
            else T1ExampleFloat.One.ReturnTo();
        }
    }
}