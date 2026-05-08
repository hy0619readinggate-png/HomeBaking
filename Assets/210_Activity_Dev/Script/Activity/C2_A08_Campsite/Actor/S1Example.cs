using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A08
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Bottle))]
    public class S1Example : MonoBehaviour, ISubmitable
    {
        // Properties
        public bool IsAnswer => exam.IsAnswer;
        public AudioClip WordCLIP => exam.WordCLIP;
        public Sprite WordSPR => exam.WordSPR;
        public string Word => exam.Word;

        // Methods
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup()", this);

            this.exam = exam;

            bottle.Setup(exam.WordSPR, false);

            anim.SetTrigger("Hidden");

            vfxCorrectGO.SetActive(false);
        }
        public void Appear()
        {
            LOG.Info($"Appear()", this);

            //bottle.ShowImage();

            anim.SetTrigger("Appear");
        }
        public void Disppear()
        {
            LOG.Info($"Disppear()", this);

            anim.SetTrigger("Disappear");
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            //bottle.ShowImage();
            anim.SetTrigger("Idle");
        }
        public void Hidden()
        {
            LOG.Info($"Hidden()", this);

            anim.SetTrigger("Hidden");
        }
        public Coroutine StartCorrect()
        {
            LOG.Info($"StartCorrect()", this);

            crCorrect = StartCoroutine(coCorrect());
            return crCorrect;
        }
        public void StopCorrect()
        {
            LOG.Info($"StopCorrect()", this);

            this.StopCoroutineSafe(ref crCorrect);
        }
        public Coroutine StartWrong()
        {
            LOG.Info($"StartWrong()", this);

            crWrong = StartCoroutine(coWrong());
            return crWrong;
        }
        public void StopWrong()
        {
            LOG.Info($"StopWrong()", this);

            this.StopCoroutineSafe(ref crWrong);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            AudioMGR.One.PlayEffect(wrongCLIP);
            anim.SetTrigger("Wrong");
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            if (enable)
                isSubmit = false;

            btn.interactable = enable;
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private Bottle bottle_ = null;
        private Bottle bottle => bottle_ ??= GetComponent<Bottle>();

        // Fields 
        private bool isSubmit = false;
        private ExampleData exam = null;
        private Coroutine crCorrect = null;
        private Coroutine crWrong = null;

        // Event Handlers
        private void btn_onClick()
        {
            isSubmit = true;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button btn = null;
        [SerializeField] private GameObject vfxCorrectGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;

        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(btn_onClick);
            btn.interactable = false;

            vfxCorrectGO.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                AudioMGR.One.PlayEffect(correctCLIP);
                yield return new WaitForSeconds(0.5f);

                anim.SetTrigger("Correct");
                vfxCorrectGO.SetActive(true);
                yield return new WaitForSeconds(0.5f);

                yield return AudioMGR.One.PlayNarrationAndWait(exam.WordCLIP);
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                AudioMGR.One.PlayEffect(wrongCLIP);
                anim.SetTrigger("Wrong");
                yield return new WaitForSeconds(0.5f);
            }
        }



        // Interface : ISubmitable
        public bool IsSubmit => isSubmit;
    }
}