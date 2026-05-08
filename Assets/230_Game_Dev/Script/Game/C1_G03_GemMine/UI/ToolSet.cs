using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.C1_G03
{
    public class ToolSet : MonoBehaviour
    {
        // Methods
        public void Show()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(showCLIP);
            anim.SetTrigger("Show");
            cg.blocksRaycasts = true;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Function(this);

            cg.blocksRaycasts = enable;
        }
        public void Hide()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(hideCLIP);
            anim.SetTrigger("Hide");
            cg.blocksRaycasts = false;
        }

        // Events
        public event Action OnListenClick;
        public event Action OnUndoClick;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator anim = null;
        [SerializeField] private Button listenBTN = null;
        [SerializeField] private Button undoBTN = null;
        [Header("★ Bindings")]
        [SerializeField] private AudioClip showCLIP = null;
        [SerializeField] private AudioClip hideCLIP = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            listenBTN.onClick.AddListener(() => OnListenClick?.Invoke());
            undoBTN.onClick.AddListener(() => OnUndoClick?.Invoke());
        }
        private void Start()
        {
        }
    }
}