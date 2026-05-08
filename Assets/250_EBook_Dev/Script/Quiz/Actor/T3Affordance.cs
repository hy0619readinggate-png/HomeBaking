using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.EBook.Quiz
{
    public class T3Affordance : AffBase
    {
        // Methods
        public void Setup(T3Example[] exams, Transform[] targetTRs)
        {
            LOG.Info($"Setup()", this);

            this.exams = exams;
            this.targetTRs = targetTRs;
        }



        // Fields
        private Transform[] targetTRs = null;
        private T3Example[] exams = null;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            var exam = exams.FirstOrDefault(ex => ex.IsReady);

            if (exam != null)
            {
                var startPos = exam.transform.position;
                var endPos = targetTRs[exam.Sequence - 1].position;

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
                yield return new WaitForSeconds(0.2f);

                fingerUpGO.SetActive(true);
                fingerDownGO.SetActive(false);
                yield return new WaitForSeconds(0.5f);

                affTargetGO.SetActive(false);
                yield return null;

                finishAff();
            }
        }
        protected override IEnumerator onFinishAff()
        {
            DOTween.Kill(affTargetGO.transform);
            affTargetGO.SetActive(false);
            this.StopAllCoroutines();
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