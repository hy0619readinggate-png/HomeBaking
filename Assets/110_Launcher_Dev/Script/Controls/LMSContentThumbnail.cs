using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Newtonsoft.Json.Linq;

namespace DoDoEng.Launcher.UI
{
	public class LMSContentThumbnail : MonoBehaviour
	{
        // Definitions

        // Properties
        public string Title => titleTMP.text;
        public Sprite Thumbnail
        {
            get => thumbnailIMG.sprite;
            set => thumbnailIMG.sprite = value;
        }
        public bool IsNew { get => newGO.activeSelf; set => newGO.SetActive(value); }
        public int ContentIndex;
        public bool IsRecorded { get; private set; }
        public int RecordedType { get; set; }
        public int LearningTime { get; set; }
        public DateTime CompleteDatetime { get; set; }
        public int LogSN { get; set; }
        public bool HasRecordFiles { get; set; }

        // Methods
        public void Init(string title, Sprite thumbnail, bool myFlag = false, int idxMyFlag = 0)
        {
            titleTMP.text = title;
            thumbnailIMG.sprite = thumbnail;
            IsRecorded = myFlag;
            RecordedType = idxMyFlag;
            for (int i = 0; i < myFlags.Length; i++)
                myFlags[i].SetActive(myFlag && idxMyFlag == i);
            lockGO.SetActive(false);
        }

        // Events
        public Action<LMSContentThumbnail> OnClick;



        // Fields : caching
        private Button button_;
        private Button button => button_ ??= GetComponent<Button>();

        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] myFlags = null;
        [SerializeField] private TMP_Text titleTMP = null;
        [SerializeField] private GameObject lockGO = null;
        [SerializeField] private Image thumbnailIMG = null;
        [SerializeField] private GameObject newGO = null;

        // Unity Messages
        private void Awake()
		{
            titleTMP.text = "";
            thumbnailIMG.sprite = null;
            myFlags.ForEach(myFlag => myFlag.SetActive(false));
            lockGO.SetActive(false);
            newGO.SetActive(false);

            button.onClick.AddListener(() => OnClick?.Invoke(this));
        }
		private void Start()
		{
		}

		// Unity Coroutine
	}
}