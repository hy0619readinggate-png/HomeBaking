using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DoDoEng.Common;
using Cysharp.Threading.Tasks;

#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
    public class LMSStampControl : MonoBehaviour
    {
        // Definitions
        // Properties

        // Methods
        public void SetStamp(int stamp, bool isAttendance = true)
        {
            LOG.Function(this, $"| stamp={stamp}, isAttendance={isAttendance}");

            this.isAttendance = isAttendance;

            if (popup != null)
                popup.SetActive(false);
            for (int i = 0; i < stamps.Length; i++)
            {
                stamps[i].SetActive(i == stamp - 1);
            }
        }
        public bool Interactable
        {
            get => stampBT.interactable;
            set => stampBT.interactable = value;
        }

        // Events
        public Action<int> OnStamp;



        // Fields : caching

        // Fields
        private bool isAttendance;

        // Functions

        // Event Handlers
        private void stampBT_onClick()
        {
            LOG.Function(this);

            if (popup.activeSelf)
                popup.SetActive(false);
            else if (!stamps.Any(stamp => stamp.activeSelf))
            {
                if (isAttendance)
                {
                    popup.SetActive(true);
                }
                else
                {
                    SystemUI.One.ShowPopupCannotStamp().Forget();
                }
            }
        }
        private void button_onClick(Button button)
        {
            LOG.Function(this, $"| button={button}");

            for (int i = 0; i < buttons.Length; i++)
            {
                if (button == buttons[i])
                {
                    SetStamp(i + 1);
                    popup.SetActive(false);
                    OnStamp?.Invoke(i + 1);
                    break;
                }
            }
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button stampBT = null;
        [SerializeField] private GameObject[] stamps = null;
        [SerializeField] private GameObject popup = null;
        [SerializeField] private Button[] buttons = null;

        // Unity Messages
        private void Awake()
		{
            if (stampBT != null) stampBT.onClick.AddListener(() => stampBT_onClick());
            buttons.ForEach(button => button.onClick.AddListener(() => button_onClick(button)));

            if (popup != null) popup.SetActive(false);
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}