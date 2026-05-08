using beyondi.Behaviour;
using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C1_G01
{
    public class IceCreamCup : BYDSingleton<IceCreamCup>,
        IDropHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Properties
        public int[] ColorIDs => getColors();
        public bool IsRespawning => isRespawning;
        public int IceCreamCount => count;

        // Methods
        public void Respawn(float delay = 0)
        {
            LOG.Info($"Respawn() | {delay}", this);

            if (!isRespawning)
            {
                isRespawning = true;
                isDrag = false;

                StartCoroutine(coRespawn(delay));
            }
        }
        public void ExplodeAndRespawn()
        {
            LOG.Info($"ExplodeAndRespawn()", this);

            if (!isRespawning)
            {
                isRespawning = true;
                isDrag = false;

                StartCoroutine(coExplodeAndRespawn());
            }
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= rt.AddComponent<CanvasGroup>();
        private AffBase aff_ = null;
        private AffBase aff => aff_ ??= GetComponent<AffBase>();

        // Fields
        private Transform originParent = null;
        private int originSiblingIndex;
        private Vector3 originPosition;
        private int count = 0;
        private bool isDrag = false;
        private bool isRespawning = false;

        // Functions
        private bool canPileUp => count < iceCreams.Length;
        private int[] getColors()
        {
            return iceCreams.Take(count).Select(i => i.ColorID).ToArray();
        }

        // Functions
        private void pileUpIceCream(int colorID)
        {
            var current = iceCreams[count++];
            current.gameObject.SetActive(true);
            current.Setup(colorID);
            current.Drop();
        }
        private void clearIceCream()
        {
            iceCreams.ForEach(i => i.gameObject.SetActive(false));
            count = 0;
        }
        private void returnToOrigin()
        {
            rt.DOJump(originPosition, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() => cg.blocksRaycasts = true);
        }
        private bool enableAff()
        {
            return IceCreamCount > 0;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject rigGO = null;
        [SerializeField] private IceCream[] iceCreams = null;
        [SerializeField] private GameObject respawnVfxGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pileUpIceCreamCLIP = null;
        [SerializeField] private AudioClip pickupCLIP = null;
        [SerializeField] private AudioClip returnCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float respawnDuration = 0.2f;
        [SerializeField] private float explodeDuration = 1f;
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            cg.blocksRaycasts = true;
            respawnVfxGO.SetActive(false);

            aff.Enabler = enableAff;

            clearIceCream();
        }
        private void Start()
        {
            originPosition = transform.position;
            originParent = transform.parent;
            originSiblingIndex = transform.GetSiblingIndex();
        }

        // Unity Coroutine
        IEnumerator coRespawn(float delay)
        {
            using (LOG.Coroutine($"coRespawn()", this))
            {
                transform.SetParent(originParent);
                transform.position = originPosition;
                transform.SetSiblingIndex(originSiblingIndex);
                rigGO.SetActive(false);
                yield return new WaitForSeconds(delay);
                yield return null;

                clearIceCream();
                rigGO.SetActive(true);
                yield return new WaitForSeconds(respawnDuration);

                cg.blocksRaycasts = true;
                yield return null;

                isRespawning = false;
            }
        }
        IEnumerator coExplodeAndRespawn()
        {
            using (LOG.Coroutine($"coExplodeAndRespawn()", this))
            {
                respawnVfxGO.SetActive(true);
                yield return new WaitForSeconds(explodeDuration / 2);

                rigGO.SetActive(false);
                yield return new WaitForSeconds(explodeDuration / 2);

                respawnVfxGO.SetActive(false);
                yield return null;

                clearIceCream();

                transform.SetParent(originParent);
                transform.position = originPosition;
                transform.SetSiblingIndex(originSiblingIndex);
                rigGO.SetActive(true);
                yield return null;

                cg.blocksRaycasts = true;
                yield return null;

                isRespawning = false;
            }
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var tin = eventData.pointerDrag.GetComponent<IceCreamTin>();
            if (tin != null && canPileUp)
            {
                AudioMGR.One.PlayEffect(pileUpIceCreamCLIP);

                pileUpIceCream(tin.ColorID);
                eventData.Use();
            }
        }

        // Interface : IBeginDragHandler, IEndDragHandler, IDragHandler
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            if (!isRespawning)
            {
                isDrag = true;
                cg.blocksRaycasts = false;

                AudioMGR.One.PlayEffect(pickupCLIP);
            }
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (isDrag)
            {
                var cam = eventData.pressEventCamera;
                var pos = eventData.position;

                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, pos, cam, out var ptWorld))
                    rt.position = ptWorld;
            }
        }
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            LOG.Info($"OnEndDrag()", this);

            if (isDrag)
            {
                if (!eventData.used)
                {
                    returnToOrigin();
                    AudioMGR.One.PlayEffect(returnCLIP);
                }

                isDrag = false;
            }
        }
    }
}