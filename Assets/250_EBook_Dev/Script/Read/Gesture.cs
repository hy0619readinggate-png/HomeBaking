using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.EBook.Read
{
    public enum Swipe { Left, Right };

    public class Gesture : Graphic,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        // Methods
        public void Setup(EBookRead ebook)
        {
            LOG.Info($"Setup() | {ebook}", this);

            this.ebook = ebook;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            raycastTarget = enable;
        }

        // Events
        public event Action<Swipe> OnSwipe;



        // Fields
        private EBookRead ebook = null;
        private bool isSwipe = false;
        private Vector2 dragBeginPosition;

        // Functions
        private void setupMenuForPreventBeginDrag()
        {
            if (menu.GetComponent<EventTrigger>() == null)
            {
                // 메뉴가 활성화 되었을때, Swipe 제스쳐를 막아주기 위해
                var entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.BeginDrag;

                var trigger = menu.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Add(entry);
            }
        }

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



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Menu menu = null;
        [Header("★ Config")]
        [SerializeField] private float swipeDistance = 20;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            raycastTarget = false;
        }
        protected override void Start()
        {
            base.Start();

            setupMenuForPreventBeginDrag();
        }



        // Implementation Interface
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag() | {eventData.position}", this);

            isSwipe = false;
            dragBeginPosition = eventData.position;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!menu.IsLocked || !ebook.IsAutoMode)
            {
                var distH = eventData.position.x - dragBeginPosition.x;
                if (!isSwipe && Mathf.Abs(distH) > swipeDistance)
                {
                    isSwipe = true;
                    OnSwipe?.Invoke(distH < 0 ? Swipe.Left : Swipe.Right);
                }
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag() | {eventData.position}", this);

            isSwipe = false;
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown() | {eventData.position}", this);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp() | {eventData.position}", this);

            if (!isSwipe)
            {
                if (!menu.IsShown)
                    menu.ShowMenu();
                else menu.HideMenu();
            }
        }
    }
}