using DG.Tweening;
using DoDoEng.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A07
{
    public class Luggage : MonoBehaviour,
        IPointerDownHandler,
        IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        // Properties
        public bool IsAnswer { get; private set; } = false;
        public string Word { get; private set; }
        public Sprite Sprite { get; private set; }
        public int CaseType { get; private set; }

        // Methods
        public void Setup(ExampleData exam, int caseType)
        {
            LOG.Info($"Setup() | {exam} {caseType}", this);

            gameObject.SetActive(true);
            cg.blocksRaycasts = false;
            cg.alpha = 1;

            image.sprite = exam.WordSPR;
            for (var i = 0; i < cases.Length; i++)
                cases[i].SetActive(caseType - 1 == i);

            IsAnswer = exam.IsAnswer;
            Word = exam.Word;
            Sprite = image.sprite;
            CaseType = caseType;

            this.exam = exam;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Return()
        {
            LOG.Info($"Return()", this);

            returnLuggaeFloat(false);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= rt.AddComponent<CanvasGroup>();

        // Fields
        private ExampleData exam = null;
        private Vector3 originPosition;

        // Functions
        private void returnLuggaeFloat(bool enable)
        {
            floatLuggage.FloatRT.DOLocalJump(originPosition, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() =>
                {
                    floatLuggage.gameObject.SetActive(false);
                    if (enable)
                        cg.blocksRaycasts = true;
                    cg.alpha = 1;
                });
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image image = null;
        [SerializeField] private GameObject[] cases = null;
        [SerializeField] private FloatLuggage floatLuggage = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip dragCLIP = null;
        [SerializeField] private AudioClip dragWrongCLIP = null;
        [SerializeField] private AudioClip incorrectCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            originPosition = transform.localPosition;
        }
        private void Start()
        {
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
            LOG.Info($"OnBeginDrag() | {Word}", this);

            cg.blocksRaycasts = false;
            cg.alpha = 0;

            floatLuggage.Pickup(exam, CaseType, eventData);

            AudioMGR.One.PlayEffect(dragCLIP);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            floatLuggage.Locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            var drop = eventData.used;
            if (drop && IsAnswer)
            {
                floatLuggage.Drop();
                gameObject.SetActive(false);
            }
            else if (drop && !IsAnswer)
            {
                AudioMGR.One.PlayEffect(incorrectCLIP);
                returnLuggaeFloat(false);
            }
            else
            {
                AudioMGR.One.PlayEffect(dragWrongCLIP);
                returnLuggaeFloat(true);
            }
        }
    }
}