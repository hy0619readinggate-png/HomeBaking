using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

namespace DoDoEng.Launcher.UI
{
	public class ParentChildLMSPopupPageMonth : ParentChildLMSPopupPageBase
    {
        // Definitions

        // Properties
        public string CurrentDate => currentDT.ToString("yyyyMMdd");

        // Methods

        // Events
        public Action<string> OnChangeDay;



        // Fields : caching

        // Fields
        DateTime currentDT = DateTime.Today;

        // Functions
        private void loadDay(DateTime dt)
        {
            currentDT = dt;
            monthTMP.text = $"{currentDT.ToString("MMMM", LocalizationMGR.One.Culture)}";

            nextMonthBT.gameObject.SetActive(currentDT.FirstDayOfMonth() < DateTime.Today.FirstDayOfMonth());

            OnChangeDay?.Invoke(CurrentDate);
        }

        // Event Handlers
        private async UniTask calendarBT_onClick()
        {
            LOG.Function(this);

            var result = await SystemUI.One.CalendarPU.ShowPopup(currentDT);
            if (result == SimplePopupResult.Okay)
                loadDay(SystemUI.One.CalendarPU.CurrentDT);
        }
        private void prevMonthBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddMonths(-1));
        }
        private void nextMonthBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddMonths(1));
        }
        private void thisMonthBT_onClick()
        {
            LOG.Function(this);

            loadDay(DateTime.Today);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text monthTMP = null;
        [SerializeField] private Button calendarBT = null;
        [SerializeField] private Button prevMonthBT = null;
        [SerializeField] private Button nextMonthBT = null;
        [SerializeField] private Button thisMonthBT = null;
        [SerializeField] private TabControl tabControl = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            calendarBT.onClick.AddListener(() => calendarBT_onClick().Forget());
            prevMonthBT.onClick.AddListener(() => prevMonthBT_onClick());
            nextMonthBT.onClick.AddListener(() => nextMonthBT_onClick());
            thisMonthBT.onClick.AddListener(() => thisMonthBT_onClick());
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            loadDay(DateTime.Today);
            tabControl.Select(0);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}