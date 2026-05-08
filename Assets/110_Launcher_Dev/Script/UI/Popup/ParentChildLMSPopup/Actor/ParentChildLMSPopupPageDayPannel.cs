using beyondi.Util;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DoDoEng.Launcher.UI
{
    public class ParentChildLMSPopupPageDayPannel : MonoBehaviour
    {
        // Methods
        public void HideAndClear()
        {
            LOG.Function(this);

            gameObject.SetActive(false);

            icons.ForEach(icon => icon.Clear());

        }
        public void ShowAndSetup(JToken dayJSON)
        {
            LOG.Function(this, $"{dayJSON}");

            var day = dayJSON.Value<int>("day");
            dayTXT.text = $"Day {day}";

            var learningList = dayJSON.Value<JArray>("learningList");
            foreach (var (icon, i) in icons.Select((icon, i) => (icon, i)))
            {
                var learning = learningList[i];
                var contentIndex = learning.Value<string>("contentIndex");
                var contentCode = contentIndex.FirstOrDefault().ToString();
                var isComplete = learning.Value<bool>("isComplete");
                var isPrevComplete = false;
                try
                {
                    isPrevComplete = learning.Value<bool>("isPrevComplete");
                }
                catch { }
                icon.Setup(contentCode, isComplete, isPrevComplete);
            }

            gameObject.SetActive(true);
        }



        // Fields : caching
        private ParentChildLMSPopupPageDayIcon[] icons_ = null;
        private ParentChildLMSPopupPageDayIcon[] icons => icons_ ??= GetComponentsInChildren<ParentChildLMSPopupPageDayIcon>(true);


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI dayTXT = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}