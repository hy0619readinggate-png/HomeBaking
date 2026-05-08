using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class EBookQuizPanel_T5 : EBookQuizPanel
    {
        // Fields
        private EBookQuizData_Type5 quizData = null;
        private T2Example submitExample = null;

        // Event Handlers
        private void backdropBTN_OnClick()
        {
            LOG.Info($"backdropBTN_OnClick()", this);

            question.StopNarration();
        }

        // Overrides
        protected override int onGetQuizType() => 5;
        protected override int onGetLife() => 1;
        protected override bool onGetIsCorrect()
        {
            return submitExample?.IsAnswer ?? false;
        }
        protected override void onSetup(EBookQuizData data)
        {
            quizData = data as EBookQuizData_Type5;

            question.Setup(quizData.Question, quizData.QuestionCLIP);
            subjectIMG.sprite = quizData.QuestionSPR;

            examples[0].Setup(null, quizData.IsTrue);
            examples[1].Setup(null, !quizData.IsTrue);
        }
        protected override void onEnableUserInteraction(bool enable)
        {
            question.EnableInteraction(enable);
            examples.ForEach(ex => ex.EnableInteraction(enable));

            if (enable)
                backdropBTN.onClick.AddListener(backdropBTN_OnClick);
            else backdropBTN.onClick.RemoveListener(backdropBTN_OnClick);
        }
        protected override IEnumerator onShowProblem()
        {
            yield return question.PlayNarration();
            yield return null;
        }
        protected override IEnumerator onSolveProblem()
        {
            var answer = examples.First(ex => ex.IsAnswer);
            affTR.position = answer.transform.position;

            var wait = new WaitForSubmit(this, examples);
            yield return wait;

            submitExample = wait.Submited as T2Example;
            yield return null;
        }
        protected override IEnumerator onCheckAnswer()
        {
            submitExample.Feedback();
            yield return new WaitForSeconds(feedbackDuration);
        }
        protected override IEnumerator onShowAnswer()
        {
            var answerExam = examples.Single(ex => ex.IsAnswer);
            if (answerExam != submitExample)
                answerExam.ShowAnswer();

            yield return null;
        }
        protected override IEnumerator onReview()
        {
            // 리뷰없음
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button backdropBTN = null;
        [SerializeField] private T2Question question;
        [SerializeField] private T2Example[] examples;
        [SerializeField] private Image subjectIMG;
        [SerializeField] private Transform affTR = null;
        [Header("★ Config")]
        [SerializeField] private float feedbackDuration = 1;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}