using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A08
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class LeverButton : Graphic, IPointerDownHandler
    {
        // Methods
        public Coroutine StartWaitClick()
        {
            LOG.Info($"StartWaitClick()", this);

            this.StopCoroutineSafe(ref crWaitClick);

            crWaitClick = StartCoroutine(coWaitClick());
            return crWaitClick;
        }
        public void FinishWaitClick()
        {
            LOG.Info($"FinishWaitClick()", this);

            this.StopCoroutineSafe(ref crWaitClick);
            raycastTarget = false;
        }



        // Fields
        private Coroutine crWaitClick = null;
        private bool isClicked = false;

        // Overrides
        public override bool Raycast(Vector2 sp, Camera eventCamera)
        {
            // https://younitystudy.tistory.com/m/75
            return true;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }



        // Unity Inspectors
        [Header("★ Audios")]
        [SerializeField] private AudioClip clickCLIP = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            raycastTarget = false;
        }
        protected override void Start()
        {
            base.Start();
        }

        // Unity Coroutine
        IEnumerator coWaitClick()
        {
            using (LOG.Coroutine($"coWaitClick()", this))
            {
                isClicked = false;
                raycastTarget = true;
                yield return new WaitUntil(() => isClicked);

                raycastTarget = false;
                yield return null;
            }
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown() | {eventData.position}", this);

            if (clickCLIP != null)
                AudioMGR.One.PlayEffect(clickCLIP);

            isClicked = true;
        }
    }
}