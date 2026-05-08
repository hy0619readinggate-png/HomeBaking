using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace DoDoEng.Launcher.UI
{
	public class LMSProgressbar : MonoBehaviour
	{
        // Definitions
        // Properties
        public int Max { get; set; } = 1;
        public int Value
        {
            get => value;
            set 
            {
                this.value = value;
                slider.value = value / (float)Max;
                valueTMP.text = $"{value}";
            }
        }
        public bool IsToday
        { 
            set
            {
                otherDayGO.SetActive(!value);
                todayGO.SetActive(value);
            } 
        }

        // Methods
        // Events



        // Fields : caching
        private Slider slider_ = null;
        private Slider slider => slider_ ??= GetComponent<Slider>();

        // Fields
        private int value;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject otherDayGO = null;
        [SerializeField] private GameObject todayGO = null;
        [SerializeField] private TMP_Text valueTMP = null;

        // Unity Messages
        private void Awake()
		{
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}