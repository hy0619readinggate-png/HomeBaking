using beyondi.Coroutine;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A12
{
    [RequireComponent(typeof(Button))]
    public class SupplyButton : AffBase, ISubmitable
    {
        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
            isEnabled = enable;

            if (enable)
            {
                IsSubmit = false;
                AudioMGR.One.PlayEffect(activeCLIP);
            }
            vfxAiveGO.SetActive(enable);
            glowGO.SetActive(enable);
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private bool isEnabled = false;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            if (isEnabled)
                affTargetGO.SetActive(true);
            yield return null;
        }
        protected override IEnumerator onFinishAff()
        {
            affTargetGO.SetActive(false);
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject glowGO = null;
        [SerializeField] private GameObject vfxAiveGO = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip activeCLIP = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            cg.blocksRaycasts = false;
            glowGO.SetActive(false);
            affTargetGO.SetActive(false);

            btn.onClick.AddListener(() => IsSubmit = true);
        }
        private void Start()
        {
        }



        // Interface : ISubmitable
        public bool IsSubmit { get; private set; }
    }
}