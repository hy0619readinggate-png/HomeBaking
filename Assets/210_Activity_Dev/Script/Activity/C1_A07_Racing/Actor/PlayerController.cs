using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A07
{
    public class PlayerController : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
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

        // Functions
        private void moveEdmond(Vector2 position)
        {
            var laneNo = Track.One.GetLaneNo(position);

            if (laneNo != -1)
                edmond.MoveTo(laneNo);
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Edmond edmond = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }

        // Interface : IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            isDown = true;
            moveEdmond(eventData.position);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            isDown = false;
        }
        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            if (isDown)
                moveEdmond(eventData.position);
        }
    }
}