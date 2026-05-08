using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Launcher
{
    public class TodaysStudyNaviIcon : MonoBehaviour
    {
        // Properties
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                invalidate();
            }
        }

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Init(bool isCompleted, string contentIndex)
        {
            this.isCompleted = isCompleted;

            Activate(true);

            int idx = 0;
            if (contentIndex.StartsWith("2"))
            {// eBook
                idx = 1;
            }
            else if (contentIndex.StartsWith("4"))
            {// Movie
                idx = 3;
            }
            else if (contentIndex.StartsWith("5"))
            {// Game
                idx = 2;
            }
            else if (contentIndex.StartsWith("1"))
            {// Activity
                idx = 0;
            }

            for (int i = 0; i < incompleteTexts.Length; i++)
            {
                incompleteTexts[i].SetActive(i == idx);
            }
            for (int i = 0; i < completeTexts.Length; i++)
            {
                completeTexts[i].SetActive(i == idx);
            }
            for (int i = 0; i < completeDots.Length; i++)
            {
                completeDots[i].SetActive(i == idx);
            }

            invalidate();
        }

        // Events



        // Fields
        private bool isCompleted;
        private bool isSelected;

        // Functions
        private void invalidate()
        {
            incompleteGO.SetActive(!isCompleted && !isSelected);
            incompleteSelectGO.SetActive(!isCompleted && isSelected);
            completeGO.SetActive(isCompleted && !isSelected);
            completeSelectGO.SetActive(isCompleted && isSelected);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject incompleteGO = null;
        [SerializeField] private GameObject incompleteSelectGO = null;
        [SerializeField] private GameObject completeGO = null;
        [SerializeField] private GameObject completeSelectGO = null;
        [SerializeField] private GameObject[] incompleteTexts = null;
        [SerializeField] private GameObject[] completeTexts = null;
        [SerializeField] private GameObject[] completeDots = null;

        // Unity Messages
        private void Awake()
        {
            incompleteTexts.ForEach(text => text.SetActive(false));
            completeTexts.ForEach(text => text.SetActive(false));
            completeDots.ForEach(dot => dot.SetActive(false));
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