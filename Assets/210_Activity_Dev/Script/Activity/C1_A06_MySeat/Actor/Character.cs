using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using FlexFramework.Excel;
using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A06
{
    public class Character : MonoBehaviour,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Properties
        public int CharacterID { get; private set; }

        // Methods
        public void Setup(int id)
        {
            LOG.Info($"Setup() | {id}", this);

            CharacterID = id;
            characters.SetActiveOnly(id - 1);
            activeCharacter = characters.Single(c => c.gameObject.activeSelf);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Snapped(bool snap)
        {
            LOG.Info($"Snapped() | {snap}", this);

            cg.alpha = snap ? 0 : 1;
        }

        // Events
        public event Action OnBeginDrag;
        public event Action OnEndDrag;



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= rt.AddComponent<CanvasGroup>();

        // Fields
        private CharacterAni activeCharacter = null;

        // Functions
        private void returnToOrigin()
        {
            transform.DOJump(originTR.position, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() => cg.blocksRaycasts = true);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CharacterAni[] characters = null;
        [SerializeField] private RectTransform pickPointRT = null;
        [SerializeField] private Transform originTR = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }



        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            activeCharacter.PlayAnimationLoop(CharacterAnimation.Pick);

            cg.blocksRaycasts = false;

            OnBeginDrag?.Invoke();
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            //var pos = eventData.position - pickPointRT.anchoredPosition;
            var pos = eventData.position;
            var delta = transform.position - pickPointRT.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                transform.position = ptWorld + delta;
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            if (!eventData.used)
            {
                activeCharacter.PlayAnimationLoop(CharacterAnimation.Idle1);

                returnToOrigin();
            }
            else
            {
                transform.position = originTR.position;
                gameObject.SetActive(false);
            }

            AudioMGR.One.StopEffectLL();

            OnEndDrag?.Invoke();
        }
    }
}
