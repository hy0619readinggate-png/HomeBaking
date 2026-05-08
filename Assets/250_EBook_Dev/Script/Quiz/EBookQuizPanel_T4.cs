using beyondi.Util;
using DoDoEng.EBook.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DoDoEng.EBook.Quiz
{
    public class EBookQuizPanel_T4 : EBookQuizPanel
    {
        // Fields
        private EBookQuizData_Type4 quizData = null;

        // Overrides
        protected override int onGetQuizType() => 4;
        protected override int onGetLife() => 2;
        protected override bool onGetIsCorrect()
        {
            return targets.All(t => t.IsAnswer);
        }
        protected override void onSetup(EBookQuizData data)
        {
            quizData = data as EBookQuizData_Type4;

            examAreaGO.SetActive(true);
            reviewTextGO.SetActive(false);

            var shuffled = UtilArray.Shuffled(examples);
            foreach (var (ex, i) in shuffled.Select((ex, i) => (ex, i)))
                ex.Setup(
                    quizData.StorySPR[i],
                    quizData.StoryCLIP[i],
                    i + 1);

            foreach (var (target, i) in targets.Select((t, i) => (t, i)))
                target.Setup(quizData.StoryCLIP[i]);

            var targetTRs = targets.Select(t => t.transform).ToArray();
            aff.Setup(examples, targetTRs);
        }
        protected override void onEnableUserInteraction(bool enable)
        {
            targets.ForEach(t => t.EnableInteraction(enable));
            examples.ForEach(ex => ex.EnableInteraction(enable));
        }
        protected override IEnumerator onShowProblem()
        {
            foreach (var target in targets)
            {
                yield return target.PlayNarration();
                yield return new WaitForSeconds(narrationInterval);
            }
            yield return null;
        }
        protected override IEnumerator onSolveProblem()
        {
            // 오답의 경우 submit을 정리
            targets.ForEach(t => t.ResetSubmit());
            examples.ForEach(ex => ex.ResetExample());

            yield return new WaitUntil(() => targets.All(t => t.IsSubmit));
            yield return null;
        }
        protected override IEnumerator onCheckAnswer()
        {
            foreach (var target in targets)
            {
                target.Feedback();
                yield return new WaitForSeconds(feedbackInterval);
            }
            yield return new WaitForSeconds(feedbackDelay);
        }
        protected override IEnumerator onShowAnswer()
        {
            var targetWrong = targets.Where(t => !t.IsAnswer);
            var t0 = targetWrong.First();
            var tR = targetWrong.Skip(1);

            foreach (var target in tR)
            {
                var source = targets.Single(t => t.SubmitID == target.ID);
                target.ShowAnswer(source);
            }
            {
                var source = targets.Single(t => t.SubmitID == t0.ID);
                yield return t0.ShowAnswer(source);
            }

            yield return new WaitForSeconds(showAnswerDelay);
            yield return null;
        }
        protected override IEnumerator onReview()
        {
            var textQ = new Queue<string>(quizData.StoryText);

            examAreaGO.SetActive(false);
            reviewTextGO.SetActive(true);
            reviewTXT.text = textQ.Peek();

            foreach (var target in targets)
            {
                reviewTXT.text = textQ.Dequeue().Replace("\\n", "\n"); ;
                yield return target.PlayNarration();
                yield return new WaitForSeconds(narrationInterval);
            }
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private T3Example[] examples = null;
        [SerializeField] private T3Target[] targets = null;
        [SerializeField] private GameObject examAreaGO = null;
        [SerializeField] private GameObject reviewTextGO = null;
        [SerializeField] private TextMeshProUGUI reviewTXT = null;
        [SerializeField] private T3Affordance aff = null;
        [Header("★ Config")]
        [SerializeField] private float narrationInterval = 0.5f;
        [SerializeField] private float feedbackInterval = 0.4f;
        [SerializeField] private float feedbackDelay = 0.5f;
        [SerializeField] private float showAnswerDelay = 0.5f;

        // Unity Messages
        private void Awake()
        {
            targets.AutoFillID();
            examAreaGO.SetActive(true);
            reviewTextGO.SetActive(false);
        }
        private void Start()
        {
        }
    }
}