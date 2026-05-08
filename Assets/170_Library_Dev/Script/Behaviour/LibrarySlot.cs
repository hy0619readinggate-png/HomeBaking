using beyondi.Util;
using System.Linq;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Cysharp.Threading.Tasks;

namespace DoDoEng.Behaviour
{
	public class LibrarySlot : MonoBehaviour
	{
        // Definitions
        // Properties
        public int CurriculumId { get; set; }
        public string ContentIndex { get; private set; }
        public int MainCategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public int Course => course;
        public int Type => type;
        public string ThumbnailPath { get; set; }
        public Sprite Thumbnail
        {
            get => thumbnailIMG.sprite;
            set => thumbnailIMG.sprite = value;
        }
        public string Title => titleTMP.text;
        public LibraryEBookList EBookList => ebookList;
        public MovieList MovieList { get; set; }
        public string Date => date;
        public bool Checked
        {
            get { return checkGO.activeSelf; }
            set { checkGO.SetActive(value); }
        }
        public bool IsMyRecord => isMyRecord;
        public bool IsLock { get; private set; }
        public bool IsComplete { get; set; }
        public bool IsRead { get; set; }
        public bool IsRecorded { get; private set; }
        public bool IsQuizDone { get; set; }
        public bool IsFavorite
        {
            get => likeTGL.isOn;
            set => likeTGL.SetIsOnWithoutNotify(value);
        }


        // Methods
        public void Init(string index, string title, Sprite thumbnail)
		{// for Movie
			LOG.Info($"Init({index}, {title}, {thumbnail})", this);

            gameObject.SetActive(true);
            init();

            this.ContentIndex = index;
            titleTMP.text = title;
            thumbnailIMG.sprite = thumbnail;

            if (likeTGL) likeTGL.gameObject.SetActive(true);
            if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(false);
        }
        public void InitEBook(LibraryEBookList ebookList, bool isRecorded = false, bool isLock = false, int stage = 0)
        {// for EBook
            LOG.Function(this, $"({ebookList} | {isRecorded} | {isLock} | {stage})");

            gameObject.SetActive(true);
            init();

            this.isMyRecord = false;
            IsRecorded = isRecorded;

            this.ebookList = ebookList;
            this.ContentIndex = ebookList.Index;
            IsLock = isLock;
            titleTMP.text = ebookList.Title;

            lockGO.SetActive(isLock);
            lockStageTMP.text = $"Stage {stage}";

            if (likeTGL) likeTGL.gameObject.SetActive(true);
            if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(IsRecorded);
        }
        public void InitMyEBook(LibraryEBookList ebookList, Sprite thumbnail, string date)
        {// for My EBook
            LOG.Function(this, $"({ebookList} | {thumbnail} | {date})");

            this.date = date;

            gameObject.SetActive(true);
            init();

            isMyRecord = true;
            IsRecorded = true;

            this.ebookList = ebookList;
            this.ContentIndex = ebookList.Index;
            titleTMP.text = ebookList.Title;
            thumbnailIMG.sprite = thumbnail;

            if (likeTGL) likeTGL.gameObject.SetActive(false);
            if (deleteBT) deleteBT.gameObject.SetActive(true);
            //if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(true);
        }
        public void InitEBookPlayer(LibraryEBookList ebookList, bool isMyRecord = false, bool isLock = false, int stage = 0)
        {// for EBook
            LOG.Function(this, $"({ebookList} | {isMyRecord} |{isLock} {stage})");

            gameObject.SetActive(true);
            init();

            this.isMyRecord = isMyRecord;
            IsRecorded = isMyRecord;

            this.ebookList = ebookList;
            this.ContentIndex = ebookList.Index;
            IsLock = isLock;
            titleTMP.text = ebookList.Title;

            lockGO.SetActive(isLock);
            lockStageTMP.text = $"Stage {stage}";

            if (likeTGL) likeTGL.gameObject.SetActive(false);
            if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(this.isMyRecord);
        }
        public void InitEBookStartPopup(Sprite thumbnail, string date)
        {// for EBook Start Popup
            LOG.Info($"Init({thumbnail}, {date})", this);

            gameObject.SetActive(true);
            init();

            titleTMP.text = date;
            if (ebookShadowGO) ebookShadowGO.SetActive(!string.IsNullOrEmpty(date));
            thumbnailIMG.sprite = thumbnail;

            if (likeTGL) likeTGL.gameObject.SetActive(false);
            if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(!string.IsNullOrEmpty(date));
        }
        public void InitMovie(string index, string title, bool isRecorded = false, bool isLock = false, int stage = 0)
        {// for Movie
            LOG.Function(this, $"({index} | {title} | {isRecorded} | {isLock} | {stage})");

            gameObject.SetActive(true);
            init();

            isMyRecord = false;
            IsRecorded = isRecorded;
            IsLock = isLock;

            ContentIndex = index;
            titleTMP.text = title;

            lockGO.SetActive(isLock);
            lockStageTMP.text = $"Stage {stage}";

            if (likeTGL) likeTGL.gameObject.SetActive(true);
            if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(isRecorded);
        }
        public void InitMyMovie(string index, string title, Sprite thumbnail, string date)
        {// for My Movie
            LOG.Function(this, $"({index} | {title} | {thumbnail} | {date})");

            this.date = date;

            gameObject.SetActive(true);
            init();

            isMyRecord = true;
            IsRecorded = true;

            this.ContentIndex = index;
            titleTMP.text = title;
            thumbnailIMG.sprite = thumbnail;

            if (likeTGL) likeTGL.gameObject.SetActive(false);
            if (deleteBT) deleteBT.gameObject.SetActive(true);
            //if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(true);
        }
        public void InitMoviePlayer(MovieList movieList, Sprite thumbnail, string date = null)
        {
            LOG.Function(this, $"({movieList} | {thumbnail} | {date})");

            this.date = date;

            gameObject.SetActive(true);
            init();

            isMyRecord = false;

            MovieList = movieList;
            ContentIndex = movieList.Index;
            titleTMP.text = movieList.Title;
            thumbnailIMG.sprite = thumbnail;

            if (likeTGL) likeTGL.gameObject.SetActive(false);
            if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(this.date != null);
        }
        public void InitActivityType(int course, int type, string title, string thumbnailPath, bool isLock = false)
        {// for Activity
            LOG.Info($"Init({course}, {type}, {title}, {thumbnailPath}, {isLock})", this);

            gameObject.SetActive(true);
            init();

            this.course = course;
            this.type = type;
            IsLock = isLock;
            titleTMP.text = title;
            this.ThumbnailPath = thumbnailPath;

            lockGO.SetActive(isLock);

            if (likeTGL) likeTGL.gameObject.SetActive(true);
            if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(false);
        }
        public async UniTask LoadThumbnail()
        {
            try
            {
                Thumbnail = await DataLoader.One.LoadSprite(ThumbnailPath);
            }
            catch { }
        }

        // Events
        public Action<LibrarySlot> OnClick;
        public Action<LibrarySlot> OnFavorite;
        public Action<LibrarySlot> OnDelete;



        // Fields : caching
        private Button button_;
        private Button button => button_ ??= GetComponent<Button>();

        // Fields
        private int course;
        private int type;
        private LibraryEBookList ebookList;
        private string date;
        private bool isMyRecord;
        private int songAndChant;

        // Functions
        private void init()
        {
            if (newGO) newGO.SetActive(false);
            checkGO.SetActive(false);
            if (myFlagGO) myFlagGO.SetActive(false);
            if (deleteBT) deleteBT.gameObject.SetActive(false);
            if (likeTGL)
            {
                likeTGL.isOn = false;
                likeTGL.gameObject.SetActive(true);
            }
            titleTMP.text = string.Empty;
            lockGO.SetActive(false);
        }

        // Event Handlers
        private void likeTGL_onValueChanged(bool value)
        {
            LOG.Function(this, $"| value={value}");

            if (string.IsNullOrEmpty(ContentIndex))
            {
                LOG.Warning($"ContentIndex is null", this);
                return;
            }

            OnFavorite?.Invoke(this);
        }
        private void deleteBT_onClick()
        {
            LOG.Function(this);

            OnDelete?.Invoke(this);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private GameObject newGO = null;
		[SerializeField] private GameObject checkGO = null;
		[SerializeField] private GameObject myFlagGO = null;
		[SerializeField] private Button deleteBT = null;
		[SerializeField] private Toggle likeTGL = null;
        [SerializeField] private TMP_Text titleTMP = null;
		[SerializeField] private GameObject lockGO = null;
        [SerializeField] private TMP_Text lockStageTMP = null;
        [SerializeField] private Image thumbnailIMG = null;
        [SerializeField] private GameObject ebookShadowGO = null;
        //[Header("★ Config")]
        //[SerializeField] private int intValue = 0;

        // Unity Messages
        private void Awake()
		{
            thumbnailIMG.sprite = null;
            button.onClick.AddListener(() =>
            {
                if (!lockGO.activeSelf) OnClick?.Invoke(this);
            });
            if (likeTGL != null) likeTGL.onValueChanged.AddListener(value => likeTGL_onValueChanged(value));
            if (deleteBT != null) deleteBT.onClick.AddListener(() => deleteBT_onClick());
        }
		private void Start()
		{
		   
		}

		// Unity Coroutine
	}
}