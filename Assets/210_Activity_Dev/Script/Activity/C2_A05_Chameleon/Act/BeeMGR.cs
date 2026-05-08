using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.C2_A05
{
    public class BeeMGR : MonoBehaviour
    {
        // Properties
        public Bee[] AlLBees => bees;
        public Bee[] AliveBees => bees.Where(b => b.IsAlive).ToArray();
        public Bee AnswerBee => bees.Where(b => b.IsAnswer).SingleOrDefault();

        // Methods
        public void Setup(ExampleData[] exams)
        {
            LOG.Info($"Setup()", this);

            cg.alpha = 0;
            bees.ForEach((i, b) => b.Setup(exams[i]));
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            bees.ForEach(b => b.EnableInteraction(enable));
        }
        public Coroutine StartIn()
        {
            LOG.Info($"StartIn()", this);

            crStartIn = StartCoroutine(coStartIn());
            return crStartIn;
        }
        public void FinishIn()
        {
            LOG.Info($"FinishIn()", this);

            this.StopCoroutineSafe(ref crStartIn);

            cg.alpha = 1;
            stopAndUnassignTimeline();
        }
        public void TakeAndOut(int id)
        {
            LOG.Info($"TakeAndOut()", this);

            bees.Where(b => b.ID == id).SingleOrDefault().Take();
            if (AliveBees.Length > 0)
            {
                timeline = UtilArray.ExtractOne(beeOutTL);
                timeline.time = 0;
                timeline.Play();
            }
        }



        // Fields : caching
        private Bee[] bees_ = null;
        private Bee[] bees => bees_ ??= GetComponentsInChildren<Bee>(true);
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private PlayableDirector timeline = null;
        private Coroutine crStartIn = null;

        // Functions
        private void stopAndUnassignTimeline()
        {
            if (timeline != null)
            {
                if (timeline.state == PlayState.Playing)
                    timeline.Stop();

                timeline.time = timeline.duration;
                timeline.Evaluate();
            }
            timeline = null;
        }



        // Unity Inspectors
        [Header("★ TimeLine")]
        [SerializeField] private PlayableDirector[] beeInTL = null;
        [SerializeField] private PlayableDirector[] beeOutTL = null;

        // Unity Messages
        private void Awake()
        {
            cg.alpha = 0;
            bees.AutoFillID();
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coStartIn()
        {
            using (LOG.Coroutine($"coStartIn()", this))
            {
                cg.alpha = 1;

                timeline = UtilArray.ExtractOne(beeInTL);
                timeline.time = 0;
                timeline.Play();
                yield return new WaitForSeconds((float)timeline.duration);
            }
        }
    }
}