using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Activity.UI;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace DoDoEng.Activity.C4_A07
{
    public class ProblemZone : MonoBehaviour
    {
        // Properties
        public IceProblem Problem => problems.First(p => p.IsBlank);
        public IceExample[] Examples => examples;

        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Function(this);

            this.pData = pData;

            problemIMG.sprite = pData.SentenceSPR;
            for (var i = 0; i < problems.Length; i++)
            {
                var problem = problems[i];
                var active = i < pData.SubjectCount;

                problem.gameObject.SetActive(active);
                if (active)
                    problem.Setup(pData.Subjects[i]);
            }

            sentenceTXT.text = pData.Sentence;

            examples.ForEach((i, e) => e.Setup(pData.Examples[i]));
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Function(this);

            cg.blocksRaycasts = enable;
        }
        public Coroutine ShowExamples()
        {
            LOG.Function(this);

            crShowExamples = StartCoroutine(coShowExamples());
            return crShowExamples;
        }
        public void ShownExamples()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crShowExamples);

            examples.ForEach(e => e.Shown());
        }
        public Coroutine StartWaitForComplete()
        {
            LOG.Function(this);

            crWaitForComplete = StartCoroutine(coWaitForComplete());
            return crWaitForComplete;
        }
        public void FinishWaitForComplete()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crWaitForComplete);
            this.StopCoroutineSafe(ref crWrong);
            this.StopCoroutineSafe(ref crPlaySound);
        }
        public void ClearExamples()
        {
            LOG.Function(this);

            foreach (var exam in examples)
            {
                if (!exam.IsAnswer)
                    exam.Hide();
            }
        }
        public void PlaySound()
        {
            LOG.Function(this);

            crPlaySound = StartCoroutine(coPlaySound());
        }


        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private TimelineSignal wrongSIG_ = null;
        private TimelineSignal wrongSIG => wrongSIG_ ??= wrongTL.GetComponent<TimelineSignal>();

        // Fields
        private ProblemData pData = null;
        private Coroutine crShowExamples = null;
        private Coroutine crWaitForComplete = null;
        private Coroutine crWrong = null;
        private Coroutine crPlaySound = null;
        private bool isComplete = false;

        // Event Handlers
        private void problem_OnCorrect(bool isFeedbackPlaying)
        {
            LOG.Function(this, $"{isFeedbackPlaying}");

            if (!isFeedbackPlaying)
                cg.blocksRaycasts = false;
            else isComplete = true;
        }
        private void problem_OnWrong()
        {
            LOG.Function(this);

            ActivityProgress.One.Wrong();
            crWrong = StartCoroutine(coWrong());
        }
        private void questionBTN_OnClick()
        {
            LOG.Function(this);

            crPlaySound = StartCoroutine(coPlaySound());
        }
        private void wrongSIG_OnSignal(string signal)
        {
            LOG.Info($"wrongSIG_OnSignal() | {signal}", this);

            if (signal == "Activity-ExtraAnimation")
            {
                var clip = UtilArray.ExtractOne(wrongClip);
                AudioMGR.One.PlayEffect(clip);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image problemIMG = null;
        [SerializeField] private IceProblem[] problems = null;
        [SerializeField] private IceExample[] examples = null;
        [SerializeField] private TextMeshProUGUI sentenceTXT = null;
        [SerializeField] private Button questionBTN = null;
        [SerializeField] private Animator questionANIM = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector wrongTL = null;
        [Header("★ Audios")]

        [SerializeField] private AudioClip[] wrongClip = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            questionBTN.onClick.AddListener(questionBTN_OnClick);
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            problems.ForEach(p => p.OnCorrect += problem_OnCorrect);
            problems.ForEach(p => p.OnWrong += problem_OnWrong);
            wrongSIG.OnSignal += wrongSIG_OnSignal;
        }
        private void OnDisable()
        {
            problems.ForEach(p => p.OnCorrect -= problem_OnCorrect);
            problems.ForEach(p => p.OnWrong -= problem_OnWrong);
            wrongSIG.OnSignal -= wrongSIG_OnSignal;
        }

        // Unity Coroutine
        IEnumerator coShowExamples()
        {
            using (LOG.Coroutine($"coShowExamples()", this))
            {
                examples.ForEach((i, e) => { e.Show(i == 0); });
                yield return new WaitForSeconds(1.1f);
            }
        }
        IEnumerator coWaitForComplete()
        {
            using (LOG.Coroutine($"coWaitForComplete()", this))
            {
                isComplete = false;
                yield return new WaitUntil(() => isComplete);
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                cg.blocksRaycasts = false;
                yield return null;

                wrongTL.time = 0;
                wrongTL.Play();
                yield return new WaitForSeconds((float)wrongTL.duration);

                yield return new WaitForSeconds(1);
                yield return AudioMGR.One.PlayNarrationAndWait(pData.SentenceCLIP);

                cg.blocksRaycasts = true;
                yield return null;
            }
        }
        IEnumerator coPlaySound()
        {
            using (LOG.Coroutine($"coPlaySound()", this))
            {
                cg.blocksRaycasts = false;
                UIActivityCommon.One.EnableSpeakerButton = false;
                yield return null;

                questionANIM.SetTrigger("Click");
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pData.SentenceCLIP);

                cg.blocksRaycasts = true;
                UIActivityCommon.One.EnableSpeakerButton = true;
                yield return null;
            }

        }
    }
}