using beyondi.Behaviour;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI.Extensions;

namespace DoDoEng.Game.C1_G03
{
    [RequireComponent(typeof(NonDrawingGraphic))]
    public class BlockDropZone : BYDSingleton<BlockDropZone>, IDropHandler
    {
        // Properties
        public bool EnableDrop
        {
            get => ndg.raycastTarget;
            set => ndg.raycastTarget = value;
        }



        // Fields
        private NonDrawingGraphic ndg_ = null;
        private NonDrawingGraphic ndg => ndg_ ??= GetComponent<NonDrawingGraphic>();



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            ndg.raycastTarget = false;
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Function(this);

            var roadBlock = eventData.pointerDrag.GetComponent<RoadBlock>();
            if (roadBlock != null)
            {
                eventData.Use();
            }
        }
    }
}