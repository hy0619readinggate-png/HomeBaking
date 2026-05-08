using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A03
{
    public class DropArea : MonoBehaviour, IDropHandler
    {
        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop()", this);

            var example = eventData.pointerDrag.GetComponent<ExamplePart>();
            LOG.Assert(example != null, $"pointerDrag must be ExamplePart", this);

            if (example != null)
                eventData.Use();
        }
    }
}