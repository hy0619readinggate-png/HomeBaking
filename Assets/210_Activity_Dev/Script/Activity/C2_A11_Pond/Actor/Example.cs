using beyondi.Coroutine;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A11
{
    [RequireComponent(typeof(Leaf))]
    public class Example : AffBase, ISubmitable
    {
        // Properties
        public bool IsAnswer => exam.IsAnswer;

        // Methods
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Show()", this);

            this.exam = exam;

            textTXT.text = exam.Text;

            leaf.Hidden();
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            if (enable)
                isSubmit = false;

            cg.blocksRaycasts = enable && isShown;
        }
        public void Show(float delay)
        {
            LOG.Info($"Show()", this);

            DOVirtual.DelayedCall(delay, () =>
            {
                if (!isShown)
                {
                    isShown = true;
                    leaf.Show();
                }
            });
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            if (!isShown)
            {
                isShown = true;
                leaf.Idle();
            }
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            AudioMGR.One.PlayEffect(correctCLIP);

            vfxCorrectGO.SetActive(false);
            vfxCorrectGO.SetActive(true);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            AudioMGR.One.PlayEffect(wrongCLIP);

            isShown = false;
            leaf.Hide();
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            if (isShown)
            {
                isShown = false;
                leaf.Hide();
            }
        }
        public void Hidden()
        {
            LOG.Info($"Hidden()", this);

            isShown = false;
            leaf.Hidden();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Leaf leaf_ = null;
        private Leaf leaf => leaf_ ??= GetComponent<Leaf>();

        // Fields
        private ExampleData exam = null;
        private bool isSubmit = false;
        private bool isShown = false;

        // Overrides
        protected override IEnumerator onStartAff()
        {
            if (cg.blocksRaycasts)
                affTargetGO?.SetActive(true);
            yield return null;
        }
        protected override IEnumerator onFinishAff()
        {
            affTargetGO?.SetActive(false);
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI textTXT = null;
        [SerializeField] private Button btn = null;
        [SerializeField] private GameObject vfxCorrectGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            cg.blocksRaycasts = false;
            vfxCorrectGO.SetActive(false);

            btn.onClick.AddListener(() =>
            {
                isSubmit = true;
            });
        }
        private void Start()
        {
        }



        // Interface : ISubmitable
        public bool IsSubmit => isSubmit;
    }
}