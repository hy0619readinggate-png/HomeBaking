using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using System;
using DoDoEng.Common;

namespace DoDoEng.Launcher.UI
{
    public class TabControl : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Select(int idx)
        {
            LOG.Function(this, $"| idx={idx}");

            for (int i = 0; i < buttons.Length; i++)
                buttons[i].SetIsOnWithoutNotify(i == idx);
            for (int i = 0; i < pages.Length; i++)
                pages[i].Activate(i == idx);
            OnSelect?.Invoke(idx);
        }
        public void LoadAllPages()
        {
            pages.ForEach(page => page.Load());
        }

        // Events
        public Action<int> OnSelect;



        // Fields : caching
        // Fields
        // Functions

        // Event Handlers
        private void button_onSelect(int idx)
        {
            LOG.Function(this, $"| idx={idx}");

            Select(idx);
        }
        private void page_onUpdate(TabPage page)
        {
            buttons[page.Idx].IsNew = page.IsNew;
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TabButton[] buttons = null;
		[SerializeField] private TabPage[] pages = null;

        // Unity Messages
        private void Awake()
		{
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Init(i, false);
                pages[i].Idx = i;
            }
        }
		private void Start()
		{
        }
        protected void OnEnable()
        {
            buttons.ForEach(button => button.OnSelect += button_onSelect);
            pages.ForEach(page => page.OnUpdate += page_onUpdate);
        }
        protected void OnDisable()
        {
            buttons.ForEach(button => button.OnSelect -= button_onSelect);
            pages.ForEach(page => page.OnUpdate -= page_onUpdate);
        }

        // Unity Coroutine
    }
}