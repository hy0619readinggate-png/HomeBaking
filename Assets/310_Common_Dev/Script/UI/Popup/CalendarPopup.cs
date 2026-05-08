using beyondi.Util;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class CalendarPopup : PopupBase<SimplePopupResult>
	{
        // Definitions
        // Properties
        public DateTime CurrentDT => currentDT;

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(bool reward = false)
        {
            LOG.Function(this);

            this.reward = reward;
            isSelectable = false;

            AudioMGR.One.PlayEffect(popupCLIP);

            loadDay(DateTime.Today).Forget();

            return await showPopup();
        }
        public async UniTask<SimplePopupResult> ShowPopup(DateTime dt)
        {
            LOG.Function(this);

            isSelectable = true;

            AudioMGR.One.PlayEffect(popupCLIP);

            loadDay(dt).Forget();

            return await showPopup();
        }

        // Events



        // Fields : caching

        // Fields
        private DateTime currentDT = DateTime.Today;
        private bool isSelectable = false;
        private bool reward = false;

        // Functions
        private async UniTask loadDay(DateTime dt)
        {
            currentDT = dt;
            //dateTMP.text = $"{currentDT:Y}";
            dateTMP.text = currentDT.ToString("Y", LocalizationMGR.One.Culture);

            nextMonthBT.gameObject.SetActive(currentDT < DateTime.Today);

            days.ForEach(day => day.Init());
            weeks.ForEach(week => week.SetActive(false));

            var data = await LMS.One.LoadAttendanceCalendar(currentDT.ToString("yyyyMM"));
            if (data != null)
            {
                var continuousCount = data.Value<int>("continuousCount");
                var calendar = data.Value<JArray>("calendar");

                var currentDay = currentDT.FirstDayOfMonth();
                var idxStart = (int)currentDay.DayOfWeek;
                DateTime today = DateTime.Today;
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
                            //days[i].Init(idxDay + 1, currentDay.DayOfWeek, today == currentDay, isAttendance, isPraiseStamp ? (int)Enum.Parse<LMS.StampType>(praiseStampType) : -1);
                            days[i].Init(currentDay, today == currentDay, isAttendance, isPraiseStamp ? (int)Enum.Parse<LMS.StampType>(praiseStampType) : -1, isTodayLearningComplete, today == currentDay && reward);
                        }
                        else
                            //days[i].Init(idxDay + 1, currentDay.DayOfWeek, today == currentDay);
                            days[i].Init(currentDay, today == currentDay);

                        if (today == currentDay)
                        {
                            var firstDayOfMonth = today.FirstDayOfMonth();
                            int firstWeekDays = (firstDayOfMonth.LastDayOfWeek(DayOfWeek.Sunday).Day - firstDayOfMonth.Day + 1);
                            int numWeek = ((today.Day - 1 + (7 - firstWeekDays)) / 7) + 1;
                            weeks[numWeek - 1].SetActive(true);
                        }

                        currentDay = currentDay.AddDays(1);
                        if (currentDay.Month != currentDT.Month)
                            break;
                    }
                }
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
        private void todayBT_onClick()
        {
            LOG.Function(this);

            loadDay(DateTime.Today).Forget();
        }
        private void days_onClick(CalendarDay slot)
        {
            if (isSelectable)
            {
                if (slot.Day != 0 && slot.CurrentDT <= DateTime.Today)
                    currentDT = slot.CurrentDT;
                CloseWithResult(SimplePopupResult.Okay);
            }
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text dateTMP = null;
        [SerializeField] private Button prevMonthBT = null;
        [SerializeField] private Button nextMonthBT = null;
        [SerializeField] private Button todayBT = null;
        [SerializeField] private CalendarDay[] days = null;
        [SerializeField] private GameObject[] weeks = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        //[Header("★ Config")]
        //[SerializeField] private float buttonDelay = 0.5f;

        // Unity Messages
        private void Awake()
		{
            prevMonthBT.onClick.AddListener(() => prevMonthBT_onClick());
            nextMonthBT.onClick.AddListener(() => nextMonthBT_onClick());
            todayBT.onClick.AddListener(() => todayBT_onClick());
        }
		private void Start()
		{
        }
        protected void OnEnable()
        {
            days.ForEach(day => day.OnClick += days_onClick);
        }
        protected void OnDisable()
        {
            days.ForEach(day => day.OnClick -= days_onClick);
        }

        // Unity Coroutine
    }
}