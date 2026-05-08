using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A01
{
    public class ProblemSign : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup()", this);

            text.text = pData.Word;
            image.sprite = pData.WordSPR;
            image.gameObject.SetActive(pData.WordSPR != null);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }

        // Events
        public Action OnClick;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image image = null;
        [SerializeField] private TextMeshProUGUI text = null;
        [SerializeField] private Button button = null;

        // Unity Messages
        private void Awake()
        {
            EnableInteraction(false);

            button.onClick.AddListener(() => OnClick?.Invoke());
        }
        private void Start()
        {
        }
    }
}