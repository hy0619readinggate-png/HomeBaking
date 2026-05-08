using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using DoDoEng.Behaviour;
using TMPro;
using DoDoEng.Common;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

namespace DoDoEng.Library.UI
{
	public class LibraryEBookStartPopup : MonoBehaviour
	{
		// Definitions
		// Properties
		public bool ActiveSelf => gameObject.activeSelf;

		// Methods
		public async UniTask Show(LibrarySlot parentSlot)
		{
            LOG.Function(this, $"| parentSlot={parentSlot}");

            this.parentSlot = parentSlot;

            gameObject.SetActive(true);

            ebookList = parentSlot.EBookList;

			slot.InitEBookStartPopup(parentSlot.Thumbnail, parentSlot.IsMyRecord ? parentSlot.Date : null);
			titleTMP.text = ebookList.Title;
            infoTMP.text = ebookList.Info;
            wordClips = new AudioClip[ebookList.Words.Length];

            likeTGL.gameObject.SetActive(!parentSlot.IsMyRecord);
            likeTGL.SetIsOnWithoutNotify(parentSlot.IsFavorite);
            removeBTN.gameObject.SetActive(parentSlot.IsMyRecord);
            ebookButtonSetGO.SetActive(!parentSlot.IsMyRecord);
            myebookButtonSetGO.SetActive(parentSlot.IsMyRecord);

            quizDisableGO.SetActive(!parentSlot.IsRead && !parentSlot.IsRecorded);
            quizBT.gameObject.SetActive(parentSlot.IsRead || parentSlot.IsRecorded);

            readBT.Checked = parentSlot.IsRead;
            recordBT.Checked = parentSlot.IsRecorded;
            quizBT.Checked = parentSlot.IsQuizDone;

            for (int i = 0; i < wordsTMP.Length; i++)
            {
                wordsTMP[i].gameObject.SetActive(false);
                wordSpeeakersBT[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < ebookList.Words.Length; i++)
            {
                var word = ebookList.Words[i];
                wordClips[i] = await DataLoader.One.LoadSound($"eBook/Sounds/Word/{word}.mp3");
                wordsTMP[i].text = word;
                wordsTMP[i].gameObject.SetActive(true);
                wordSpeeakersBT[i].gameObject.SetActive(true);
            }
        }
		public void Hide()
		{
			gameObject.SetActive(false);
		}

        // Events
        public Action<LibrarySlot> OnRead;
        public Action<LibrarySlot, bool> OnRecord;
        public Action<LibrarySlot> OnQuiz;
        public Action<LibrarySlot> OnPlay;
        public Action<LibrarySlot> OnAgain;
        public Action<LibrarySlot> OnFavorite;
        public Action<LibrarySlot> OnDelete;



        // Fields : caching

        // Fields
        private LibrarySlot parentSlot;
        private LibraryEBookList ebookList;
        private AudioClip[] wordClips;

        // Functions

        // Event Handlers
        private void readBT_onClick()
        {
            OnRead?.Invoke(parentSlot);
        }
        private void recordBT_onClick()
        {
            OnRecord?.Invoke(parentSlot, recordBT.Checked);
        }
        private void quizBT_onClick()
        {
            OnQuiz?.Invoke(parentSlot);
        }
        private void playBT_onClick()
        {
            OnPlay?.Invoke(parentSlot);
        }
        private void againBT_onClick()
        {
            OnAgain?.Invoke(parentSlot);
        }
        private void wordSpeakerBT_onClick()
        {
            LOG.Function(this, $"{EventSystem.current.currentSelectedGameObject}");
            var clickObject = EventSystem.current.currentSelectedGameObject;
            var idx = wordSpeeakersBT.FindIndex(clickObject.GetComponent<Button>());
            if (idx != -1 && wordClips[idx] != null)
            {
                AudioMGR.One.PlayNarration(wordClips[idx]);
            }
        }
        private void likeTGL_onValueChanged(bool value)
        {
            LOG.Function(this, $"| value={value}");

            parentSlot.IsFavorite = value;
            OnFavorite?.Invoke(parentSlot);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button closeBT = null;
        [SerializeField] private LibrarySlot slot = null;
        [SerializeField] private TMP_Text titleTMP = null;
        [SerializeField] private TMP_Text infoTMP = null;
        [SerializeField] private Button[] wordSpeeakersBT = null;
        [SerializeField] private TMP_Text[] wordsTMP = null;
        [SerializeField] private LibraryEBookStartPopupButton readBT = null;
        [SerializeField] private LibraryEBookStartPopupButton recordBT = null;
        [SerializeField] private LibraryEBookStartPopupButton quizBT = null;
        [SerializeField] private LibraryEBookStartPopupButton playBT = null;
        [SerializeField] private LibraryEBookStartPopupButton againBT = null;
        [SerializeField] private GameObject quizDisableGO = null;
        [SerializeField] private Toggle likeTGL = null;
        [SerializeField] private Button removeBTN = null;
        [SerializeField] private GameObject ebookButtonSetGO = null;
        [SerializeField] private GameObject myebookButtonSetGO = null;
        //[Header("★ Config")]
        //[SerializeField] private int intValue = 0;

        // Unity Messages
        private void Awake()
		{
            closeBT.onClick.AddListener(() => Hide());
            for (int i = 0; i < wordSpeeakersBT.Length; i++)
                wordSpeeakersBT[i].onClick.AddListener(() => wordSpeakerBT_onClick());
            likeTGL.onValueChanged.AddListener(value => likeTGL_onValueChanged(value));
            removeBTN.onClick.AddListener(() => OnDelete?.Invoke(parentSlot));
        }
		private void Start()
		{
		}
        private void OnEnable()
        {
            readBT.OnClick += readBT_onClick;
            recordBT.OnClick += recordBT_onClick;
            quizBT.OnClick += quizBT_onClick;
            playBT.OnClick += playBT_onClick;
            againBT.OnClick += againBT_onClick;
        }
        private void OnDisable()
        {
            readBT.OnClick -= readBT_onClick;
            recordBT.OnClick -= recordBT_onClick;
            quizBT.OnClick -= quizBT_onClick;
            playBT.OnClick -= playBT_onClick;
            againBT.OnClick -= againBT_onClick;
        }

        // Unity Coroutine
    }
}