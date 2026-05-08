using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A04
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class Problem : MonoBehaviour
    {
        // Methods
        public void Setup(Sprite sprite)
        {
            LOG.Info($"Setup()", this);

            image.sprite = sprite;
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
        private Image image_ = null;
        private Image image => image_ ??= GetComponent<Image>();
        private Button button_ = null;
        private Button button => button_ ??= GetComponent<Button>();



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