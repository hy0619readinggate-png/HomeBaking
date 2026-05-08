using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C1_G02
{
    public class PlayerController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private bool isDown = false;



        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = true;
        }
        private void Start()
        {
        }



        // Interface : IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            Player.One?.MoveTo(eventData.position);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (isDown)
            {
                isDown = false;
                Player.One?.StopMove();
            }
        }
        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (isDown)
                Player.One?.MoveTo(eventData.position);
        }
    }
}