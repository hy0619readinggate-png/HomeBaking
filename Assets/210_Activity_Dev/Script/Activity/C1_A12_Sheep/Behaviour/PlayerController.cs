using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

namespace DoDoEng.Activity.C1_A12
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



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= rt.GetParentCanvas();

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
                Papa.One.MoveTo(worldPosition);
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
                Papa.One.StopMoving();
            isDown = false;
        }
    }
}