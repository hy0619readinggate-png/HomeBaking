using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using System;
using TMPro;
using DoDoEng.Common;

namespace DoDoEng.Launcher.UI
{
	public class ContinuousAttendSlot : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Init(int value = 1, bool isDone = false, bool isCurrent = false, bool isRed = false)
        {
            LOG.Function(this, $"| value={value} | isDone={isDone} | isCurrent={isCurrent} | isRed={isRed}");

            redGO.SetActive(isRed);
            currentGO.SetActive(isCurrent);
            effANI.SetBool("eff", isCurrent);
            valueTMP.text = $"x{value}";
            checkedGO.SetActive(isDone);
            if (isCurrent) checkedANI.SetTrigger("stamp");
        }

        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject redGO = null;
        [SerializeField] private GameObject currentGO = null;
        [SerializeField] private Animator effANI = null;
        [SerializeField] private TMP_Text valueTMP = null;
        [SerializeField] private GameObject checkedGO = null;
        [SerializeField] private Animator checkedANI = null;

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