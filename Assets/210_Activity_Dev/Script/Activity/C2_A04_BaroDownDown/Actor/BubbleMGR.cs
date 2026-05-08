using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C2_A04
{
    public class BubbleMGR : MonoBehaviour
    {
        // Methods
        public Coroutine StartWaitPopAll(ProblemData pData)
        {
            LOG.Info($"StartWaitPopAll()", this);

            crStartWaitPopAll = StartCoroutine(coStartWaitPopAll(pData));
            return crStartWaitPopAll;
        }
        public void FinishWaitPopAll()
        {
            LOG.Info($"FinishWaitPopAll()", this);

            this.StopCoroutineSafe(ref crStartWaitPopAll);

            bubbleGroup.PopAll();
            Destroy(bubbleGroup.gameObject);
        }



        // Fields
        private BubbleGroup bubbleGroup = null;
        private Coroutine crStartWaitPopAll = null;

        // Functions
        private void spawnBubble(ProblemData pData)
        {
            var prefabGO = UtilArray.ExtractOne(bubbleGroupPrefabs);
            bubbleGroup = Instantiate(prefabGO, bubbleParentTR);
            bubbleGroup.Setup(pData, startTR.position, endTR.position, speed);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private BubbleGroup[] bubbleGroupPrefabs;
        [SerializeField] private Transform startTR;
        [SerializeField] private Transform endTR;
        [SerializeField] private Transform bubbleParentTR;
        [Header("★ Config")]
        [SerializeField] private float speed = 4f;
        [SerializeField] private float completeDelay = 1f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coStartWaitPopAll(ProblemData pData)
        {
            using (LOG.Coroutine($"coStartWaitPopAll()", this))
            {
                spawnBubble(pData);

                yield return new WaitForCompleteAll(this, bubbleGroup.Bubbles);

                yield return new WaitForSeconds(completeDelay);
            }

        }
    }
}