using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
    [RequireComponent(typeof(Toggle))]
    public class CoursePopupContentStage : MonoBehaviour
    {
        // Definitions

        // Properties
        public int Num
        {
            get => int.Parse(numTMP.text);
            set => numTMP.text = value.ToString();
        }
        public bool IsFirstStage
        {
            set
            {
                stageGO.SetActive(value);
            }
        }
        public Slider Slider => slider;

        // Methods
        public void Activate(bool active)
        {
            LOG.Function(this, $"{active}");

            gameObject.SetActive(active);
        }
        public void Select(bool withoutNotify = false)
        {
            if (withoutNotify) toggle.SetIsOnWithoutNotify(true);
            else toggle.isOn = true;
        }
        public void SetInteract(bool interact)
        {
            toggle.interactable = interact;
            toggle.SetIsOnWithoutNotify(false);
        }

        // Events
        public Action<int> OnSelect;



        // Fields : caching
        private Toggle toggle_;
        private Toggle toggle => toggle_ ??= GetComponent<Toggle>();

        // Fields
        // Functions

        // Event Handlers
        private void toggle_onValueChanged(bool check)
        {
            if (check) OnSelect?.Invoke(Num);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject stageGO = null;
        [SerializeField] private TMP_Text numTMP = null;
        [SerializeField] private Slider slider = null;

        // Unity Messages
        private void Awake()
		{
            toggle.onValueChanged.AddListener((check) => toggle_onValueChanged(check));
        }
		private void Start()
		{  
		}
    }
}