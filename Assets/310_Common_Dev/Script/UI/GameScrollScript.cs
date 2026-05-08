using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng
{
	public class GameScrollScript : ScrollRect
    {
		// Definitions
		// Properties
		// Methods
		// Events



		// Fields : caching
		// Fields
		// Functions
		// Event Handlers
		// Overrides



		// Unity Inspectors
		//[Header("★ Bindings")]
		//[SerializeField] private TextMeshProUGUI labelTXT = null;
		//[Header("★ Config")]
		//[SerializeField] private int intValue = 0;

        bool forParent;
        ScrollRect parentScrollRect;


        protected override void Start()
        {
            parentScrollRect = GameObject.FindWithTag("GameScrollManager").GetComponent<ScrollRect>();
        }


        public override void OnBeginDrag(PointerEventData eventData)
        {
            // 드래그 시작하는 순간 수평이동이 크면 부모가 드래그 시작한 것, 수직이동이 크면 자식이 드래그 시작한 것
            if (vertical)
                forParent = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
            // 드래그 시작하는 순간 수직이동이 크면 부모가 드래그 시작한 것, 수평이동이 크면 자식이 드래그 시작한 것
            if (horizontal)
                forParent = Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x);

            if (forParent)
                parentScrollRect.OnBeginDrag(eventData);
            else base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (forParent)
                parentScrollRect.OnDrag(eventData);
            else base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (forParent)
                parentScrollRect.OnEndDrag(eventData);
            else base.OnEndDrag(eventData);
        }

    }
}