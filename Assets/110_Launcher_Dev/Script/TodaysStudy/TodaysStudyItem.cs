using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class TodaysStudyItem : MonoBehaviour
    {
        // Enum
        public enum TSItemState
        {
            Locked,
            Opened,
        }
        public enum TSLearningType
        {
            Activity,
            eBook,
            Game,
            Movie
        }

        // Properties
        public int Order => order;
        public TSItemState State => state;
        public TSLearningType LearningType => learningType;
        public IndexBase Index => index;
        public string ContentIndex => contentIndex;
        public int LearningIndexId => learningIndexId;
        public Texture Thumbnail
        {
            get => thumbnailRIM.texture;
            set => thumbnailRIM.texture = value;
        }
        public bool IsComplete { get; set; }
        public bool IsRead { get; set; }
        public bool IsRecorded { get; private set; }
        public bool IsQuizDone { get; set; }

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Init(int order, TSItemState state, string contentIndex, int learningIndexId, bool isCompleted, bool isRead, bool isRecorded, bool isQuizDone)
        {
            LOG.Function(this, $"| order={order} | state={state} | contentIndex={contentIndex} | learningIndexId={learningIndexId} | isCompleted={isCompleted} | isRead={isRead} | isRecorded={isRecorded} | isQuizDone={isQuizDone}");
            this.order = order;
            this.state = state;
            this.contentIndex = contentIndex;
            this.learningIndexId = learningIndexId;
            IsComplete = isCompleted;
            IsRead = isRead;
            IsRecorded = isRecorded;
            IsQuizDone = isQuizDone;

            if (this.contentIndex.StartsWith("2"))
            {
                learningType = TSLearningType.eBook;
                index = new EBookReadIndex(this.contentIndex, EBookReadMode.Native, isCompleted, isRead, isRecorded, isQuizDone);
            }
            else if (this.contentIndex.StartsWith("4"))
            {
                learningType = TSLearningType.Movie;
                index = new MovieSingleIndex(this.contentIndex, isCompleted, isRead, isRecorded);
            }
            else if (this.contentIndex.StartsWith("5"))
            {
                learningType = TSLearningType.Game;
                index = new GameIndex(this.contentIndex);
            }
            else if (this.contentIndex.StartsWith("1"))
            {
                learningType = TSLearningType.Activity;
                index = new ActivityIndex(this.contentIndex);
            }

            for (int i = 0; i < learningTypes.Length; i++)
                learningTypes[i].SetActive((int)learningType == i);

            if (state == TSItemState.Locked)
            {
            }
            else if (state == TSItemState.Opened)
            {
            }

            Activate(true);

            if (isCompleted)
                completeAni.SetTrigger("Checked");
            else
                completeAni.SetTrigger("Empty");
        }
        public void Select(bool selected)
        {
            selectedGO.SetActive(selected);
        }
        public void CompleteANIReady()
        {
            completeAni.SetTrigger("Empty");
        }
        public void CompleteANIStart()
        {
            completeAni.SetTrigger("Check");
        }

        // Events
        public event Action<TodaysStudyItem> OnClick;



        // Fields
        private int order;
        private TSItemState state;
        private string contentIndex;
        private int learningIndexId;
        private IndexBase index;
        private TSLearningType learningType;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private RawImage thumbnailRIM = null;
        [SerializeField] private GameObject[] learningTypes = null;
        [SerializeField] private Animator completeAni = null;
        [SerializeField] private GameObject selectedGO = null;

        // Unity Messages
        private void Awake()
        {
            selectedGO.SetActive(false);
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                if (GetComponent<RectTransform>().localScale.x > 0.9f)
                    OnClick?.Invoke(this);
            });
            button.enabled = true;
        }
        private void Start()
        {
        }
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }
        private void OnDestroy()
        {
        }
    }
}