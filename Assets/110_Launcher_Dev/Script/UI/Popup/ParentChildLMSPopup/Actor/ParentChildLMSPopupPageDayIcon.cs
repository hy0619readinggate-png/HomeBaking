using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Launcher.UI
{
    public class ParentChildLMSPopupPageDayIcon : MonoBehaviour
    {
        // Methods
        public void Clear()
        {
            LOG.Function(this);

            activityTodayGO.SetActive(false);
            activityPrevGO.SetActive(false);
            activityNoneGO.SetActive(false);
            eBookTodayGO.SetActive(false);
            eBookPrevGO.SetActive(false);
            eBookNoneGO.SetActive(false);
            movieTodayGO.SetActive(false);
            moviePrevGO.SetActive(false);
            movieNoneGO.SetActive(false);
            playgroundTodayGO.SetActive(false);
            playgroundPrevGO.SetActive(false);
            playgroundNoneGO.SetActive(false);
            aiStudioTodayGO.SetActive(false);
            aiStudioPrevGO.SetActive(false);
            aiStudioNoneGO.SetActive(false);
        }
        public void Setup(string contentCode, bool isComplete, bool isPrevComplete)
        {
            LOG.Function(this, $"{contentCode}, {isComplete}, {isPrevComplete}");

            // studyID
            // Activity 1
            // EBook 2
            // Movie 4
            // Playground 5
            // AIStudio 7
            activityTodayGO.SetActive(contentCode == "1" && isComplete && !isPrevComplete);
            activityPrevGO.SetActive(contentCode == "1" && isPrevComplete);
            activityNoneGO.SetActive(contentCode == "1" && !isComplete && !isPrevComplete);
            eBookTodayGO.SetActive(contentCode == "2" && isComplete && !isPrevComplete);
            eBookPrevGO.SetActive(contentCode == "2" && isPrevComplete);
            eBookNoneGO.SetActive(contentCode == "2" && !isComplete && !isPrevComplete);
            movieTodayGO.SetActive(contentCode == "4" && isComplete && !isPrevComplete);
            moviePrevGO.SetActive(contentCode == "4" && isPrevComplete);
            movieNoneGO.SetActive(contentCode == "4" && !isComplete && !isPrevComplete);
            playgroundTodayGO.SetActive(contentCode == "5" && isComplete && !isPrevComplete);
            playgroundPrevGO.SetActive(contentCode == "5" && isPrevComplete);
            playgroundNoneGO.SetActive(contentCode == "5" && !isComplete && !isPrevComplete);
            aiStudioTodayGO.SetActive(contentCode == "7" && isComplete && !isPrevComplete);
            aiStudioPrevGO.SetActive(contentCode == "7" && isPrevComplete);
            aiStudioNoneGO.SetActive(contentCode == "7" && !isComplete && !isPrevComplete);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject activityTodayGO = null;
        [SerializeField] private GameObject activityPrevGO = null;
        [SerializeField] private GameObject activityNoneGO = null;
        [SerializeField] private GameObject eBookTodayGO = null;
        [SerializeField] private GameObject eBookPrevGO = null;
        [SerializeField] private GameObject eBookNoneGO = null;
        [SerializeField] private GameObject movieTodayGO = null;
        [SerializeField] private GameObject moviePrevGO = null;
        [SerializeField] private GameObject movieNoneGO = null;
        [SerializeField] private GameObject playgroundTodayGO = null;
        [SerializeField] private GameObject playgroundPrevGO = null;
        [SerializeField] private GameObject playgroundNoneGO = null;
        [SerializeField] private GameObject aiStudioTodayGO = null;
        [SerializeField] private GameObject aiStudioPrevGO = null;
        [SerializeField] private GameObject aiStudioNoneGO = null;


        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}