using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A09
{
    [RequireComponent(typeof(Animator))]
    public class PopcornExam : Popcorn, IPointerDownHandler
    {
        // Properties
        public bool IsAnswer { get; private set; }
        public string Text { get; private set; }

        // Methods
        public void Setup(ExampleData data, PopcornAniParam popcornAniParam)
        {
            LOG.Info($"Setup() | {data}", this);

            IsAnswer = data.IsAnswer;
            Text = data.Text;
            param = popcornAniParam;

            var variation = UtilArray.RandomOne(0, popcornVariationGO.Length - 1);
            foreach (var (go, i) in popcornVariationGO.Select((go, i) => (go, i)))
                go.SetActive(i == variation);
            alphabetTXT.ForEach(txt => txt.text = data.Text);
        }
        public void DisableInteraction()
        {
            LOG.Info($"DisableInteraction()", this);

            cg.blocksRaycasts = false;
        }
        public void DoCorrect()
        {
            LOG.Info($"DoCorrect()", this);

            stopFly();
            IsCollected = true;
            StartCoroutine(coCorrect());
        }
        public void DoWrong(bool collect = true)
        {
            LOG.Info($"DoWrong()", this);

            stopFly();
            IsCollected = collect;
            StartCoroutine(coWrong());
        }

        // Events
        public event Action<PopcornExam> OnSubmit;
        public event Action<PopcornExam> OnCollected;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private PopcornAniParam param = null;

        // Overrides
        protected override void onFired()
        {
            cg.blocksRaycasts = true;
        }
        protected override void onStartAff()
        {
            base.onStartAff();

            anim.SetTrigger("Affordance");
        }
        protected override void onStopAff()
        {
            base.onStopAff();

            anim.SetTrigger("Idle");
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI[] alphabetTXT = null;
        [SerializeField] private GameObject[] popcornVariationGO = null;
        [SerializeField] private GameObject glitterGO = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            cg.blocksRaycasts = false;
            glitterGO.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCorrect()
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                AudioMGR.One.PlayEffect(param.correctCLIP);
                anim.SetTrigger("Answer");
                yield return new WaitForSeconds(param.correctDuration);

                anim.enabled = false;
                yield return null;

                AudioMGR.One.PlayEffect(param.collectCLIP);
                glitterGO.SetActive(true);
                rt.DOJump(
                    param.collectTargetTR.position,
                    param.collectJumpPower, 1,
                    param.collectDuration);
                rt.DOScale(
                    param.collectScale,
                    param.collectDuration);
                rt.DORotate(
                    new Vector3(0, 0, 360),
                    param.collectDuration / param.collectRotateCount,
                    RotateMode.WorldAxisAdd).SetLoops(param.collectRotateCount).SetEase(Ease.Linear);
                cg.DOFade(
                    param.collectAlpha,
                    param.collectDuration);
                yield return new WaitForSeconds(param.collectDuration);

                OnCollected?.Invoke(this);
                gameObject.SetActive(false);
            }
        }
        IEnumerator coWrong()
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                AudioMGR.One.PlayEffect(param.wrongCLIP);
                //anim.SetTrigger("Wrong");
                yield return new WaitForSeconds(param.wrongDuration);

                gameObject.SetActive(false);
            }
        }


        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.VeryImportant($"OnPointerDown() | {IsAnswer}", this);

            cg.blocksRaycasts = false;

            OnSubmit?.Invoke(this);
        }
    }
}