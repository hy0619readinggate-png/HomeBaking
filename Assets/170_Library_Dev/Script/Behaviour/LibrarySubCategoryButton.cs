using beyondi.Util;
using System.Linq;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace DoDoEng.Behaviour
{
	public class LibrarySubCategoryButton : MonoBehaviour
	{
		// Definitions
		// Properties
		public int Index { get; set; }

        // Methods
        public void Show(int index, string name)
		{
			LOG.Function(this, $"{index} | {name})");

            gameObject.SetActive(true);

			Index = index;
            nameTMP.text = name;
        }
		public void Hide()
		{
			LOG.Function(this);

            gameObject.SetActive(false);
        }
		public void Select()
		{
			toggle.SetIsOnWithoutNotify(true);
        }

		// Events
		public Action<LibrarySubCategoryButton> OnSelect;



		// Fields : caching
		private Toggle toggle_;
		private Toggle toggle => toggle_ ??= GetComponent<Toggle>();

		// Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private TMP_Text nameTMP = null;
		[SerializeField] private GameObject newGO = null;
        //[Header("★ Config")]
        //[SerializeField] private int intValue = 0;

        // Unity Messages
        private void Awake()
		{
			newGO.SetActive(false);
            toggle.onValueChanged.AddListener((check) =>
			{
				if (check) OnSelect?.Invoke(this);
			});
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}