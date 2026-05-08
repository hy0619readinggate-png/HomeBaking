using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using DoDoEng.Playground.Behaviour;

namespace DoDoEng.Launcher.UI
{
	public class LMSPlaygroundSlot : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Init(int course, int num, int stars, bool isComplete)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (i == course - 1)
                {
                    slots[i].gameObject.SetActive(true);
                    slots[i].InitForLMS(num, stars, isComplete);
                }
                else
                {
                    slots[i].gameObject.SetActive(false);
                }
            }
        }

        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlaygroundSlot[] slots = null;

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