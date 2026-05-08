using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DoDoEng.Behaviour
{
	public class LibrarySubCategory : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Show(string[] names, int idxSelected)
		{
			LOG.Function(this, $"{names}");

            gameObject.SetActive(true);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (i < names.Length)
                {
                    buttons[i].Show(i, names[i]);
                    if (i == idxSelected) buttons[i].Select();
                }
                else
                    buttons[i].Hide();
            }
        }
        public void Hide()
        {
            LOG.Function(this);

            gameObject.SetActive(false);
        }

        // Events
        public Action<int> OnClick;



		// Fields : caching
		// Fields
        // Functions

        // Event Handlers
        private void button_onClick(LibrarySubCategoryButton button)
        {
            LOG.Function(this, $"{button}");
            OnClick?.Invoke(button.Index);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private LibrarySubCategoryButton[] buttons = null;
        //[Header("★ Config")]
        //[SerializeField] private int intValue = 0;

        // Unity Messages
        private void Awake()
		{
        }
		private void Start()
		{
        }
        private void OnEnable()
        {
            buttons.ForEach(button => button.OnSelect += button_onClick);
        }
        private void OnDisable()
        {
            buttons.ForEach(button => button.OnSelect -= button_onClick);
        }

        // Unity Coroutine
    }
}