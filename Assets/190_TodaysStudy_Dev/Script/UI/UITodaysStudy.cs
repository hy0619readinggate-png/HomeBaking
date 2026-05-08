using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using beyondi.Behaviour;
using DoDoEng.Launcher.UI;

namespace DoDoEng.TodaysStudy.UI
{
	public class UITodaysStudy : BYDSingleton<UITodaysStudy>
    {
        // Properties
        public Button BackButton => backBTN;
        public CoinInfo CoinInfo => coinInfo;
        public CoursePopup CoursePU => coursePU;
        public ContinuousAttendPopup ContinuousAttendPU => continuousAttendPU;
        public SimplePopup ExitPU => exitPU;

        // Properties
        public bool VisibleBackButton
        {
            get => backBTN.gameObject.activeSelf;
            set
            {
                backBTN.gameObject.SetActive(value);
            }
        }
        public bool VisibleCoinInfo
        {
            get => coinInfo.gameObject.activeSelf;
            set
            {
                coinInfo.gameObject.SetActive(value);
            }
        }
        public bool VisibleReview
        {
            get => reviewGO.activeSelf;
            set
            {
                reviewGO.SetActive(value);
            }
        }



        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private Button backBTN = null;
        [SerializeField] private CoinInfo coinInfo = null;
        [SerializeField] private CoursePopup coursePU = null;
        [SerializeField] private ContinuousAttendPopup continuousAttendPU = null;
        [SerializeField] private SimplePopup exitPU = null;
        [SerializeField] private GameObject reviewGO = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            VisibleBackButton = false;
            VisibleCoinInfo = false;

            coursePU.gameObject.SetActive(false);
            continuousAttendPU.gameObject.SetActive(false);
            exitPU.gameObject.SetActive(false);
            reviewGO.SetActive(false);
        }
        private void Start()
        {
        }
    }
}