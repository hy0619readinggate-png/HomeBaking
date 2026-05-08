using DG.Tweening;
using DoDoEng.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C1_G01
{
    public class IceCreamFloat : MonoBehaviour
    {
        // Methods
        public void Pickup(int colorID, PointerEventData eventData)
        {
            LOG.Info($"Pickup() | {colorID}", this);

            AudioMGR.One.PlayEffect(pickupCLIP);

            cancelReturn();
            locateIceCream(eventData);

            iceCream.Setup(colorID);
            iceCream.gameObject.SetActive(true);
        }
        public void Locate(PointerEventData eventData)
        {
            //LOG.Info($"Locate()", this);

            locateIceCream(eventData);
        }
        public void Drop()
        {
            LOG.Info($"Drop()", this);

            iceCream.gameObject.SetActive(false);
        }
        public void ReturnTo(Vector3 returnTo)
        {
            LOG.Info($"ReturnTo() | {returnTo}", this);

            tw1 = cg.DOFade(0, returnJumpDuration).SetEase(Ease.InExpo);
            tw2 = rt.DOJump(returnTo, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() =>
                {
                    rt.gameObject.SetActive(false);
                    cg.alpha = 1;
                });

            AudioMGR.One.PlayEffect(returnCLIP);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= iceCream.GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= rt.AddComponent<CanvasGroup>();

        // Fields
        private Tween tw1 = null;
        private Tween tw2 = null;

        // Functions
        private void locateIceCream(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
        }
        private void cancelReturn()
        {
            tw1.Complete(true);
            tw2.Complete(true);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private IceCream iceCream = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pickupCLIP = null;
        [SerializeField] private AudioClip returnCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            rt.gameObject.SetActive(false);
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }
    }
}