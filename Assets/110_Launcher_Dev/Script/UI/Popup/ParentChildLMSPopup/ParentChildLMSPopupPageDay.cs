using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;
using System.Globalization;
using Cysharp.Threading.Tasks;

namespace DoDoEng.Launcher.UI
{
	public class ParentChildLMSPopupPageDay : ParentChildLMSPopupPageBase
    {
        // Definitions

        // Properties
        public string CurrentDate => currentDT.ToString("yyyyMMdd");

        // Methods
        public void Jump(int page, DateTime date)
        {
            LOG.Function(this, $"| page={page} | date={date}");
            idxJump = page;
            dateJump = date;
        }

        // Events
        public Action<string> OnChangeDay;



        // Fields : caching

        // Fields
        DateTime currentDT = DateTime.Today;
        private int idxJump = -1;
        private DateTime dateJump = DateTime.Today;

        // Functions
        private void loadDay(DateTime dt)
        {
            currentDT = dt;
            UserData.One.LastReportDate = currentDT;
            dayTMP.text = currentDT.ToString("MM\\/dd (ddd)", LocalizationMGR.One.Culture);

            nextDayBT.gameObject.SetActive(currentDT < DateTime.Today);

            //OnChangeDay?.Invoke(CurrentDate);
            tabControl.LoadAllPages();
        }

        // Event Handlers
        private async UniTask calendarBT_onClick()
        {
            LOG.Function(this);

            var result = await SystemUI.One.CalendarPU.ShowPopup(currentDT);
            if (result == SimplePopupResult.Okay)
                loadDay(SystemUI.One.CalendarPU.CurrentDT);
        }
        private void prevDayBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddDays(-1));
        }
        private void nextDayBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddDays(1));
        }
        private void todayBT_onClick()
        {
            LOG.Function(this);

            loadDay(DateTime.Today);
        }
        private void tabControl_onSelect(int idx)
        {
            LOG.Function(this, $"| idx={idx}");
            UserData.One.LastReportPage = idx;
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text dayTMP = null;
        [SerializeField] private Button calendarBT = null;
        [SerializeField] private Button prevDayBT = null;
        [SerializeField] private Button nextDayBT = null;
        [SerializeField] private Button todayBT = null;
        [SerializeField] private TabControl tabControl = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            calendarBT.onClick.AddListener(() => calendarBT_onClick().Forget());
            prevDayBT.onClick.AddListener(() => prevDayBT_onClick());
            nextDayBT.onClick.AddListener(() => nextDayBT_onClick());
            todayBT.onClick.AddListener(() => todayBT_onClick());
        }
        protected override void Start()
        {
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            if (idxJump != -1)
            {
                loadDay(dateJump);
                tabControl.Select(idxJump);
                idxJump = -1;
            }
            else
            {
                loadDay(DateTime.Today);
                tabControl.Select(0);
                tabControl.LoadAllPages();
            }

            tabControl.OnSelect += tabControl_onSelect;
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            tabControl.OnSelect -= tabControl_onSelect;
        }

        // Unity Coroutine
    }
}