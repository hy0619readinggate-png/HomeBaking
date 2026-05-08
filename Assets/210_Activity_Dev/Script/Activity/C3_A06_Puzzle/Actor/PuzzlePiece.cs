using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C3_A06
{
    public class PuzzlePiece : MonoBehaviour,
        IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        // Properties
        public static PuzzlePiece CurrentDrag { get; private set; } = null;
        public int ID { get; private set; }
        public bool IsAvailable { get; private set; }

        // Methods
        public void Setup(int ID, Sprite sprite)
        {
            LOG.Info($"Setup() | {ID}", this);

            this.ID = ID;

            anims.ForEach(a => a.SetTrigger("Hidden"));
            images.ForEach(i => i.sprite = sprite);

            transform.localPosition = originPosition;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            AudioMGR.One.PlayEffect(piecesCLIP);

            currentAnim.SetTrigger("Show");
            IsAvailable = true;

            cg.blocksRaycasts = true;
        }
        public void Shown()
        {
            LOG.Info($"Shown()", this);

            currentAnim.SetTrigger("Normal");
            IsAvailable = true;

            cg.blocksRaycasts = true;
        }
        public void Hidden()
        {
            LOG.Info($"Hidden()", this);

            currentAnim.SetTrigger("Hidden");
            IsAvailable = false;

            cg.blocksRaycasts = false;
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator[] anims_ = null;
        private Animator[] anims => anims_ ??= GetComponentsInChildren<Animator>(true);

        // Fields
        private Vector3 originPosition;
        private int childCount;
        private int originSiblingIndex;

        // Functions
        private void returnToOrigin()
        {
            rt.DOLocalJump(originPosition, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() =>
                {
                    currentAnim.SetTrigger("Normal");
                    transform.SetSiblingIndex(originSiblingIndex);
                    cg.blocksRaycasts = true;
                });
        }
        private Animator currentAnim => anims[ID - 1];



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image[] images = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip piecesCLIP = null;
        [SerializeField] private AudioClip dragCLIP = null;
        [SerializeField] private AudioClip incorrectCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            originPosition = transform.localPosition;
            cg.blocksRaycasts = false;
            childCount = transform.parent.childCount;
            originSiblingIndex = transform.GetSiblingIndex();
        }
        private void Start()
        {
        }



        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            transform.SetSiblingIndex(childCount);
            cg.blocksRaycasts = false;
            currentAnim.SetTrigger("Grab");

            AudioMGR.One.PlayEffect(dragCLIP);

            CurrentDrag = this;
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //LOG.Info($"OnDrag()", this);

            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            var drop = eventData.used;
            if (drop)
            {
                transform.SetSiblingIndex(originSiblingIndex);
                currentAnim.SetTrigger("Hide");
                IsAvailable = false;
            }
            else
            {
                AudioMGR.One.PlayEffect(incorrectCLIP);
                returnToOrigin();
            }

            if (CurrentDrag == this)
                CurrentDrag = null;
        }
    }
}