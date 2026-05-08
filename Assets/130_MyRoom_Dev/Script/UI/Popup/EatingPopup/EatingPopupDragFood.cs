using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DoDoEng.MyRoom.UI.Popup
{
	public class EatingPopupDragFood : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
        public void Init(int index)
        {
            for (int i = 0; i < foods.Length; i++)
            {
                foods[i].SetActive(i == index);
            }
        }
        public void MoveTo(Vector2 ptScreen)
        {
            //LOG.Info($"MoveTo() | {ptScreen}", this);

            rt.anchoredPosition = UtilTransform.ScreenToLocal(ptScreen, rt.parent as RectTransform, GetComponentInParent<Canvas>());
        }

        // Events



        // Fields : caching
        private RectTransform rt_;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();

        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] foods = null;

        // Unity Messages
        private void Awake()
		{
        }
		private void Start()
		{
		}
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }

        // Unity Coroutine
	}
}