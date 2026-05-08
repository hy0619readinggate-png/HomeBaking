using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace DoDoEng.MyRoom.Behavior
{
    [RequireComponent(typeof(CanvasRenderer))]
    [RequireComponent(typeof(Animator))]
	public class MyRoomActivityInteractable : Graphic, IPointerDownHandler
    {
        // Definitions
        // Properties
        // Methods



        // Fields : caching
        private Animator animator_;
        private Animator animator => animator_ ??= GetComponent<Animator>();

        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        //[Header("★ Bindings")]
        //[SerializeField] private GameObject activityFX = null;

        // Unity Messages
        protected override void Awake()
		{
            base.Awake();
        }
        protected override void Start()
		{
            base.Start();
		}

        // Unity Coroutine



        // Implementation Interface
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Function(this, $"{eventData.position}");

            animator.SetTrigger("interaction");
        }

    }
}