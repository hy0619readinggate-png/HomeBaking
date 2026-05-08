using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C4_A05
{
    public class WallMGR : MonoBehaviour
    {
        // Properties
        public bool IsCorrect => currentWall.IsComplete;
        public float[] AvailableBlanksPosX => currentWall.AvailableBlanksPosX;

        // Methods
        public void Setup(ProblemData pData, int pNO)
        {
            LOG.Info($"Setup()", this);

            currentWall = activeWall(pNO, pData.TextsCount);
            currentWall.Setup(pData);
        }
        public Coroutine StartWaitComplete()
        {
            LOG.Info($"StartWaitComplete()", this);

            currentWall.EnableInteraction(true);

            crWaitComplete = StartCoroutine(coWaitComplete());
            return crWaitComplete;
        }
        public void FinishWaitComplete()
        {
            LOG.Info($"FinishWaitComplete()", this);

            currentWall.EnableInteraction(false);

            this.StopCoroutineSafe(ref crWaitComplete);
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
        }
        public Coroutine StartChangeWall()
        {
            LOG.Info($"StartChangeWall()", this);

            crChangeWall = StartCoroutine(coChangeWall());
            return crChangeWall;
        }
        public void FinishChangeWall()
        {
            LOG.Info($"FinishChangeWall()", this);

            this.StopCoroutineSafe(ref crChangeWall);
        }
        public void Outro()
        {
            LOG.Info($"Outro()", this);

            anim.SetTrigger("Outro");
            bgAnim.SetTrigger("Outro");
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private Wall currentWall = null;
        private Coroutine crWaitComplete = null;
        private Coroutine crCorrect = null;
        private Coroutine crChangeWall = null;

        // Functions
        private Wall activeWall(int pNO, int textsCount)
        {
            var wallGroup = wallGroups[pNO - 1];
            var index = textsCount - 3;

            wallGroup.SetChildActiveOnly(index);
            return wallGroup.GetChild(index).GetComponent<Wall>();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform[] wallGroups = null;
        [SerializeField] private Animator bgAnim = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coWaitComplete()
        {
            using (LOG.Coroutine($"coWaitComplete()", this))
            {
                yield return new WaitUntil(() => currentWall.IsComplete);

                yield return new WaitForSeconds(1f);
            }
        }
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                currentWall.Correct();

                yield return new WaitForSeconds(0.5f);
            }
        }
        IEnumerator coChangeWall()
        {
            using (LOG.Coroutine($"coChangeWall()", this))
            {
                anim.SetTrigger("Next");
                bgAnim.SetTrigger("Next");

                yield return new WaitForSeconds(3f);
            }
        }
    }
}