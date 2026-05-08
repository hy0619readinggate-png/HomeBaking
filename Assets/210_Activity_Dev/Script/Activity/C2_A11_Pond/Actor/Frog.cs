using DG.Tweening;
using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A11
{
    public class Frog : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup()", this);

            this.pData = pData;

            wordTXT.text = pData.BlankWord;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void ProblemIdle()
        {
            LOG.Info($"ProblemIdle()", this);

            anim.SetTrigger("ProblemIdle");
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            DOVirtual.DelayedCall(answerTextDelay, () =>
            {
                AudioMGR.One.PlayEffect(correctCLIP);
                wordTXT.text = pData.Word;
            });

            AudioMGR.One.PlayEffect(leverCLIP);
            anim.SetTrigger("Correct");
        }
        public void CorrectFin()
        {
            LOG.Info($"CorrectFin()", this);

            DOTween.CompleteAll(true);
            wordTXT.text = pData.Word;
            anim.SetTrigger("CorrectFin");
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            anim.SetTrigger("Wrong");
        }

        // Events
        public event Action OnClick;

        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private ProblemData pData = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI wordTXT = null;
        [SerializeField] private Button btn = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip leverCLIP = null;
        [SerializeField] private AudioClip correctCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float answerTextDelay = 0.4f;

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