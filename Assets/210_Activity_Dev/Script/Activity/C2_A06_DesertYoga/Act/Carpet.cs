using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A06
{
    public class Carpet : MonoBehaviour,
        IDropHandler, IPointerDownHandler,
        ISubmitable, IID
    {
        // Properties
        public int ID { get; set; }
        public bool IsSubmit { get; private set; }
        public int CharacterID => submitExample.CharacterID;
        public bool IsCorrect => submitExample?.Word == exam.Word;
        public string Word => exam.Word;

        // Methods 
        public void Setup(ExampleData exam, int pose)
        {
            LOG.Info($"Setup() | {exam}, {pose}", this);

            this.exam = exam;

            foreach (var c in characters)
            {
                c.Setup(pose);
                c.gameObject.SetActive(false);
            }
            examTXT.gameObject.SetActive(true);
            examTXT.text = exam.Word;
            examTXT.gameObject.SetActive(true);

            answerIMG.sprite = exam.Image;
            answerTXT.text = exam.Word;

            IsSubmit = false;
            submitExample = null;

            glowGO.SetActive(false);
        }
        public Coroutine StartCorrect()
        {
            LOG.Info($"StartCorrect()", this);

            glowGO.SetActive(false);

            crFeedback = StartCoroutine(coCorrect());
            return crFeedback;

        }
        public void FinishCorrect()
        {
            LOG.Info($"FinishCorrect()", this);

            this.StopCoroutineSafe(ref crFeedback);

            var currentCharacter = characters.Single(c => c.gameObject.activeSelf);
            currentCharacter.FinishCorrect();

            correctInTL.Stop();

            AudioMGR.One.StopNarration();

            correctOutTL.time = correctOutTL.duration;
            correctOutTL.Evaluate();
            correctOutTL.Stop();
        }
        public Coroutine StartWrong()
        {
            LOG.Info($"StartCorrectStartWrong()", this);

            glowGO.SetActive(false);

            crFeedback = StartCoroutine(coWrong());
            return crFeedback;

        }
        public void FinishWrong()
        {
            LOG.Info($"FinishWrong()", this);

            this.StopCoroutineSafe(ref crFeedback);

            var currentCharacter = characters.Single(c => c.gameObject.activeSelf);
            currentCharacter.gameObject.SetActive(false);

            wrongTL.time = wrongTL.duration;
            wrongTL.Evaluate();
            wrongTL.Stop();

            examTXT.gameObject.SetActive(true);
            IsSubmit = false;

            submitExample.FinishComeback();
        }
        public void Out()
        {
            LOG.Info($"Out()", this);

            var currentCharacter = characters.Single(c => c.gameObject.activeSelf);
            currentCharacter.Out();
        }



        // Fields
        private ExampleData exam = null;
        private ExampleCharacter submitExample = null;
        private Coroutine crFeedback = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Character[] characters = null;
        [SerializeField] private TextMeshProUGUI examTXT = null;
        [SerializeField] private Image answerIMG = null;
        [SerializeField] private TextMeshProUGUI answerTXT = null;
        [SerializeField] private GameObject glowGO = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector correctInTL = null;
        [SerializeField] private PlayableDirector correctOutTL = null;
        [SerializeField] private PlayableDirector wrongTL = null;

        // Unity Messages
        private void Awake()
        {
            glowGO.SetActive(false);
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            if (!IsSubmit)
                glowGO.SetActive(true);
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerExit2D() | {collision.gameObject.name}", this);

            glowGO.SetActive(false);
        }

        // Unity Coroutine
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect", this))
            {
                examTXT.gameObject.SetActive(false);
                characters.SetActiveOnly(CharacterID - 1);

                var currentCharacter = characters.Single(c => c.gameObject.activeSelf);
                currentCharacter.StartCorrect();

                correctInTL.time = 0;
                correctInTL.Play();
                yield return new WaitForSeconds((float)correctInTL.duration);

                yield return AudioMGR.One.PlayNarrationAndWait(exam.WordCLIP);
                yield return new WaitForSeconds(0.5f);

                correctOutTL.time = 0;
                correctOutTL.Play();
                yield return new WaitForSeconds((float)correctOutTL.duration);
                yield return new WaitForSeconds(0.5f);
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong", this))
            {
                examTXT.gameObject.SetActive(false);
                characters.SetActiveOnly(CharacterID - 1);

                var currentCharacter = characters.Single(c => c.gameObject.activeSelf);
                currentCharacter.Wrong();

                wrongTL.time = 0;
                wrongTL.Play();
                yield return new WaitForSeconds((float)wrongTL.duration);
                yield return new WaitForSeconds(0.5f);

                examTXT.gameObject.SetActive(true);
                IsSubmit = false;
                yield return null;

                yield return submitExample.StartComeback();
            }
        }



        // IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var eample = eventData.pointerDrag.GetComponent<ExampleCharacter>();
            if (eample != null && !IsSubmit)
            {
                eventData.Use();

                submitExample = eample;
                IsSubmit = true;
            }
        }

        // IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (exam != null)
            {
                AudioMGR.One.PlayNarration(exam.WordCLIP);
            }
        }
    }
}