using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C4_A07
{
    public class DragAff : AffBase
    {
        // Methods
        public void Setup(IceProblem problem, IceExample[] exams)
        {
            LOG.Info($"Setup()", this);

            this.problem = problem;
            this.exams = exams;
        }



        // Fields
        private IceProblem problem = null;
        private IceExample[] exams = null;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            var exam = UtilArray.ExtractOne(exams);

            var startPos = exam.transform.position;
            var endPos = problem.transform.position;

            affTargetGO.transform.position = startPos;
            affTargetGO.SetActive(true);

            fingerUpGO.SetActive(true);
            fingerDownGO.SetActive(false);
            yield return new WaitForSeconds(0.5f);

            fingerUpGO.SetActive(false);
            fingerDownGO.SetActive(true);
            yield return new WaitForSeconds(0.5f);

            var duration = Vector2.Distance(startPos, endPos) / dragSpeed;
            yield return affTargetGO.transform.DOMove(endPos, duration)
                .SetEase(Ease.Linear)
                .WaitForCompletion();

            fingerUpGO.SetActive(true);
            fingerDownGO.SetActive(false);
            yield return new WaitForSeconds(0.5f);

            affTargetGO.SetActive(false);
            yield return null;

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            affTargetGO.SetActive(false);
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject fingerUpGO = null;
        [SerializeField] private GameObject fingerDownGO = null;
        [Header("★ Config")]
        [SerializeField] private float dragSpeed = 10f;



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            affTargetGO.SetActive(false);
        }
        private void Start()
        {
        }
    }
}