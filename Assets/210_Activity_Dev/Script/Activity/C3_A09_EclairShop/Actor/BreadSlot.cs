using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C3_A09
{
    public class BreadSlot : MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        // Properties
        public bool IsComplete { get; private set; } = false;
        public Transform VfxCorrectTR => correctGO.transform;
        public string Text => subject.Text;

        // Methods
        public void Setup(SubjectData subject)
        {
            LOG.Info($"Setup()", this);

            this.subject = subject;

            IsComplete = false;
            cg.blocksRaycasts = true;

            if (bgGO != null)
                bgGO.SetActive(true);
            normalLineGO.SetActive(true);
            overGO.SetActive(false);
        }
        public void Normal()
        {
            LOG.Function(this);

            normalLineGO.SetActive(true);
            overGO.SetActive(false);
        }

        // Events
        public event Action<Bread, Transform> onBreadCorrect;
        public event Action<Bread, Transform> onBreadWrong;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private SubjectData subject = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject bgGO = null;
        [SerializeField] private GameObject normalLineGO = null;
        [SerializeField] private GameObject overGO = null;
        [SerializeField] private Transform breadParentTR = null;
        [SerializeField] private GameObject correctGO = null;
        [SerializeField] private Animator anim = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            overGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IDropHandler, IPointerEnterHandler, IPointerExitHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            if (IsComplete)
            {
                normalLineGO.SetActive(false);
                overGO.SetActive(false);
                return;
            }

            var bread = eventData.pointerDrag.GetComponent<Bread>();
            if (bread != null)
            {
                normalLineGO.SetActive(false);
                overGO.SetActive(false);

                eventData.Use();

                var correct = bread.Text == subject.Text;
                if (correct)
                {
                    if (bgGO != null)
                        bgGO.SetActive(false);
                    correctGO.SetActive(false);
                    correctGO.SetActive(true);

                    if (anim != null)
                        anim.SetTrigger("Correct");

                    onBreadCorrect?.Invoke(bread, breadParentTR);

                    IsComplete = true;
                }
                else
                {
                    normalLineGO.SetActive(true);
                    overGO.SetActive(false);

                    onBreadWrong?.Invoke(bread, breadParentTR);
                }
            }
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (Bread.CurrentDrag != null)
            {
                normalLineGO.SetActive(false);
                overGO.SetActive(true);
            }
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!IsComplete)
            {
                normalLineGO.SetActive(true);
                overGO.SetActive(false);
            }
        }
    }
}