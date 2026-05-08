using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DoDoEng.Common;

#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
	public class LMSLearningCategory : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Set(string name, int accumulate, int percent)
        {
            nameTMP.text = name;

            gaugeRT.anchoredPosition = new Vector2(posStart + (accumulate / 100f * maxSize), gaugeRT.anchoredPosition.y);
            gaugeRT.sizeDelta = new Vector2(percent / 100f * maxSize, gaugeRT.sizeDelta.y);

            percentTMP.text = $"{percent}%";

            messageGO.SetActive(false);
        }

        // Events



        // Fields : caching

        // Fields
        private float posStart;
        private float maxSize;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text nameTMP = null;
        [SerializeField] private RectTransform gaugeRT = null;
        [SerializeField] private TMP_Text percentTMP = null;
        [SerializeField] private GameObject messageGO = null;
        [SerializeField] private TMP_Text messageTMP = null;

        // Unity Messages
        private void Awake()
		{
            posStart = gaugeRT.anchoredPosition.x;
            maxSize = gaugeRT.sizeDelta.x;
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}