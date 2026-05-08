using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
	public class ParentChildLMSPopupPageBase : TabPage
    {
        // Definitions
        // Properties
        public UserDataChild ChildData => parentChildLMSPopup.ChildData;

        // Methods
        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private ParentChildLMSPopup parentChildLMSPopup = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        // Unity Coroutine
    }
}