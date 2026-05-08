using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


namespace DoDoEng.Activity.C1_A03
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(ParticleSystemForceField))]
    public class DryGun : AffBase,
        IPointerDownHandler, IPointerUpHandler,
        IDragHandler
    {
        // Properties
        public GameObject BlowArea => GetComponentInChildren<BoxCollider2D>(true).gameObject;
        public bool IsBlowing { get; internal set; }


        // Methods
        public void Setup(GunParam gunParam)
        {
            LOG.Info($"Setup()", this);

            param = gunParam;
        }
        public void Affordance()
        {
            LOG.Info($"Affordance()", this);

            anim.SetTrigger("startAff");
        }



        // Fields : caching
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= transform.parent.GetComponent<RectTransform>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private ParticleSystemForceField forceField_ = null;
        private ParticleSystemForceField forceField => forceField_ ??= GetComponent<ParticleSystemForceField>();

        // Fields
        private GunParam param;



        // Overrides
        protected override IEnumerator onStartAff()
        {
            anim?.SetTrigger("startAff");
            yield return new WaitForSeconds(param.affDuration);

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            anim.SetTrigger("abortAff");
            yield return null;
        }



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            IsBlowing = false;

            forceField.enabled = false;

            BlowArea.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            IsBlowing = true;
            forceField.enabled = true;

            var pos = UtilTransform.ScreenToLocal(eventData.position, rt, canvas);
            transform.localPosition = pos;

            BlowArea.SetActive(true);

            anim.SetTrigger("on");
            AudioMGR.One.PlayEffectLL(param.gunOnClip);

        }

        // Interface : IPointerDownHandler, IPointerUpHandler
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            LOG.Info($"OnPointerUp()", this);

            IsBlowing = false;
            forceField.enabled = false;

            BlowArea.SetActive(false);

            anim.SetTrigger("abortAff");
            AudioMGR.One.StopEffectLL(false);


        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //LOG.Info($"OnDrag()", this);

            var pos = UtilTransform.ScreenToLocal(eventData.position, rt, canvas);
            transform.localPosition = pos;
        }


    }
}