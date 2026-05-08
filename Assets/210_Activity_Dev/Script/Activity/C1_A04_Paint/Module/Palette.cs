using beyondi.Util;
using DoDoEng.Common;
using System;
using UnityEngine;

namespace DoDoEng.Activity.C1_A04
{
    [RequireComponent(typeof(Animator))]
    public class Palette : MonoBehaviour
    {
        // Properties
        public ToolType ToolType => toolType;
        public bool IsSelected { get; private set; }
        public bool IsShown { get; private set; }
        public PaletteItem SelectedItem { get; private set; }

        // Methods
        public void Show()
        {
            LOG.Info($"Show()", this);

            if (!IsShown)
            {
                transform.SetAsLastSibling();
                IsShown = true;
                anim.SetTrigger("appear");
            }
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            if (IsShown)
            {
                IsShown = false;
                anim.SetTrigger("disappear");
            }
        }

        // Events
        public event Action<PaletteItem> OnItemSelected;



        // Fields : caching
        private Animator anim => GetComponent<Animator>();
        private PaletteItem[] items_ = null;
        private PaletteItem[] items => items_ ??= GetComponentsInChildren<PaletteItem>(true);

        // Event Handlers
        private void item_OnSelected(PaletteItem item)
        {
            LOG.Info($"item_OnSelected() | {item}", this);

            SelectedItem = item;
            OnItemSelected?.Invoke(SelectedItem);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private ToolType toolType = ToolType.NA;

        // Unity Messages
        private void Awake()
        {
            SelectedItem = items[0];
            SelectedItem.Selected = true;
        }
        private void Start()
        {

        }
        private void OnEnable()
        {
            IsSelected = false;
            items.ForEach(i => i.OnSelected += item_OnSelected);
        }
        private void OnDisable()
        {
            items.ForEach(i => i.OnSelected -= item_OnSelected);
        }
    }
}