using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace DoDoEng.Behaviour
{
	public class LibraryCategoryButton : MonoBehaviour
	{
		// Definitions
		// Properties
		public int Index => index;
		public Sprite Thumbnail => categoryIMG.sprite;
		public bool Enabled
		{
			get { return toggle.enabled; }
			set { toggle.enabled = value; }
		}
		public void Select(bool value = true)
		{
			toggle.SetIsOnWithoutNotify(value);
		}

		// Methods
		public void Init(int id, Sprite image)
		{
			LOG.Function(this, $"| index={id}, image={image}");

			this.index = id;
            categoryIMG.sprite = image;

            gameObject.SetActive(true);
        }

		// Events
		public Action<LibraryCategoryButton> OnClick;



		// Fields : caching
		private Toggle toggle_;
		private Toggle toggle => toggle_ ??= GetComponent<Toggle>();

		// Fields
		private int index;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private Image categoryIMG = null;
        //[Header("★ Config")]
        //[SerializeField] private int intValue = 0;

        // Unity Messages
        private void Awake()
		{
			toggle.onValueChanged.AddListener((value) =>
			{
				if (value) OnClick?.Invoke(this);
			});
        }
		private void Start()
		{
		   
		}

		// Unity Coroutine
	}
}