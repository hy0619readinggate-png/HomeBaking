using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.C4_A05
{
    public class Gino : MonoBehaviour
    {
        // Methods
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            hide();
        }
        public void StartAppear()
        {
            LOG.Info($"StartAppear()", this);

            appear = true;

            crAppear = StartCoroutine(coAppear());
        }
        public void StopAppear()
        {
            LOG.Info($"StopAppear()", this);

            appear = false;

            this.StopCoroutineSafe(ref crAppear);

            showTL.ForEach(t => t.Stop());
        }
        public void StartWrong()
        {
            LOG.Info($"StartWrong()", this);

            crWrong = StartCoroutine(coWrong());
        }
        public void FinishWrong()
        {
            LOG.Info($"FinishWrong()", this);

            this.StopCoroutineSafe(ref crWrong);

            wrongTL.ForEach(t => t.Stop());
        }
        public Coroutine StartCorrect()
        {
            LOG.Info($"StartCorrect()", this);

            crCorrect = StartCoroutine(coCorrect());
            return crCorrect;
        }
        public void FinishCorrect()
        {
            LOG.Info($"FinishCorrect()", this);

            this.StopCoroutineSafe(ref crCorrect);

            correctTL.ForEach(t => t.Stop());
        }
        public Coroutine StartOut()
        {
            LOG.Info($"StartOut()", this);

            crOut = StartCoroutine(coOut());
            return crOut;
        }
        public void FinishOut()
        {
            LOG.Info($"FinishOut()", this);

            this.StopCoroutineSafe(ref crOut);

            outTL[positionIdx].Stop();
            hide();
        }



        // Fields
        private int positionIdx;
        private bool appear = false;
        private Coroutine crAppear = null;
        private Coroutine crCorrect = null;
        private Coroutine crWrong = null;
        private Coroutine crOut = null;

        // Functions
        private void hide()
        {
            transform.SetActiveAllChildren(false);
        }
        private void readyGino()
        {
            positionIdx = UtilArray.RandomOne(0, 2);
            if (positionIdx == 2)
            {
                var blanksPosX = wallMGR.AvailableBlanksPosX;
                if (blanksPosX.Length > 0)
                {
                    var posX = UtilArray.ExtractOne(blanksPosX);
                    var pos = ginoMidTR.position;
                    ginoMidTR.position = new Vector3(posX, pos.y, pos.z);
                }
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlayableDirector[] showTL = null;
        [SerializeField] private PlayableDirector[] correctTL = null;
        [SerializeField] private PlayableDirector[] wrongTL = null;
        [SerializeField] private PlayableDirector[] outTL = null;
        [SerializeField] private WallMGR wallMGR = null;
        [SerializeField] private Transform ginoMidTR = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip[] correctClip = null;
        [SerializeField] private AudioClip[] wrongClip = null;
        [Header("★ Config")]
        [SerializeField] private float appearDelay = 0.3f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator playTimeline(PlayableDirector timeline)
        {
            timeline.time = 0;
            timeline.Evaluate();
            timeline.Play();
            yield return new WaitForSeconds((float)timeline.duration);
        }
        IEnumerator coAppear()
        {
            using (LOG.Coroutine($"coAppear()", this))
            {
                while (appear)
                {
                    readyGino();
                    yield return playTimeline(showTL[positionIdx]);
                    yield return null;

                    hide();

                    yield return new WaitForSeconds(appearDelay);
                }
            }
        }
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                var clip = UtilArray.ExtractOne(correctClip);
                AudioMGR.One.PlayEffect(clip);
                yield return playTimeline(correctTL[positionIdx]);
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {

                var clip = UtilArray.ExtractOne(wrongClip);
                AudioMGR.One.PlayEffect(clip);
                yield return playTimeline(wrongTL[positionIdx]);
            }
        }
        IEnumerator coOut()
        {
            using (LOG.Coroutine($"coOut()", this))
            {
                yield return playTimeline(outTL[positionIdx]);
            }
        }
    }
}