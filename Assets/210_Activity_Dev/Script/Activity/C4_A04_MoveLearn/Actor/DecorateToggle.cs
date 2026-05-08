using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C4_A04
{
    public class DecorateToggle : MonoBehaviour
    {
        // Properties
        public string Color => color;

        // Events
        public event Action<DecorateToggle> OnSelected;



        // Fields : caching
        private Toggle toggle_ = null;
        private Toggle toggle => toggle_ ??= GetComponent<Toggle>();

        // Event Handlers
        private void toggle_onValueChanged(bool isOn)
        {
            if (isOn)
                OnSelected?.Invoke(this);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private string color = "";

        // Unity Messages
        private void Awake()
        {
            toggle.isOn = false;
            toggle.onValueChanged.AddListener(toggle_onValueChanged);
        }
        private void Start()
        {
        }
    }
}