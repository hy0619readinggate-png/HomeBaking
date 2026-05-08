using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using System;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace DoDoEng.MyRoom.Behavior
{
    public class ScrollButton : MonoBehaviour
	{
        // Definitions
        // Properties
        // Methods
        // Events



        // Fields : caching

        // Fields
        private bool isDown;

        // Functions
        // Event Handlers

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Scrollbar scrollbar = null;
        [Header("★ Config")]
        [SerializeField] private float amount = 0.5f;

        // Unity Messages
        private void Awake()
		{
            var entryDown = new EventTrigger.Entry();
            entryDown.eventID = EventTriggerType.PointerDown;
            entryDown.callback.AddListener((data) => { isDown = true; });

            var entryUp = new EventTrigger.Entry();
            entryUp.eventID = EventTriggerType.PointerUp;
            entryUp.callback.AddListener((data) => { isDown = false; });

            var trigger = gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Add(entryDown);
            trigger.triggers.Add(entryUp);
        }
		private void Start()
		{
        }
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }
        private void Update()
        {
            if (isDown)
            {
                scrollbar.value += Time.deltaTime * amount;
            }
        }

        // Unity Coroutine
    }
}