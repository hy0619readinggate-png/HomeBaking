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
    public class CoursePopupContentDay : MonoBehaviour
    {
        // Definitions

        // Properties
        public int Num
        {
            get => int.Parse(numTMP.text);
            set => numTMP.text = value.ToString();
        }
        public bool Final
        {
            get => finalOff.activeSelf;
            set => finalOff.SetActive(value);
        }
        public bool IsReview
        {
            get => reviewOff.activeSelf;
            set => reviewOff.SetActive(value);
        }
        public bool IsComplete
        {
            get => completeGO.activeSelf;
            set
            {
                completeGO.SetActive(value);
                if (IsReview)
                {
                    reviewOn.SetActive(value);
                    normalOn.SetActive(false);
                }
                else
                {
                    reviewOn.SetActive(false);
                    normalOn.SetActive(value);
                }
                if (Final)
                {
                    finalOn.SetActive(value);
                    finalEff.SetActive(value);
                }
                else
                {
                    finalOn.SetActive(false);
                    finalEff.SetActive(false);
                }
            } 
        }
        public bool Current
        {
            set
            {
                if (!completeGO.activeSelf)
                {
                    if (IsReview)
                    {
                        reviewOn.SetActive(value);
                        normalOn.SetActive(false);
                    }
                    else
                    {
                        reviewOn.SetActive(false);
                        normalOn.SetActive(value);
                    }
                    if (Final)
                    {
                        finalOn.SetActive(value);
                        finalEff.SetActive(value);
                    }
                    else
                    {
                        finalOn.SetActive(false);
                        finalEff.SetActive(false);
                    }
                }
            }
        }

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
        public void DeSelect()
        {
            toggle.SetIsOnWithoutNotify(false);
        }
        public void SetInteract(bool interact)
        {
            toggle.interactable = interact;
            if (!reviewOn.activeSelf && !normalOn.activeSelf)
                toggle.interactable = false;
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
        [SerializeField] private GameObject normalOn = null;
        [SerializeField] private GameObject finalOff = null;
        [SerializeField] private GameObject finalOn = null;
        [SerializeField] private GameObject finalEff = null;
        [SerializeField] private GameObject reviewOff = null;
        [SerializeField] private GameObject reviewOn = null;
        [SerializeField] private GameObject completeGO = null;
        [SerializeField] private TMP_Text numTMP = null;

        // Unity Messages
        private void Awake()
		{
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((check) => toggle_onValueChanged(check));
        }
		private void Start()
		{  
		}
    }
}