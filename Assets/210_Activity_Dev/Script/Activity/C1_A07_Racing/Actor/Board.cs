using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A07
{
    public class Board : MonoBehaviour
    {
        // Properties
        public bool IsShow => isShow;

        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup()", this);

            textTXT.text = pData.Subject;
            textCLIP = pData.SubjectCLIP;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            anim.SetTrigger("Show");
            isShow = true;
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            anim.SetTrigger("Hide");
            isShow = false;
        }
        public Coroutine PlayWordSound()
        {
            LOG.Info($"PlayWordSound()", this);

            stopWordSound();
            crPlaySound = StartCoroutine(coPlayWordSound());
            return crPlaySound;
        }
        public void StopWordSound()
        {
            LOG.Info($"StopWordSound()", this);
            stopWordSound();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private bool isShow = false;
        private AudioClip textCLIP = null;
        private Coroutine crPlaySound = null;

        // Functions
        private void stopWordSound()
        {
            if (crPlaySound != null)
            {
                StopCoroutine(crPlaySound);
                AudioMGR.One.StopNarration();
            }
        }


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI textTXT = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            btn.onClick.AddListener(() => PlayWordSound());
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coPlayWordSound()
        {
            using (LOG.Coroutine("coPlayWordSound()", this))
            {
                LOG.Info($"{textCLIP.name}", this);

                cg.blocksRaycasts = false;
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(textCLIP);

                cg.blocksRaycasts = true;
                yield return null;
            }
        }
    }
}