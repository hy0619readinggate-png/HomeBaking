using beyondi.Util;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Library.UI
{
	public class LibraryEBookStartPopupButton : MonoBehaviour
	{
        // Definitions
        // Properties
        public bool Checked
        {
            get => checkGO.activeSelf;
            set => checkGO.SetActive(value);
        }

        // Methods

        // Events
        public Action OnClick;



        // Fields : caching
        private Button button_;
        private Button button => button_ ??= GetComponent<Button>();
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject checkGO = null;

        // Unity Messages
        private void Awake()
		{
            checkGO.SetActive(false);
            button.onClick.AddListener(() => OnClick?.Invoke());
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}