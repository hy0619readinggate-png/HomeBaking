using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A09
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Animator))]
    public class ProblemStar : MonoBehaviour,
        IDropHandler, IPointerDownHandler
    {
        // Methods
        public void Activate(bool activate)
        {
            LOG.Info($"Activate() | {activate}", this);

            cg.blocksRaycasts = activate;
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);
        }
        public void Complete()
        {
            LOG.Info($"Complete()", this);
        }

        // Events
        public event Action<ProblemStar, ExampleStar> OnSubmit;
        public event Action OnClick;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= GetComponent<CanvasGroup>();



        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"IDropHandler.OnDrop()", this);

            var exampleStar = eventData.pointerDrag.GetComponent<ExampleStar>();
            LOG.Assert(exampleStar != null, $"pointerDrag must be ExamplePart", this);

            if (exampleStar != null)
            {
                eventData.Use();

                OnSubmit?.Invoke(this, exampleStar);
            }
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"IPointerDownHandler.OnPointerDown()", this);

            OnClick?.Invoke();
        }
    }
}