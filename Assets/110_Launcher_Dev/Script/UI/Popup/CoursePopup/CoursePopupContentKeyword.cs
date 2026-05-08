using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace DoDoEng.Launcher.UI
{
	public class CoursePopupContentKeyword : MonoBehaviour
	{
        // Definitions
        // Properties
        public string Text
        {
            get => keywordTMP.text;
            set => keywordTMP.text = value;
        }

        // Methods
        public void Activate(bool active)
        {
            //LOG.Function(this, $"{active}");

            gameObject.SetActive(active);
        }

        // Events



        // Fields : caching

        // Fields

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private TMP_Text keywordTMP = null;

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