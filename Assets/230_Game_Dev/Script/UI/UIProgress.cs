using DoDoEng.Common;
using TMPro;
using UnityEngine;

namespace DoDoEng
{
    public class UIProgress : MonoBehaviour
    {
        // Methods
        public void Setup(int total)
        {
            LOG.Info($"Setup()", this);

            totalCount = total;
            currentCount = 0;
            updateProgress();
        }
        public void Increase()
        {
            LOG.Info($"Increase()", this);

            currentCount++;
            updateProgress();
        }



        // Fields
        private int totalCount;
        private int currentCount;

        // Functions
        private void updateProgress()
        {
            progressTMP.text = $"{currentCount}/{totalCount}";
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI progressTMP;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}