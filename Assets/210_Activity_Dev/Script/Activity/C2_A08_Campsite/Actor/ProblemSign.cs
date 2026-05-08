using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A08
{
    public class ProblemSign : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData problem)
        {
            LOG.Info($"Setup()", this);

            phoneticTXT.text = problem.Text;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }

        // Events
        public event Action OnClick;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI phoneticTXT = null;
        [SerializeField] private Button button = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            button.onClick.AddListener(() => OnClick?.Invoke());
        }
        private void Start()
        {
        }
    }
}