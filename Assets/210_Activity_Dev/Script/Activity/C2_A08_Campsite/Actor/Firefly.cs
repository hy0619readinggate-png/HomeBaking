using DG.Tweening;
using DoDoEng.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A08
{
    public class Firefly : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler,
        IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        // Properties
        public bool IsActivated => isActivated;

        // Methods
        public void Setup(bool activation)
        {
            LOG.Info($"Setup() | {activation}", this);

            activate(activation);
        }
        public void Hide()
        {
            activate(false);
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= rt.AddComponent<CanvasGroup>();

        // Fields
        private Vector3 originPosition;
        private bool isActivated = false;

        // Functions
        private void ReturnDragFirefly()
        {
            dragFirefly.RT.DOLocalJump(originPosition, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() =>
                {
                    dragFirefly.gameObject.SetActive(false);

                    activate(true);
                });
        }
        private void activate(bool active)
        {
            isActivated = active;
            cg.alpha = active ? 1f : 0f;
            cg.blocksRaycasts = active;
        }
        private CollectBottle hitTest(Vector3 pos)
        {

            var origin = pos - Vector3.forward;
            var direction = Vector3.forward;
            var hit = Physics2D.Raycast(origin, direction, 2);

            return hit.collider?.GetComponentInParent<CollectBottle>();
        }
        private void dropToBottle()
        {
            dragFirefly.Drop();
            AudioMGR.One.PlayEffect(collectCLIP);
            cg.alpha = 0f; // fadeout
            Hide();

        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private FireflyAni fireflyAni = null;
        [SerializeField] private FloatFirefly dragFirefly = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip dragCLIP = null;
        [SerializeField] private AudioClip dropCLIP = null;
        [SerializeField] private AudioClip collectCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        private void Awake()
        {
            originPosition = transform.localPosition;
        }
        private void Start()
        {
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            fireflyAni.PlayAnimation(FireflyAnimation.Drag);
        }
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {

            LOG.Info($"OnPointerUp() | {eventData.position}", this);

            fireflyAni.PlayAnimationLoop(FireflyAnimation.Idle);
        }

        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag() | {eventData.position}", this);

            AudioMGR.One.PlayEffect(dragCLIP);

            activate(false);

            dragFirefly.Drag(eventData);

            AudioMGR.One.PlayEffect(dragCLIP);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            //LOG.Info($"OnDrag()", this);

            dragFirefly.Locate(eventData);
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag() | {eventData.position}", this);

            var drop = eventData.used;
            if (drop)
            {
                dropToBottle();
            }
            else
            {
                var bottle = hitTest(dragFirefly.transform.position);
                if (bottle != null)
                {
                    dropToBottle();

                    bottle.ForceCollect();
                }
                else
                {
                    dragFirefly.PlayFireflyAnimation(FireflyAnimation.Move);
                    AudioMGR.One.PlayEffect(dropCLIP);
                    ReturnDragFirefly();
                }
            }
        }
    }
}