using beyondi.Util;
using DoDoEng.Common;
using DoDoEng.EBook.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class EBookQuizPanel_T1 : EBookQuizPanel
    {
        // Fields
        private EBookQuizData_Type1 quizData = null;

        // Event Handlers
        private void backdropBTN_OnClick()
        {
            LOG.Info($"backdropBTN_OnClick()", this);

            question.StopNarration();
        }

        // Overrides
        protected override int onGetQuizType() => 1;
        protected override int onGetLife() => 1;
        protected override bool onGetIsCorrect()
        {
            return question.IsAnswer;
        }
        protected override void onSetup(EBookQuizData data)
        {
            quizData = data as EBookQuizData_Type1;

            question.Setup(quizData.Question, quizData.QuestionCLIP);

            var shuffled = UtilArray.Shuffled(examples);
            shuffled[0].Setup(quizData.CorrectSPR, true);
            aff.Variation = examples.FindIndex(shuffled[0]);
            shuffled[1].Setup(quizData.WrongSPR, false);
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
            yield return new WaitUntil(() => question.IsSubmit);
            yield return null;
        }
        protected override IEnumerator onCheckAnswer()
        {
            question.Feedback();
            yield return new WaitForSeconds(feedbackDuration);
        }
        protected override IEnumerator onShowAnswer()
        {
            var answerExam = examples.Single(ex => ex.IsAnswer);
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
        [SerializeField] private T1Question question;
        [SerializeField] private T1Example[] examples;
        [SerializeField] private AffMecanim aff = null;
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