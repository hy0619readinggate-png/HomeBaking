using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C3_A06
{
    public class PuzzleSlot : MonoBehaviour,
        IID,
        IDropHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        // Properties
        public int ID { get; set; }
        public bool IsComplete { get; private set; } = false;
        public Transform CenterTR => centerTR;

        // Methods
        public void Setup(Sprite sprite)
        {
            LOG.Info($"Setup()", this);

            image.sprite = sprite;
            IsComplete = false;

            anim.SetTrigger("Normal");
        }
        public void Complete()
        {
            LOG.Info($"Complete()", this);

            anim.SetTrigger("Complete");
        }
        public void Completed()
        {
            LOG.Info($"Completed()", this);

            anim.SetTrigger("Completed");
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator anim = null;
        [SerializeField] private Image image = null;
        [SerializeField] private Transform centerTR = null;
        [SerializeField] private GameObject vfxCorrectGO = null;
        [SerializeField] private GameObject vfxOverGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctCLIP = null;

        // Unity Messages
        private void Awake()
        {
            vfxCorrectGO.SetActive(false);
            vfxOverGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var piece = eventData.pointerDrag.GetComponent<PuzzlePiece>();
            if (piece != null)
            {
                vfxOverGO.SetActive(false);

                LOG.Info($"OnDrop() | {ID} == {piece.ID}", this);

                if (ID == piece.ID)
                {
                    IsComplete = true;

                    anim.SetTrigger("Correct");
                    vfxCorrectGO.SetActive(true);
                    AudioMGR.One.PlayEffect(correctCLIP);

                    eventData.Use();
                }
                else
                {
                    ActivityProgress.One.Wrong();
                }
            }
        }

        // Interface : IPointerEnterHandler, IPointerExitHandler
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (PuzzlePiece.CurrentDrag != null && !IsComplete)
                vfxOverGO.SetActive(true);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (PuzzlePiece.CurrentDrag != null)
                vfxOverGO.SetActive(false);
        }
    }
}