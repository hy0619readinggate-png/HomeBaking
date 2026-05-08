using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;

namespace DoDoEng.Launcher.UI
{
	public class TabPage : MonoBehaviour
	{
        // Definitions

        // Properties
        public int Idx { get; set; }
        public bool IsNew { get; protected set; }

        // Methods
        public void Activate(bool active)
        {
            LOG.Function(this, $"| active={active}");

            gameObject.SetActive(active);
        }
        public void Load()
        {
            onLoad();
        }

        // Events
        public Action<TabPage> OnUpdate;



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers

        // Overrides
        protected virtual void onLoad() { }
        protected virtual void Awake()
        {
        }
        protected virtual void Start()
        {
        }
        protected virtual void OnEnable()
        {
        }
        protected virtual void OnDisable()
        {
        }



        // Unity Inspectors
        // Unity Messages
        // Unity Coroutine
    }
}