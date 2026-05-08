using DG.Tweening;
using DoDoEng.Common;
using TMPro;
using UnityEngine;

namespace DoDoEng
{
    public class S1Answer : MonoBehaviour
    {
        // Methods
        public void Setup(string word)
        {
            LOG.Info($"Setup() | {word}", this);

            wordTXT.text = word;

            cg.alpha = 0;
            cg.blocksRaycasts = false;
        }
        public void Show(float duration)
        {
            LOG.Info($"Show()", this);

            DOVirtual.DelayedCall(duration * 0.5f, () => cg.DOFade(1, duration));
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            cg.DOKill();
            cg.alpha = 0;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI wordTXT = null;

        // Unity Messages
        private void Awake()
        {
            cg.alpha = 0;
        }
        private void Start()
        {
        }
    }
}