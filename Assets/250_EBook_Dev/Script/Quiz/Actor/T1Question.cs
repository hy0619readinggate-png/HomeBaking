using beyondi.Util;
using DoDoEng.Activity.C1_A03;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class T1Question : MonoBehaviour,
        IDropHandler
    {
        // Properties
        public bool IsAnswer { get; private set; }
        public bool IsSubmit { get; private set; }

        // Methods
        public void Setup(string question, AudioClip clip)
        {
            LOG.Info($"Setup() | {question}", this);

            clearExample();

            questionClip = clip;
            questionTXT.text = question.Replace("\\n", "\n"); ;

            correctGO.SetActive(false);
            wrongGO.SetActive(false);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            listenBTN.interactable = enable;

            if (!enable)
                stopNarration();
        }
        public Coroutine PlayNarration()
        {
            return playNarration();
        }
        public void StopNarration()
        {
            stopNarration();
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



        // Fields
        private AudioClip questionClip = null;
        private Coroutine crNarration = null;

        // Functions
        private Coroutine playNarration()
        {
            if (crNarration != null)
                this.StopCoroutineSafe(ref crNarration);

            crNarration = StartCoroutine(coNarration());
            return crNarration;
        }
        private void stopNarration()
        {
            if (crNarration != null)
                this.StopCoroutineSafe(ref crNarration);

            AudioMGR.One.StopNarration();
            listenAniGO.SetActive(false);
        }
        private void setExample(T1Example example)
        {
            submitIMG.sprite = example.Sprite;
            submitIMG.gameObject.SetActive(true);
            IsSubmit = true;
            IsAnswer = example.IsAnswer;
        }
        private void clearExample()
        {
            submitIMG.gameObject.SetActive(false);
            IsSubmit = false;
            IsAnswer = false;
        }

        // Event Handlers
        private void listenBTN_OnClick()
        {
            LOG.Info($"listenBTN_OnClick()", this);

            playNarration();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image submitIMG = null;
        [SerializeField] private GameObject correctGO = null;
        [SerializeField] private GameObject wrongGO = null;
        [SerializeField] private TextMeshProUGUI questionTXT = null;
        [SerializeField] private Button listenBTN = null;
        [SerializeField] private GameObject listenAniGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctSFX = null;
        [SerializeField] private AudioClip wrongSFX = null;
        [SerializeField] private AudioClip dropSFX = null;

        // Unity Messages
        private void Awake()
        {
            correctGO.SetActive(false);
            wrongGO.SetActive(false);
            submitIMG.gameObject.SetActive(false);

            listenAniGO.SetActive(false);

            listenBTN.interactable = false;
            listenBTN.onClick.AddListener(listenBTN_OnClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coNarration()
        {
            listenAniGO.SetActive(true);
            yield return AudioMGR.One.PlayNarrationAndWait(questionClip);

            listenAniGO.SetActive(false);
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var exam = eventData.pointerDrag.GetComponent<T1Example>();
            if (exam != null)
            {
                AudioMGR.One.PlayEffect(dropSFX);

                setExample(exam);
                eventData.Use();
            }
        }

    }
}