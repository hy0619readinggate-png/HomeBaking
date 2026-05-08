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
	public class ParentChildLMSPopupPageWeek : ParentChildLMSPopupPageBase
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
            var firstDayOfWeek = currentDT.FirstDayOfWeek(DayOfWeek.Sunday);
            var lastDayOfWeek = currentDT.LastDayOfWeek(DayOfWeek.Sunday);
            weekTMP.text = $"{firstDayOfWeek.ToString("MM\\/dd")} ~ {lastDayOfWeek.ToString("MM\\/dd")}";

            nextWeekBT.gameObject.SetActive(currentDT.FirstDayOfWeek(DayOfWeek.Sunday) < DateTime.Today.FirstDayOfWeek(DayOfWeek.Sunday));

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
        private void prevWeekBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddDays(-7));
        }
        private void nextWeekBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddDays(7));
        }
        private void thisWeekBT_onClick()
        {
            LOG.Function(this);

            loadDay(DateTime.Today);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text weekTMP = null;
        [SerializeField] private Button calendarBT = null;
        [SerializeField] private Button prevWeekBT = null;
        [SerializeField] private Button nextWeekBT = null;
        [SerializeField] private Button thisWeekBT = null;
        [SerializeField] private TabControl tabControl = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            calendarBT.onClick.AddListener(() => calendarBT_onClick().Forget());
            prevWeekBT.onClick.AddListener(() => prevWeekBT_onClick());
            nextWeekBT.onClick.AddListener(() => nextWeekBT_onClick());
            thisWeekBT.onClick.AddListener(() => thisWeekBT_onClick());
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