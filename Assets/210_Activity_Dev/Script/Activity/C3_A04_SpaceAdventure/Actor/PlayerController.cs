using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C3_A04
{
    public class PlayerController : Graphic,
        IPointerDownHandler, IPointerUpHandler
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            raycastTarget = enable;

            isDown = false;
        }



        // Fields
        private bool isDown = false;

        // Overrides
        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            // https://younitystudy.tistory.com/m/75
            return true;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }


        [Header("¡Ú Bindings")]
        [SerializeField] private SpaceShip spaceShip = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            raycastTarget = false;
        }
        protected override void Start()
        {
            base.Start();
        }
        private void Update()
        {
            if (isDown)
            {
                var worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                spaceShip.MoveTo(worldPosition);
            }
        }



        // Interface : IPointerDownHandler, IPointerUpHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!isDown)
            {
                var worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                spaceShip.StartMoving(worldPosition);
                isDown = true;
            }
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (isDown)
                spaceShip.StopMoving();
            isDown = false;
        }
    }
}