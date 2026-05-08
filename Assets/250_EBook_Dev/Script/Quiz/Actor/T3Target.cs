using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class T3Target : MonoBehaviour,
        IID,
        IDropHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Properties
        public bool IsAnswer { get; private set; }
        public bool IsSubmit { get; private set; }
        public int SubmitID => sequenceExam?.Sequence ?? 0;

        // Methods
        public void Setup(AudioClip clip)
        {
            LOG.Info($"Setup()", this);

            IsAnswer = false;
            IsSubmit = false;

            storyCLIP = clip;

            examIMG.gameObject.SetActive(false);
            correctGO.SetActive(false);
            wrongGO.SetActive(false);
            answerGO.SetActive(false);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public Coroutine PlayNarration()
        {
            return StartCoroutine(coNarration());
        }
        public void Feedback()
        {
            LOG.Info($"Feedback()", this);

            if (IsAnswer)
            {
                AudioMGR.One.PlayEffect(correctSFX);
                correctGO.SetActive(true);
            }
            else
            {
                AudioMGR.One.PlayEffect(wrongSFX);
                wrongGO.SetActive(true);
            }
        }
        public void ResetSubmit()
        {
            LOG.Function(this);

            clearExample();
        }
        public Coroutine ShowAnswer(T3Target from)
        {
            LOG.Info($"ShowAnswer() | {from.gameObject.name}", this);

            return StartCoroutine(coShowAnswer(from));
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private T3ExampleFloat sequenceFloat = null;
        private T3Example sequenceExam = null;
        private T3Example draggingExam = null;
        private AudioClip storyCLIP = null;
        private bool isDrag = false;

        // Functions
        private void setExample(T3Example example)
        {
            sequenceExam = example;

            examIMG.sprite = example.Sprite;
            examIMG.gameObject.SetActive(true);
            IsSubmit = true;
            IsAnswer = ID == example.Sequence;
        }
        private void clearExample()
        {
            sequenceExam = null;
            examIMG.gameObject.SetActive(false);
            IsSubmit = false;
            IsAnswer = false;

            correctGO.SetActive(false);
            wrongGO.SetActive(false);
            answerGO.SetActive(false);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private T3FloatMGR floatMGR = null;
        [SerializeField] private Image examIMG = null;
        [SerializeField] private GameObject selectedGO = null;
        [SerializeField] private GameObject correctGO = null;
        [SerializeField] private GameObject wrongGO = null;
        [SerializeField] private GameObject answerGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctSFX = null;
        [SerializeField] private AudioClip wrongSFX = null;
        [SerializeField] private AudioClip answerSFX = null;
        [SerializeField] private AudioClip dropSFX = null;
        [Header("★ Config")]
        [SerializeField] private float showAnswerMoveDuration = 0.7f;
        [SerializeField] private float showAnswerMoveDelay = 0.5f;

        // Unity Messages
        private void Awake()
        {
            sequenceFloat = floatMGR.Get();

            cg.blocksRaycasts = true;

            selectedGO.SetActive(false);

            examIMG.gameObject.SetActive(false);
            correctGO.SetActive(false);
            wrongGO.SetActive(false);
            answerGO.SetActive(false);
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coNarration()
        {
            selectedGO.SetActive(true);
            yield return AudioMGR.One.PlayNarrationAndWait(storyCLIP);

            selectedGO.SetActive(false);
        }
        IEnumerator coShowAnswer(T3Target from)
        {
            wrongGO.SetActive(false);

            var exam = from.sequenceExam;
            from.clearExample();

            yield return sequenceFloat.Move(
                exam.Sprite,
                from.examIMG.transform.position,
                examIMG.transform.position,
                showAnswerMoveDuration);
            setExample(exam);

            yield return new WaitForSeconds(showAnswerMoveDelay);
            yield return null;

            AudioMGR.One.PlayEffect(answerSFX);
            answerGO.SetActive(true);
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            T3Example exam = null;

            // Example 드래그시
            var example = eventData.pointerDrag.GetComponent<T3Example>();
            if (example != null)
                exam = example;

            // Target 드래그시
            var target = eventData.pointerDrag.GetComponent<T3Target>();
            if (target != null)
                exam = target.draggingExam;

            if (exam != null && sequenceExam == null)
            {
                AudioMGR.One.PlayEffect(dropSFX);

                setExample(exam);
                eventData.Use();
            }
        }

        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            if (sequenceExam != null)
            {
                draggingExam = sequenceExam;
                clearExample();

                sequenceFloat.Pickup(draggingExam, eventData);
                isDrag = true;
            }
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (isDrag)
                sequenceFloat.Locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            if (isDrag)
            {
                var drop = eventData.used;
                if (drop)
                    sequenceFloat.Drop();
                else sequenceFloat.ReturnTo();

                draggingExam = null;
            }
        }

        // Interface : IID
        public int ID { get; set; }
    }
}