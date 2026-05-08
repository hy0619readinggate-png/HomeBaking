using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DoDoEng.Common;
using DoDoEng.Game.C1_G03;

namespace DoDoEng.Launcher.UI
{
	public class LMSProgressbar2 : MonoBehaviour
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
                valueTMP.text = LocalizationMGR.One.GetText("WORD_122", value);
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