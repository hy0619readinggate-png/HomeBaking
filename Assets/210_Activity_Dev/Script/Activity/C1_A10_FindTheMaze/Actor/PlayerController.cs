using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A10
{
    public class PlayerController : BYDSingleton<PlayerController>,
        IPointerDownHandler, IPointerUpHandler
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;

            isDown = false;
        }
        public void IgnoreCurrentControl()
        {
            LOG.Info($"IgnoreCurrentControl()", this);

            Dodo.One.StopMoving();
            isDown = false;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private bool isDown = false;



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }
        private void Update()
        {
            if (isDown)
            {
                var worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Dodo.One.MoveTo(worldPosition);
            }
        }



        // Interface : IPointerDownHandler, IPointerUpHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (isDown)
                Dodo.One.StopMoving();
            isDown = false;
        }
    }
}