using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C1_A02
{
    [RequireComponent(typeof(Animator))]
    public class Parachute : MonoBehaviour,
        IPointerDownHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IDragHandler
    {
        // Properties
        public bool IsAnswer => exam?.IsAnswer ?? false;

        // Methods
        public void Setup(ExampleData exam, int characterID)
        {
            LOG.Info($"Setup() | {exam}, {characterID}", this);

            this.exam = exam;
            texts.ForEach(t => t.text = exam.Text);
            setCharacter(characterID);
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            cg.blocksRaycasts = true;

            transform.position = originTR.position;
            gameObject.SetActive(true);
            anim.SetTrigger("Up");
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            if (IsAnswer)
                anim.SetTrigger("Hidden");
            else anim.SetTrigger("Hide");
        }
        public void Respawn()
        {
            LOG.Info($"Respawn()", this);

            StartCoroutine(coRespawn());
        }



        // Fields : caching
        private RectTransform rt_ = null;
        private RectTransform rt => rt_ ??= GetComponent<RectTransform>();
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private GameObject[] characters_ = null;
        private GameObject[] characters => characters_ ??= rigTR.GetChildrenAsGameObject().ToArray();
        private TextMeshProUGUI[] texts_ = null;
        private TextMeshProUGUI[] texts => texts_ ??= GetComponentsInChildren<TextMeshProUGUI>(true);

        // Fields
        private ExampleData exam = null;

        // Functions
        private void setCharacter(int characterID)
        {
            characters.ForEach((i, c) => c.SetActive(i == characterID - 1));
        }
        private void returnToOrigin()
        {
            rt.DOJump(originTR.position, returnJumpPower, 1, returnJumpDuration)
                .OnComplete(() => cg.blocksRaycasts = true);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Transform rigTR = null;
        [SerializeField] private Transform originTR = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip pickupCLIP = null;
        [SerializeField] private AudioClip returnCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float returnJumpPower = 2f;
        [SerializeField] private float returnJumpDuration = 0.5f;
        [SerializeField] private float respawnDelay = 1f;
        [SerializeField] private float respawnDuration = 1f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coRespawn()
        {
            anim.SetTrigger("Hidden");
            transform.position = originTR.position;
            yield return new WaitForSeconds(respawnDelay);

            anim.SetTrigger("Up");
            yield return new WaitForSeconds(respawnDuration);

            cg.blocksRaycasts = true;
            yield return null;
        }



        // Interface : IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"IPointerDownHandler()", this);

            AudioMGR.One.PlayNarration(exam.SoundCLIP);
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            LOG.Info($"OnBeginDrag()", this);

            transform.SetAsLastSibling();

            cg.blocksRaycasts = false;

            AudioMGR.One.PlayEffect(pickupCLIP);
            AudioMGR.One.PlayNarration(exam.SoundCLIP);
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
            {
                returnToOrigin();
                AudioMGR.One.PlayEffect(returnCLIP);
            }
        }
    }
}