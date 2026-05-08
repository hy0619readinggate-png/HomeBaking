using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.C3_A06
{
    public class Feedback : MonoBehaviour
    {
        // Methods
        public Coroutine StartFeedback(int score)
        {
            LOG.Info($"StartFeedback() | {score}", this);

            this.score = score;

            crStartFeedback = StartCoroutine(coStartFeedback());
            return crStartFeedback;
        }
        public void StopFeedback()
        {
            LOG.Info($"StopFeedback()", this);

            this.StopCoroutineSafe(ref crStartFeedback);

            if (feedbackTL == null)
                return;

            feedbackTL.time = feedbackTL.duration;
            feedbackTL.Evaluate();
            feedbackTL.Stop();
        }



        // Fields
        int score;
        private Coroutine crStartFeedback = null;
        private PlayableDirector feedbackTL = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlayableDirector[] feedbacksTL = null;
        [SerializeField] private int[] feedbackScore = new int[] { 86, 71, 30 };
        [Header("★ Audios")]
        [SerializeField] private AudioClip[] wrongCLIP = null;
        [Header("★ Config")]
        [SerializeField] private int testScore = -1;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        private IEnumerator coStartFeedback()
        {
            using (LOG.Coroutine($"coStartFeedback()", this))
            {
                if (testScore != -1)
                    score = testScore;

                var idx = feedbackScore.TakeWhile(s => s > score).Count();
                LOG.Function(this, $"{idx}");
                feedbackTL = feedbacksTL[idx];
                feedbackTL.time = 0;
                yield return null;

                var wrong = idx == feedbackScore.Length;
                if (wrong)
                {
                    var clip = UtilArray.ExtractOne(wrongCLIP);
                    AudioMGR.One.PlayEffect(clip);
                }
                    
                feedbackTL.Play();
                yield return null;

                yield return new WaitForSeconds((float)feedbackTL.duration);
                yield return null;
            }
        }
    }
}