using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A12
{
    public class S1Problem : MonoBehaviour
    {
        // Methods
        public void Setup(Sprite sprite)
        {
            LOG.Info($"Setup()", this);

            problemIMG.sprite = sprite;
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
        [SerializeField] private Image problemIMG = null;
        [SerializeField] private Button btn = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            btn.onClick.AddListener(() => OnClick?.Invoke());
        }
        private void Start()
        {
        }
    }
}