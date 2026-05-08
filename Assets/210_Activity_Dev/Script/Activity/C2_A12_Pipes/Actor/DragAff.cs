using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A12
{
    public class DragAff : AffBase
    {
        // Methods
        public void Setup(Subject[] subjects)
        {
            LOG.Info($"Setup()", this);

            this.subjects = subjects;
        }



        // Fields
        private Vector3 originPosition;
        private Subject[] subjects = null;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            var incompleteSubjects = subjects.Where(s => !s.IsComplete).ToArray();

            if (incompleteSubjects.Length > 0)
            {
                var subject = UtilArray.ExtractOne(incompleteSubjects);

                affTargetGO.transform.position = originPosition;
                affTargetGO.SetActive(true);

                fingerUpGO.SetActive(true);
                fingerDownGO.SetActive(false);
                yield return new WaitForSeconds(0.5f);

                fingerUpGO.SetActive(false);
                fingerDownGO.SetActive(true);
                yield return new WaitForSeconds(0.5f);

                var duration = Vector2.Distance(subject.transform.position, originPosition) / dragSpeed;
                yield return affTargetGO.transform.DOMove(subject.transform.position, duration)
                    .SetEase(Ease.Linear)
                    .WaitForCompletion();

                fingerUpGO.SetActive(true);
                fingerDownGO.SetActive(false);
                yield return new WaitForSeconds(0.5f);

                affTargetGO.SetActive(false);
                yield return null;
            }

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            DOTween.Kill(affTargetGO.transform);

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

            originPosition = affTargetGO.transform.position;
            affTargetGO.SetActive(false);
        }
        private void Start()
        {
        }
    }
}