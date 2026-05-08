using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A10
{
    [RequireComponent(typeof(Animator))]
    public class Example : MonoBehaviour,
        IID, ISubmitable
    {
        // Properties
        public bool IsAnswer => exam.IsAnswer;

        // Methods
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup()", this);

            this.exam = exam;

            wordTXT.text = exam.Word;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            if (enable)
                IsSubmit = false;

            cg.blocksRaycasts = enable;
        }
        public void DebugClick()
        {
            LOG.Info($"DebugClick()", this);

            btn_onClick();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private ExampleData exam = null;

        // Event Handlers
        private void btn_onClick()
        {
            LOG.Info($"btn_onClick()", this);

            AudioMGR.One.PlayEffect(clickCLIP);

            AudioMGR.One.PlayNarration(exam.WordCLIP);
            anim.SetTrigger("Click");

            IsSubmit = true;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI wordTXT = null;
        [SerializeField] private Button btn = null;
        [Header("★ Bindings")]
        [SerializeField] private AudioClip clickCLIP = null;

        // Unity Messages
        private void Awake()
        {

            cg.blocksRaycasts = false;

            btn.onClick.AddListener(btn_onClick);
        }
        private void Start()
        {
        }



        // Interface : IID
        public int ID { get; set; }

        // Interface : ISubmitable
        public bool IsSubmit { get; private set; }
    }
}