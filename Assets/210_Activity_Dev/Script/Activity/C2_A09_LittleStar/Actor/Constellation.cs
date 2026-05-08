using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.C2_A09
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Constellation : MonoBehaviour
    {
        // Properties
        public ProblemStar ProblemStar => problemStars[seq - 1];
        public bool IsAnswerSubmit => submitExampleStar.IsAnswer;
        public int SubmitExampleID => submitExampleStar.ID;

        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup() | {pData.Seq}", this);

            seq = pData.Seq;
            phonicsCLIP = pData.PhonicsCLIP;

            currentTL.time = 0;
            currentTL.Evaluate();
            currentTL.Stop();
        }
        public void Select()
        {
            LOG.Info($"Select() ", this);

            ProblemStar.Activate(true);
        }
        public Coroutine StartWaitSubmit()
        {
            LOG.Info($"StartWaitSubmit()", this);

            ProblemStar.Activate(true);
            crStartWaitSubmit = StartCoroutine(coStartWaitSubmit());
            return crStartWaitSubmit;
        }
        public void FinishWaitSubmit()
        {
            LOG.Info($"FinishWaitSubmit()", this);

            ProblemStar.Activate(false);
            this.StopCoroutineSafe(ref crStartWaitSubmit);
        }
        public Coroutine StartCorrect()
        {
            LOG.Info($"StartCorrect()", this);

            crStartCorrect = StartCoroutine(coStartCorrect());
            return crStartCorrect;
        }
        public void FinishCorrect()
        {
            LOG.Info($"FinishCorrect()", this);

            this.StopCoroutineSafe(ref crStartCorrect);

            var tl = correctTL[seq - 1];
            tl.time = tl.duration;
            tl.Evaluate();
            tl.Stop();
        }
        public Coroutine StartComplete()
        {
            LOG.Info($"StartComplete()", this);

            crStartComplete = StartCoroutine(coStartComplete());
            return crStartComplete;

        }
        public void FinishComplete()
        {
            LOG.Info($"FinishComplete()", this);

            this.StopCoroutineSafe(ref crStartComplete);

            completeTL.time = completeTL.duration;
            completeTL.Evaluate();
            completeTL.Stop();
        }

        // Methods : Debug
        public void DebugSubmit(ExampleStar exampleStar)
        {
            LOG.Info($"DebugSubmit()", this);

            submitExampleStar = exampleStar;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= GetComponent<CanvasGroup>();

        // Fields
        private int seq;
        private AudioClip phonicsCLIP;
        private Coroutine crStartWaitSubmit = null;
        private Coroutine crStartCorrect = null;
        private Coroutine crStartComplete = null;
        private ExampleStar submitExampleStar = null;

        // Functions
        private PlayableDirector currentTL => correctTL[seq - 1];

        // Event Handlers
        private void problemStar_OnSubmit(ProblemStar problemStar, ExampleStar exampleStar)
        {
            LOG.Info($"problemStar_OnSubmit()", this);

            if (problemStar == ProblemStar)
            {
                submitExampleStar = exampleStar;
            }
        }
        private void problemStar_OnClick()
        {
            LOG.Info($"problemStar_OnClick()", this);

            AudioMGR.One.PlayNarration(phonicsCLIP);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private ProblemStar[] problemStars = null;
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector[] correctTL = null;
        [SerializeField] private PlayableDirector completeTL = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            var tl = correctTL[0];
            tl.time = 0;
            tl.Evaluate();
            tl.Stop();
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            problemStars.ForEach(p => p.OnSubmit += problemStar_OnSubmit);
            problemStars.ForEach(p => p.OnClick += problemStar_OnClick);
        }
        private void OnDisable()
        {
            problemStars.ForEach(p => p.OnSubmit -= problemStar_OnSubmit);
            problemStars.ForEach(p => p.OnClick -= problemStar_OnClick);
        }

        // Unity Coroutine
        IEnumerator coStartWaitSubmit()
        {
            using (LOG.Coroutine($"coStartWaitSubmit()", this))
            {
                submitExampleStar = null;
                yield return new WaitUntil(() => submitExampleStar != null);

                ProblemStar.Activate(false);
            }
        }
        IEnumerator coStartCorrect()
        {
            using (LOG.Coroutine($"coStartCorrect() | {seq}", this))
            {
                var tl = correctTL[seq - 1];
                tl.time = 0;
                tl.Play();
                ProblemStar.Complete();
                yield return new WaitForSeconds((float)tl.duration);
            }
        }
        IEnumerator coStartComplete()
        {
            using (LOG.Coroutine($"crStartComplete() | {seq}", this))
            {
                completeTL.time = 0;
                completeTL.Play();
                yield return new WaitForSeconds((float)completeTL.duration);
            }
        }
    }
}