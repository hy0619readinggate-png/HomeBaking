using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A09
{
    public class ExampleStarMGR : MonoBehaviour
    {
        // Properties
        public ExampleStar AnswerExampleStar => exampleStars.SingleOrDefault(e => e.IsAnswer);

        // Methods
        public void Init(ExampleStarParam param)
        {
            LOG.Info($"Init()", this);

            exampleStars.ForEach(e => e.Init(param));
        }
        public void Setup(ExampleData[] exams)
        {
            LOG.Info($"Setup()", this);

            var randomColorIDs = UtilArray.Random(1, exampleStars.Length);
            foreach (var (e, i) in exampleStars.Select((e, i) => (e, i)))
            {
                e.Setup(randomColorIDs[i], exams[i]);
                e.gameObject.SetActive(false);
            }
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;

            exampleStars.ForEach(e => e.EnableInteraction(enable));
        }
        public Coroutine StartAppear()
        {
            LOG.Info($"StartAppear()", this);

            crStartAppear = StartCoroutine(coStartAppear());
            return crStartAppear;
        }
        public void FinishAppear()
        {
            LOG.Info($"FinishAppear()", this);

            this.StopCoroutineSafe(ref crStartAppear);

            exampleStars.ForEach(e => e.Idle());
        }
        public Coroutine StartDisappear()
        {
            LOG.Info($"StartDisappear()", this);

            crStartDisappear = StartCoroutine(coStartDisappear());
            return crStartDisappear;
        }
        public void FinishDisppear()
        {
            LOG.Info($"FinishDisppear()", this);

            this.StopCoroutineSafe(ref crStartDisappear);

            exampleStars.ForEach(e => e.Hidden());
        }
        public void Correct(int submitExampleID)
        {
            LOG.Info($"Correct() | {submitExampleID}", this);

            var submitExampleStar = exampleStars.SingleOrDefault(e => e.ID == submitExampleID);
            submitExampleStar.gameObject.SetActive(false);
        }
        public void Wrong(int submitExampleID)
        {
            LOG.Info($"Wrong() | {submitExampleID}", this);

            var submitExampleStar = exampleStars.SingleOrDefault(e => e.ID == submitExampleID);
            submitExampleStar.Return();
        }
        public void Shuffle()
        {
            LOG.Info($"Shuffle()", this);

            var randomColorIDs = exampleStars.Select(e => e.ColorID).ToArray();
            var exams = exampleStars.Select(e => e.ExampleData).ToArray();
            var shffledIndices = UtilArray.Random(0, exams.Length - 1);

            foreach (var (e, i) in exampleStars.Select((e, i) => (e, i)))
            {
                var idx = shffledIndices[i];
                e.Setup(randomColorIDs[idx], exams[idx]);
            }
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private ExampleStar[] examplesStars_ = null;
        private ExampleStar[] exampleStars => examplesStars_ ??= GetComponentsInChildren<ExampleStar>(true);

        // Fields
        private Coroutine crStartAppear = null;
        private Coroutine crStartDisappear = null;



        // Unity Inspectors
        [Header("★ Sound")]
        [SerializeField] private AudioClip appearCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float appearMinDelay = 0.2f;
        [SerializeField] private float appearMaxDelay = 0.5f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            exampleStars.AutoFillID();
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coStartAppear()
        {
            using (LOG.Coroutine($"coStartAppear()", this))
            {
                var suffled = UtilArray.Shuffled(exampleStars);
                foreach (var (e, i) in suffled.Select((e, i) => (e, i)))
                {
                    suffled[i].gameObject.SetActive(true);
                    suffled[i].Appear();
                    AudioMGR.One.PlayEffect(appearCLIP);
                    yield return null;

                    var delay = Random.Range(appearMinDelay, appearMaxDelay);
                    yield return new WaitForSeconds(delay);
                }
            }
        }
        IEnumerator coStartDisappear()
        {
            using (LOG.Coroutine($"coStartDisappear()", this))
            {
                var suffled = UtilArray.Shuffled(exampleStars);
                foreach (var (e, i) in suffled.Select((e, i) => (e, i)))
                {
                    suffled[i].Disappear();
                    AudioMGR.One.PlayEffect(appearCLIP);
                    yield return null;

                    var delay = Random.Range(appearMinDelay, appearMaxDelay);
                    yield return new WaitForSeconds(delay);
                }

            }

        }
    }
}