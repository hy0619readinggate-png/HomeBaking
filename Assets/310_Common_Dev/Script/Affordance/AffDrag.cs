using beyondi.Util;
using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Common
{
    public class AffDrag : AffBase
    {
        // Methods
        public void Setup(Transform[] sourceTRs, Transform[] destTRs)
        {
            LOG.Info($"Setup()", this);

            this.sourceTRs = sourceTRs;
            this.destTRs = destTRs;
        }



        // Fields
        private Transform[] sourceTRs = null;
        private Transform[] destTRs = null;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            LOG.Info($"{sourceTRs?.Length}, {destTRs?.Length}", this);
            if (sourceTRs?.Length > 0 && destTRs?.Length > 0)
            {
                var sourceTR = UtilArray.ExtractOne(sourceTRs);
                var destTR = UtilArray.ExtractOne(destTRs);

                var startPos = sourceTR.transform.position;
                var endPos = destTR.transform.position;

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

                finishAff();
            }
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