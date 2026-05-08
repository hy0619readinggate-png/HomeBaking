using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook
{
    public class PageIndicator : MonoBehaviour
    {
        // Methods
        public void Setup(int totalPages)
        {
            LOG.Info($"Setup() | {totalPages}", this);

            // 표지는 페이지에 포함하지 않음
            this.pageCount = totalPages - 1;
            setup(pageCount);
            updatePage(0, pageCount);
        }
        public void UpdatePageNo(int currentPage)
        {
            LOG.Info($"UpdatePageNo() | {currentPage}", this);

            // 표지는 페이지에 포함하지 않음
            updatePage(currentPage - 1, pageCount);
        }



        // Fields : caching
        private TextMeshProUGUI numTMP_ = null;
        private TextMeshProUGUI numTMP => numTMP_ ??= numIndicatorTR.GetComponentInChildren<TextMeshProUGUI>(true);
        private Toggle[] dots_ = null;
        private Toggle[] dots => dots_ ??= dotIndicatorTR.GetComponentsInChildren<Toggle>(true);

        // Fields
        private int pageCount = 0;
        private bool isDotType = false;

        // Functions
        private void setup(int pageCount)
        {
            this.isDotType = pageCount <= dots.Length;

            dotIndicatorTR.gameObject.SetActive(isDotType);
            numIndicatorTR.gameObject.SetActive(!isDotType);
            dots.ForEach((i, d) => d.gameObject.SetActive(i < pageCount));
        }
        private void updatePage(int pageNo, int pageCount)
        {
            if (isDotType)
            {
                dotIndicatorTR.gameObject.SetActive(pageNo > 0);
                dots.ForEach((i, d) => d.isOn = i == pageNo - 1);
            }
            else
            {
                numIndicatorTR.gameObject.SetActive(pageNo > 0);
                numTMP.text = $"{pageNo}/{pageCount}";
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform numIndicatorTR = null;
        [SerializeField] private Transform dotIndicatorTR = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}