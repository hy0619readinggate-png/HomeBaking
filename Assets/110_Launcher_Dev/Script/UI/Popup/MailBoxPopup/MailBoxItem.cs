using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using System;
using TMPro;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
	public class MailBoxItem : MonoBehaviour
	{
        // Definitions
        // Properties
        public int SN => sn;

        // Methods
        public void Init(JToken data)
        {
            LOG.Function(this, $"| data={data}");

            sn = data.Value<int>("sn");
            var title = data.Value<string>("title");
            var content = data.Value<string>("content");
            var isNew = data.Value<bool>("isNew");
            var createdDatetime = data.Value<string>("createdDatetime");
            var remainingPeriod = data.Value<int>("remainingPeriod");

            titleTMP.text = title;
            contentTMP.text = content;
            isNewGO.SetActive(isNew);
            dateTMP.text = $"{createdDatetime.Substring(0, 4)}.{createdDatetime.Substring(5, 2)}.{createdDatetime.Substring(8, 2)}";
            periodTMP.text = LocalizationMGR.One.GetText("WORD_138", remainingPeriod);
        }

        // Events
        public event Action<MailBoxItem> OnDelete;



        // Fields
        private int sn;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject isNewGO = null;
        [SerializeField] private TMP_Text titleTMP = null;
        [SerializeField] private TMP_Text contentTMP = null;
        [SerializeField] private TMP_Text dateTMP = null;
        [SerializeField] private TMP_Text periodTMP = null;
        [SerializeField] private Button deleteBT = null;

        // Unity Messages
        private void Awake()
		{
            deleteBT.onClick.AddListener(() => OnDelete?.Invoke(this));
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}