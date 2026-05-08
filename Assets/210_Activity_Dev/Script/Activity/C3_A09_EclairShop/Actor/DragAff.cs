using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C3_A09
{
    public class DragAff : AffBase
    {
        // Overrides
        protected override IEnumerator onStartAff()
        {
            var breads = breadMGR.AvaliableBreads;

            if (breads.Length > 0)
            {
                var bread = UtilArray.ExtractOne(breads);
                var slot = tray.FindSlot(bread.Text);

                if (slot == null)
                    yield break;

                yield return coDrag(bread.transform, slot.transform);
                yield return new WaitForSeconds(1);
                yield return coDrag(bread.transform, slot.transform);
            }

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            affTargetGO.SetActive(false);
            DOTween.Kill(affTargetGO.transform);
            this.StopAllCoroutines();
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private BreadMGR breadMGR = null;
        [SerializeField] private Tray tray = null;
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

        // Unity Coroutine
        IEnumerator coDrag(Transform sourceTR, Transform destTR)
        {
            affTargetGO.transform.position = sourceTR.position;
            affTargetGO.SetActive(true);

            fingerUpGO.SetActive(true);
            fingerDownGO.SetActive(false);
            yield return new WaitForSeconds(0.5f);

            fingerUpGO.SetActive(false);
            fingerDownGO.SetActive(true);
            yield return new WaitForSeconds(0.5f);

            var duration = Vector2.Distance(sourceTR.position, destTR.position) / dragSpeed;
            yield return affTargetGO.transform.DOMove(destTR.position, duration)
                .SetEase(Ease.Linear)
                .WaitForCompletion();

            fingerUpGO.SetActive(true);
            fingerDownGO.SetActive(false);
            yield return new WaitForSeconds(0.2f);

            affTargetGO.SetActive(false);
            yield return null;
        }
    }
}