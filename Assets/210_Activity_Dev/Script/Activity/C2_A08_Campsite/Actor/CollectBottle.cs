using beyondi.Util;
using DoDoEng.Common;
using Spine;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A08
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Bottle))]
    public class CollectBottle : MonoBehaviour, IDropHandler
    {
        // Methods
        public void Setup(Sprite img, AudioClip wordClip, int maxCount)
        {
            LOG.Info($"Setup() | {maxCount}", this);

            collectCount = 0;
            maxCollectCount = maxCount;

            for (var i = 0; i < collectedFireflies.Length; i++)
                collectedFireflies[i].Setup();

            bottle.Setup(img, true);

            this.wordClip = wordClip;
        }
        public Coroutine StartWaitCollectAll()
        {
            LOG.Info($"StartWaitCollectAll()", this);

            crWaitCollectAll = StartCoroutine(coWaitCollectAll());
            return crWaitCollectAll;
        }
        public void StopWaitCollectAll()
        {
            LOG.Info($"StopWaitCollectAll()", this);

            cg.blocksRaycasts = false;
            this.StopCoroutineSafe(ref crWaitCollectAll);
        }
        public Coroutine StartCloseCork()
        {
            LOG.Info($"StartCloseCork()", this);

            crCloseCork = StartCoroutine(coCloseCork());
            return crCloseCork;
        }
        public void StopCloseCork()
        {
            LOG.Info($"StopCloseCork()", this);

            this.StopCoroutineSafe(ref crCloseCork);

            anim.SetTrigger("CorkClosed");
        }
        public void ShowNow()
        {
            LOG.Info($"ShowNow()", this);

            anim.SetTrigger("AppearIdle");
        }
        public void ShowFireflies()
        {
            LOG.Info($"ShowFireflies()", this);

            if (!isCollectAll)
            {
                for (int i = 0; i < maxCollectCount; i++)
                    collectedFireflies[i].Show();
                isCollectAll = true;
            }
        }
        public void ForceCollect()
        {
            LOG.Info($"DroppedOutOfScreen()", this);

            var bottleFirefly = collectedFireflies[collectCount++];
            bottleFirefly.Show();

            if (collectCount >= maxCollectCount)
                isCollectAll = true;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private Bottle bottle_ = null;
        private Bottle bottle => bottle_ ??= GetComponent<Bottle>();

        // Fields
        private int collectCount = 0;
        private int maxCollectCount;
        private bool isCollectAll = false;
        private AudioClip wordClip = null;
        private Coroutine crWaitCollectAll = null;
        private Coroutine crCloseCork = null;

        // Event Handlers
        private void decoBTN_OnClick()
        {
            LOG.Info($"decoBTN_OnClick()", this);

            AudioMGR.One.PlayNarration(wordClip);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CollectFirefly[] collectedFireflies = null;
        [SerializeField] private AudioClip closeCLIP = null;
        [SerializeField] private Button decoBTN = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            decoBTN.onClick.AddListener(decoBTN_OnClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        private IEnumerator coWaitCollectAll()
        {
            using (LOG.Coroutine($"coWaitCollectAll()", this))
            {
                cg.blocksRaycasts = true;
                isCollectAll = false;
                yield return new WaitUntil(() => isCollectAll);

                cg.blocksRaycasts = false;
            }
        }
        private IEnumerator coCloseCork()
        {
            using (LOG.Coroutine($"coCloseCork()", this))
            {
                anim.SetTrigger("Cork");
                AudioMGR.One.PlayEffect(closeCLIP);
                yield return new WaitForSeconds(1f);
            }
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var firefly = eventData.pointerDrag.GetComponent<Firefly>();
            if (firefly != null && collectCount < collectedFireflies.Length)
            {
                var bottleFirefly = collectedFireflies[collectCount++];
                bottleFirefly.Show();

                eventData.Use();

                if (collectCount >= maxCollectCount)
                    isCollectAll = true;
            }
        }
    }
}