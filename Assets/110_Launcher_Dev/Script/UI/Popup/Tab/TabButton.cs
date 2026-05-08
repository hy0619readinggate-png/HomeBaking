using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DoDoEng.Launcher.UI
{
	public class TabButton : MonoBehaviour
	{
        // Definitions
        // Properties
        public bool IsNew { get => newGO.activeSelf; set => newGO.SetActive(value); }

        // Methods
        public void Init(int idx, bool isNew = false)
        {
            this.idx = idx;

            if (newGO != null)
                newGO.SetActive(isNew);
        }
        public void SetIsOnWithoutNotify(bool value)
        {
            toggle.SetIsOnWithoutNotify(value);
        }

        // Events
        public Action<int> OnSelect;



        // Fields : caching
        private Toggle toggle_;
        private Toggle toggle => toggle_ ??= GetComponent<Toggle>();

        // Fields
        private int idx;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private GameObject newGO = null;

        // Unity Messages
        private void Awake()
		{
            toggle.onValueChanged.AddListener(value =>
            {
                if (value) OnSelect?.Invoke(idx);
            });
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}