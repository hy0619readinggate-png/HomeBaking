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
	public class TempGuidePopup : MonoBehaviour, ISelectHandler, IDeselectHandler
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

        [SerializeField] GameObject targerPopup;

        // Unity Messages


        private void Awake()
		{
            targerPopup.SetActive(false);
        }
		private void Start()
		{
            
        }

        // Unity Coroutine
        public void OnSelect(BaseEventData eventData)
        {
            //throw new System.NotImplementedException();
           //Debug.Log("클릭했당!!");
            targerPopup.SetActive(true);
        }

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            //throw new System.NotImplementedException();
            //Debug.Log("임의의 클릭했당!!");
            targerPopup.SetActive(false);
        }
    }
}