using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
    public class MyInfoPopupPageAttendance : TabPage
    {
        // Definitions
        // Properties
        // Methods
        // Events



        // Fields : caching

        // Fields
        DateTime currentDT = DateTime.Today;

        // Functions
        private async UniTask loadDay(DateTime dt)
        {
            currentDT = dt;
            monthTMP.text = $"{currentDT.ToString("MMM", LocalizationMGR.One.Culture)}";

            nextMonthBT.gameObject.SetActive(currentDT < DateTime.Today);

            days.ForEach(day => day.Init());
            progressSlider.value = 0;

            var data = await LMS.One.LoadAttendanceCalendar(currentDT.ToString("yyyyMM"));
            if (data != null)
            {
                var continuousCount = data.Value<int>("continuousCount");
                var calendar = data.Value<JArray>("calendar");

                var currentDay = currentDT.FirstDayOfMonth();
                var idxStart = (int)currentDay.DayOfWeek;
                for (int i = 0; i < days.Length; i++)
                {
                    if (i >= idxStart)
                    {
                        int idxDay = i - idxStart;
                        if (idxDay < calendar.Count)
                        {
                            var isAttendance = calendar[idxDay].Value<bool>("isAttendance");
                            var isPraiseStamp = calendar[idxDay].Value<bool>("isPraiseStamp");
                            var praiseStampType = calendar[idxDay].Value<string>("praiseStampType");
                            var isTodayLearningComplete = calendar[idxDay].Value<bool>("isTodayLearningComplete");
                            days[i].Init(idxDay + 1, currentDay.DayOfWeek, DateTime.Today == currentDay, isAttendance, isPraiseStamp ? (int)Enum.Parse<LMS.StampType>(praiseStampType) : -1, isTodayLearningComplete);
                        }
                        else
                            days[i].Init(idxDay + 1, currentDay.DayOfWeek, DateTime.Today == currentDay);

                        currentDay = currentDay.AddDays(1);
                        if (currentDay.Month != currentDT.Month)
                            break;
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    attendanceCoins[i].Enabled = i < continuousCount;
                }
                progressSlider.value = continuousCount * 0.2f;
            }
        }

        // Event Handlers
        private void prevMonthBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddMonths(-1)).Forget();
        }
        private void nextMonthBT_onClick()
        {
            LOG.Function(this);

            loadDay(currentDT.AddMonths(1)).Forget();
        }
        private async UniTask calendarBT_onClick()
        {
            LOG.Function(this);

            var result = await SystemUI.One.CalendarPU.ShowPopup(currentDT);
            if (result == SimplePopupResult.Okay)
                await loadDay(SystemUI.One.CalendarPU.CurrentDT);
        }
        private void todayBT_onClick()
        {
            LOG.Function(this);

            loadDay(DateTime.Today).Forget();
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text monthTMP = null;
        [SerializeField] private Button prevMonthBT = null;
        [SerializeField] private Button nextMonthBT = null;
        [SerializeField] private Button calendarBT = null;
        [SerializeField] private Button todayBT = null;
        [SerializeField] private Slider progressSlider = null;
        [SerializeField] private CalendarDay[] days = null;
        [SerializeField] private AttendanceCoin[] attendanceCoins = null;

        // Unity Messages
        protected override void Awake()
		{
            base.Awake();

            prevMonthBT.onClick.AddListener(() => prevMonthBT_onClick());
            nextMonthBT.onClick.AddListener(() => nextMonthBT_onClick());
            calendarBT.onClick.AddListener(() => calendarBT_onClick().Forget());
            todayBT.onClick.AddListener(() => todayBT_onClick());
        }
        protected override void Start()
		{
            base.Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();

            loadDay(DateTime.Today).Forget();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
        }

        // Unity Coroutine
    }
}