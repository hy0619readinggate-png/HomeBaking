using beyondi.Coroutine;
using DG.Tweening;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A04
{
    [RequireComponent(typeof(Animator))]
    public class StingrayExam : MonoBehaviour, ISubmitable,
        IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        // Properties
        public bool IsAnswer { get; private set; }
        public string Word { get; private set; }
        public static StingrayExam CurrentDrag { get; private set; } = null;

        // Methods
        public void Setup(ExampleData exam)
        {
            IsAnswer = exam.IsAnswer;
            Word = exam.Word;
            wordCLIP = exam.WordCLIP;

            isSubmit = false;
            cg.blocksRaycasts = true;

            text.text = Word;
            anim.SetTrigger("Hidden");
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable && !isSubmit;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            anim.SetTrigger("Show");
            AudioMGR.One.PlayEffect(appearCLIP);
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            anim.SetTrigger("Idle");
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            if (!isSubmit)
                anim.SetTrigger("Hide");
        }
        public void Hidden()
        {
            LOG.Info($"Hidden()", this);

            anim.SetTrigger("Hidden");
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private AudioClip wordCLIP;
        private Vector3 originPosition;
        private bool isSubmit;

        // Functions
        private void ReturnStingray()
        {
            stingrayDrag.RT.DOLocalJump(originPosition, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() =>
                {
                    stingrayDrag.Hide();

                    cg.alpha = 1;
                    EnableInteraction(true);
                });
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private AudioClip appearCLIP;
        [SerializeField] private StingrayDrag stingrayDrag;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            originPosition = transform.localPosition;
        }
        private void Start()
        {
        }



        // Interface : ISubmitable
        public bool IsSubmit => isSubmit;

        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            AudioMGR.One.PlayEffect(wordCLIP);
        }

        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            cg.alpha = 0;
            EnableInteraction(false);

            stingrayDrag.Drag(Word, eventData);

            CurrentDrag = this;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //LOG.Info($"OnDrag()", this);

            stingrayDrag.Locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            var drop = eventData.used;
            if (drop)
            {
                stingrayDrag.Hide();
                Hidden();
                cg.alpha = 1;

                isSubmit = true;
            }
            else ReturnStingray();

            if (CurrentDrag == this)
                CurrentDrag = null;
        }
    }
}