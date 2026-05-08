using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace DoDoEng.Game.C2_G02
{
    public class PeachItem : MonoBehaviour,
        IPointerDownHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IBYDPooledObject<PeachItem>
    {
        // Definitions
        public const float SPAWN_DURATION = 0.66f;
        private const float TAKE_DURATION = 0.3f;

        // Properties
        public static PeachItem CurrentDrag { get; private set; } = null;

        // Properties
        public BulletData BulletData { get; private set; }

        // Methods
        public void Setup(BulletData bulletData)
        {
            LOG.Info($"Setup() | {bulletData}", this);

            BulletData = bulletData;

            wordIMG.sprite = bulletData.WordSPR;

            cg.alpha = 1;
            transform.localScale = Vector3.one;
        }
        public void StartMove(Vector3 startPos, Vector3 finishPos, float speed)
        {
            LOG.Info($"StartMove()", this);

            crMove = StartCoroutine(coMove(startPos, finishPos, speed));
        }
        public void Take(Vector3 jumpPos)
        {
            LOG.Info($"Take()", this);

            cg.DOFade(0.7f, TAKE_DURATION);
            transform.DOScale(0.5f, TAKE_DURATION);
            transform.DOJump(jumpPos, takeJumpPower, 1, TAKE_DURATION)
                .OnComplete(() => Pool.Release(this));
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private Coroutine crMove = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image wordIMG = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pickupCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float takeJumpPower = 1f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {

        }

        // Unity Coroutine
        IEnumerator coMove(Vector3 startPos, Vector3 finishPos, float speed)
        {

            transform.position = startPos;
            yield return new WaitForSeconds(SPAWN_DURATION);

            cg.blocksRaycasts = true;
            var distance = Vector3.Distance(startPos, finishPos);
            var duration = distance / speed;
            yield return transform.DOMove(finishPos, duration).SetEase(Ease.Linear).WaitForCompletion();

            Pool.Release(this);
        }


        // Interface : IPointerDown
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            AudioMGR.One.PlayNarration(BulletData.WordCLIP);
        }

        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            CurrentDrag = this;

            cg.blocksRaycasts = false;
            DOTween.Kill(transform);
            this.StopCoroutineSafe(ref crMove);
            transform.SetAsLastSibling();

            AudioMGR.One.PlayEffect(pickupCLIP);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            var cam = eventData.pressEventCamera;
            var pos = eventData.position;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                rt.position = ptWorld;
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            if (!eventData.used)
                Pool.Release(this);

            if (CurrentDrag == this)
                CurrentDrag = null;
        }

        // Interface : IBYDPooledObject<T>
        public IObjectPool<PeachItem> Pool { get; set; }
    }
}
