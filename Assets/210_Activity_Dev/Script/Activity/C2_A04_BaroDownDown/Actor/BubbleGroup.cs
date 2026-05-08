using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A04
{
    public class BubbleGroup : MonoBehaviour
    {
        // Properties
        public Bubble[] Bubbles => bubbles;

        // Methods
        public void Setup(ProblemData pData, Vector3 startPosition, Vector3 endPosition, float speed)
        {
            LOG.Info($"Setup()", this);

            bubbles.ForEach(b => b.Setup(pData));

            var distance = Vector2.Distance(endPosition, startPosition);
            var duration = distance / speed;
            transform
                .DOMoveY(endPosition.y, duration)
                .From(startPosition)
                .SetEase(Ease.Linear);
        }
        public void PopAll()
        {
            LOG.Info($"PopAll()", this);

            transform.DOKill();
            bubbles.ForEach(b => b.Pop());
        }



        // Fields : caching
        private Bubble[] bubbles_ = null;
        private Bubble[] bubbles => bubbles_ ??= GetComponentsInChildren<Bubble>();



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}