using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DoDoEng.Launcher.UI
{
	public class CoursePopupTab : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Activate(bool active)
        {
            //LOG.Function(this, $"{active}");

            activeGO.SetActive(active);
            deactiveGO.SetActive(!active);
        }
        public void Init(int idx)
        {
            this.idx = idx;
        }
        public void SetComplete(bool isComplete)
        {
            completeGO.ForEach(go => go.SetActive(isComplete));
        }
        public void Interactable(bool interactable)
        {
            button.interactable = interactable;
        }

        // Events
        public Action<int> OnClick;



        // Fields : caching
        private Button button_;
        private Button button => button_ ??= GetComponent<Button>();

        // Fields
        private int idx;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private GameObject activeGO = null;
		[SerializeField] private GameObject deactiveGO = null;
        [SerializeField] private GameObject[] completeGO = null;

        // Unity Messages
        private void Awake()
		{
            activeGO.SetActive(false);
            deactiveGO.SetActive(true);
            completeGO.ForEach(go => go.SetActive(false));

            button.onClick.AddListener(() => OnClick?.Invoke(idx));
        }
		private void Start()
		{
		   
		}

		// Unity Coroutine
	}
}