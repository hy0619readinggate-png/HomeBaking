using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class ToggleButton : MonoBehaviour
    {
        // Properties
        public bool IsOn
        {
            get => isOn;
            set => switchValue(value, false);
        }
        public bool Interactable
        {
            get => onBTN.interactable;
            set
            {
                onBTN.interactable = value;
                offBTN.interactable = value;
            }
        }

        // Events
        public event Action<bool> OnValueChanged;



        // Fields
        private bool isOn;

        // Functions
        private void switchValue(bool on, bool notify = true)
        {
            isOn = on;

            onBTN.gameObject.SetActive(on == false);
            offBTN.gameObject.SetActive(on == true);

            if (notify)
                OnValueChanged?.Invoke(on);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button onBTN = null;
        [SerializeField] private Button offBTN = null;

        // Unity Messages
        private void Awake()
        {

            LOG.Function(this);
            switchValue(true, false);

            onBTN.onClick.AddListener(() => switchValue(true));
            offBTN.onClick.AddListener(() => switchValue(false));
        }
        private void Start()
        {
        }
    }
}