using DoDoEng.EBook.Framework;
using System.Collections;
using UnityEngine;

namespace DoDoEng.EBook.Quiz
{
    public abstract class EBookQuizPanel : MonoBehaviour
    {
        // Properties
        public int QuizType => onGetQuizType();
        public int Life => onGetLife();
        public bool IsCorrect => onGetIsCorrect();

        // Methods
        public void Setup(EBookQuizData quizData) => onSetup(quizData);
        public void EnableUserInteraction(bool enable) => onEnableUserInteraction(enable);
        public IEnumerator ShowProblem() => onShowProblem();
        public IEnumerator SolveProblem() => onSolveProblem();
        public IEnumerator CheckAnswer() => onCheckAnswer();
        public IEnumerator ShowAnswer() => onShowAnswer();
        public IEnumerator Review() => onReview();



        // Virtual
        protected abstract int onGetQuizType();
        protected abstract int onGetLife();
        protected abstract bool onGetIsCorrect();
        protected abstract void onSetup(EBookQuizData quizData);
        protected abstract void onEnableUserInteraction(bool enable);
        protected abstract IEnumerator onShowProblem();
        protected abstract IEnumerator onSolveProblem();
        protected abstract IEnumerator onCheckAnswer();
        protected abstract IEnumerator onShowAnswer();
        protected abstract IEnumerator onReview();
    }
}