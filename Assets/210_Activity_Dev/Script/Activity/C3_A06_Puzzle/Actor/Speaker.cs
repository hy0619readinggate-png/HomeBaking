using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C3_A06
{
    [RequireComponent(typeof(Button))]
    public class Speaker : MonoBehaviour
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void SetPlay(bool isPlaying)
        {
            LOG.Info($"SetPlay() | {isPlaying}", this);

            anim.SetTrigger(isPlaying ? "Play" : "Idle");
        }

        // Events
        public event Action OnClick;



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator anim = null;

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