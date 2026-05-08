using beyondi.Coroutine;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A03
{
    [RequireComponent(typeof(Animator))]
    public class NutSet : AffBase,
        IBeginDragHandler, IEndDragHandler, IDragHandler,
        IPointerDownHandler,
        ICompletable
    {
        // Properties
        private float rotation
        {
            get
            {
                return anim.GetFloat("Rotate");
            }
            set
            {
                anim.SetFloat("Rotate", value);
            }
        }

        // Methods
        public void Setup(NutParam nutParam)
        {
            LOG.Info($"Setup()", this);

            spannerGO.SetActive(false);

            param = nutParam;
        }
        public void Ready()
        {
            LOG.Info($"Ready()", this);

            transform.SetAsLastSibling();

            spannerGO.SetActive(true);
            anim.SetBool("Affordance", false);

            var spinVFXPos = new Vector3(transform.position.x, transform.position.y, -1);
            param.spinVFX.transform.position = spinVFXPos;
        }
        public void Affordance(float delay = 0f)
        {
            LOG.Info($"Affordance()", this);

            affDelayTw.Kill();
            affDelayTw = DOVirtual.DelayedCall(delay,
                () =>
                {
                    anim.SetBool("Affordance", true);
                    affTargetGO.SetActive(true);
                }
                );

        }
        public void Finish()
        {
            LOG.Info($"Finish()", this);

            spannerGO.SetActive(false);
            anim.SetBool("Affordance", false);
            rotation = 1;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;

            if (enable)
                currentAngle = 0;
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Canvas canvas_ = null;
        private Canvas canvas => canvas_ ??= GetComponentInParent<Canvas>();
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();



        // Fields
        private NutParam param = null;
        private bool isComplete = false;
        private Tween affDelayTw = null;

        // Fields
        private Vector2 centerPos = Vector2.zero;
        private float currentAngle = 0;
        private bool isDragging = false;

        // Functions
        private float computeAngle(Vector2 position)
        {
            Vector2 v2 = position - centerPos;
            var deg = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
            return (-deg + 360) % 360;
        }

        // Overrides
        protected override IEnumerator onStartAff()
        {
            LOG.Info($"onStartAff()", this);

            LOG.Info($"rotation  {rotation}", this);
            LOG.Info($"currentAngle  {currentAngle}", this);

            //rotation = 0;
            //currentAngle = 0;
            //yield return null;

            affTargetGO.SetActive(true);
            yield return null;

            anim?.SetBool("Affordance", true);
            yield return new WaitForSeconds(param.affDuration);

            finishAff();
        }
        protected override IEnumerator onFinishAff()
        {
            LOG.Info($"onFinishAff()", this);

            affTargetGO.SetActive(false);
            yield return null;

            anim?.SetBool("Affordance", false);
            yield return null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject spannerGO = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            cg.blocksRaycasts = false;
            centerPos = UtilTransform.LocalToScreen(transform.position, rt, canvas);

            affTargetGO.SetActive(false);

            Enabler = () => cg.blocksRaycasts;
        }
        private void Start()
        {
        }



        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler, OnPointerDown
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            isDragging = true;

            affDelayTw.Kill();
            affTargetGO.SetActive(false);

            AudioMGR.One.PlayEffectLL(param.startClip);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            if (isDragging && rotation < 1)
            {
                currentAngle = 0;
                rotation = 0;
            }

            isDragging = false;
            param.spinVFX.gameObject.SetActive(false);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (isDragging)
            {
                var angle = computeAngle(eventData.position);

                var ratio = 0f;
                if (currentAngle - param.maxAngleInterval <= angle && angle <= currentAngle + param.maxAngleInterval)
                {
                    currentAngle = angle;
                    ratio = angle / 360;

                    param.spinVFX.gameObject.SetActive(true);

                    if (ratio >= param.completeRatio)
                    {
                        param.correctVFX.PlayAtPosition(centerPos);

                        ratio = 1;
                        isDragging = false;
                        isComplete = true;
                    }
                }
                else if (currentAngle != 0)
                {
                    isDragging = false;
                    currentAngle = 0;
                }

                rotation = ratio;

                if (!isDragging)
                {
                    param.spinVFX.gameObject.SetActive(false);
                }
            }
        }
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            anim.SetBool("Affordance", false);
        }



        // Interface : ICompletable
        bool ICompletable.IsComplete => isComplete;
    }
}