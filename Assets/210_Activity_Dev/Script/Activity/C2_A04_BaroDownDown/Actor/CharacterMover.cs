using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A04
{
    public class CharacterMover : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
    {
        // Properties
        public bool IsComplete { get; private set; }

        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
            IsComplete = false;

            if (enable)
                character.StartDiving();
            else character.FinishDiving();
        }
        public Coroutine ReturnToCenter(bool idlePose)
        {
            LOG.Info($"ReturnToCenter() | {idlePose}", this);

            crMoveToCenter = StartCoroutine(coMoveToCenter(idlePose));
            return crMoveToCenter;
        }
        public void CenterNow()
        {
            LOG.Info($"CenterNow()", this);

            transform.DOKill();
            this.StopCoroutineSafe(ref crMoveToCenter);
            character.transform.position = originPosition;
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private bool isDown = false;
        private Vector3 originPosition;
        private Coroutine crMoveToCenter = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Character character = null;
        [Header("★ Timing")]
        [SerializeField] private float returnSpeed = 2.5f;
        [SerializeField] private float idlePoseDuration = 1.06f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            originPosition = character.transform.position;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coMoveToCenter(bool idlePose)
        {
            using (LOG.Coroutine($"coMoveBubble() | {idlePose}", this))
            {

                var distance = Vector2.Distance(character.transform.position, originPosition);
                var duration = distance / returnSpeed;

                yield return character.transform.DOMoveX(originPosition.x, duration).WaitForCompletion();

                if (idlePose)
                {
                    character.DivingToIdlePose();
                    yield return new WaitForSeconds(idlePoseDuration);
                }
                IsComplete = true;
            }
        }



        // Interface : IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            isDown = true;
            character.MoveTo(eventData.position);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp()", this);

            isDown = false;
        }
        void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
        {
            //LOG.Info($"OnPointerMove()", this);

            if (isDown)
                character.MoveTo(eventData.position);
        }
    }
}