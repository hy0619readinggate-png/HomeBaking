using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using TMPro;

namespace DoDoEng.Common
{
	public class CoinHistoryLine : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Init(string title, int income = 0, int outcome = 0)
        {
            titleTMP.text = title;
            incomeTMP.text = $"+{income}";
            outcomeTMP.text = $"{outcome}";

            incomeTMP.gameObject.SetActive(income > 0);
            outcomeTMP.gameObject.SetActive(outcome < 0);
        }

        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text titleTMP = null;
        [SerializeField] private TMP_Text incomeTMP = null;
        [SerializeField] private TMP_Text outcomeTMP = null;

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