using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Common
{
    public class CalendarDay : MonoBehaviour
	{
        // Definitions

        // Properties
        public DateTime CurrentDT => currentDT;
        public int Day => day;

        // Methods
        public void Init(DateTime dt, bool isToday = false, bool isAttend = false, int numStamp = -1, bool isComplete = false, bool stampAni = false)
        {
            currentDT = dt;
            Init(dt.Day, dt.DayOfWeek, isToday, isAttend, numStamp, isComplete);
            //if (isToday && isAttend && numStamp == -1)
            if (stampAni)
                stickerAni.SetTrigger("stamp");
        }
        public void Init(int day = 0, DayOfWeek dayOfWeek = DayOfWeek.Monday, bool isToday = false, bool isAttend = false, int numStamp = -1, bool isComplete = false)
        {
            LOG.Function(this, $"| day={day} | dayOfWeek={dayOfWeek} | isToday={isToday} | isAttend={isAttend} | numStamp={numStamp} | isComplete={isComplete}");

            this.day = day;

            dayTMP.text = day.ToString();
            daySatTMP.text = day.ToString();
            daySunTMP.text = day.ToString();

            dayTMP.gameObject.SetActive(day > 0 && dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday);
            daySatTMP.gameObject.SetActive(day > 0 && dayOfWeek == DayOfWeek.Saturday);
            daySunTMP.gameObject.SetActive(day > 0 && dayOfWeek == DayOfWeek.Sunday);

            todayGO.SetActive(isToday);
            attendGO.SetActive(isAttend && numStamp == -1);
            for (int i = 0; i < stickers.Length; i++)
            {
                stickers[i].SetActive(i == numStamp);
            }
            completeGo.SetActive(isComplete);
        }

        // Events
        public Action<CalendarDay> OnClick;



        // Fields : caching

        // Fields
        private DateTime currentDT;
        private int day;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject attendGO = null;
        [SerializeField] private GameObject[] stickers = null;
        [SerializeField] private GameObject todayGO = null;
        [SerializeField] private TMP_Text dayTMP = null;
        [SerializeField] private TMP_Text daySatTMP = null;
        [SerializeField] private TMP_Text daySunTMP = null;
        [SerializeField] private Animator stickerAni = null;
        [SerializeField] private GameObject completeGo = null;

        // Unity Messages
        private void Awake()
		{
            GetComponent<Button>().onClick.AddListener(() => OnClick?.Invoke(this));
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}