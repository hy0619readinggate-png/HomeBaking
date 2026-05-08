using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C3_A06
{
    public class DragAff : AffBase
    {
        // Overrides
        protected override IEnumerator onStartAff()
        {
            var pieces = puzzle.AvaliablePieces;

            if (pieces.Length > 0)
            {
                var piece = UtilArray.ExtractOne(pieces);
                var slot = puzzle.Slots.First(s => s.ID == piece.ID);

                yield return coDrag(piece.transform, slot.CenterTR);
                yield return new WaitForSeconds(1);
                yield return coDrag(piece.transform, slot.CenterTR);
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
        [SerializeField] private PuzzleMGR puzzle = null;
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