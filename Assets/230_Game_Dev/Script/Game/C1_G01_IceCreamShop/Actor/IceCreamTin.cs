using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Game.C1_G01
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Animator))]
    public class IceCreamTin : MonoBehaviour,
        IID,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler,
        IPointerDownHandler
    {
        // Properties
        public int ColorID => iceCreamData?.ColorID ?? 0;

        // Methods
        public void Setup(IceCreamData icd)
        {
            LOG.Info($"Setup() | {icd}", this);

            iceCreamData = icd;

            if (icd != null)
            {
                alphabetTMP.text = icd.Alphabet;
                alphabetTMP.color = ColorTable.One.TinTextColor(icd.ColorID);

                var reverse = icd.ColorID != ID;
                iceCream1GO.SetActive(!reverse);
                iceCream2GO.SetActive(reverse);
            }
            else alphabetTMP.text = string.Empty;
        }
        public void OpenCover(float delay)
        {
            LOG.Info($"OpenCover() | {delay}", this);

            if (isUsing)
            {
                DOVirtual.DelayedCall(delay, () =>
                {
                    anim.SetTrigger("open");
                    cg.blocksRaycasts = true;
                });
            }
        }
        public void CloseCover(float delay)
        {
            LOG.Info($"CloseCover() | {delay}", this);

            if (isUsing)
            {
                DOVirtual.DelayedCall(delay, () =>
                {
                    anim.SetTrigger("close");
                    cg.blocksRaycasts = false;
                });
            }
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Image img_ = null;
        private Image img => img_ ??= GetComponent<Image>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private AffBase aff_ = null;
        private AffBase aff => aff_ ??= GetComponent<AffBase>();

        // Fields
        private IceCreamData iceCreamData;

        // Functions
        private bool isUsing => iceCreamData != null;
        private bool enableAff()
        {
            return isUsing && IceCreamCup.One.IceCreamCount == 0;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject iceCream1GO = null;
        [SerializeField] private GameObject iceCream2GO = null;
        [SerializeField] private TextMeshProUGUI alphabetTMP = null;
        [SerializeField] private IceCreamFloat iceCreamFloat = null;

        // Unity Messages
        private void Awake()
        {
            var grs = GetComponentsInChildren<Graphic>(true);
            grs.ForEach(gr => gr.raycastTarget = false);
            img.raycastTarget = true;

            cg.blocksRaycasts = false;
            aff.Enabler = enableAff;
        }
        private void Start()
        {
        }




        // Interface : IID
        public int ID { get; set; }

        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            iceCreamFloat.Pickup(ColorID, eventData);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            iceCreamFloat.Locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            var drop = eventData.used;
            if (drop)
                iceCreamFloat.Drop();
            else iceCreamFloat.ReturnTo(transform.position);
        }

        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            AudioMGR.One.PlayNarration(iceCreamData.SoundCLIP);
        }
    }
}