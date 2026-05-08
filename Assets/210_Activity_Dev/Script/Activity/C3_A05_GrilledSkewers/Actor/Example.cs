using beyondi.Coroutine;
using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C3_A05
{
    public class Example : MonoBehaviour, IPointerDownHandler, ISubmitable, IID
    {
        // Properties
        public int ID { get; set; }
        public bool IsSubmit { get; private set; }
        public bool IsAnswer => exam.IsAnswer;

        // Methods
        public void Setup(ExampleData exam)
        {
            LOG.Info($"Setup()", this);

            this.exam = exam;

            exampleTXT.text = exam.Sentence;

            ingredients.SetActiveOnly(exam.IngredientID - 1);

        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
            if (enable)
                IsSubmit = false;
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            correctGO.SetActive(false);
            correctGO.SetActive(true);
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private GameObject[] ingredients_ = null;
        private GameObject[] ingredients => ingredients_ ??= ingredientTR.GetChildrenAsGameObject().ToArray();

        // Fields
        private ExampleData exam = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI exampleTXT = null;
        [SerializeField] private Transform ingredientTR = null;
        [SerializeField] private GameObject correctGO = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            correctGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            IsSubmit = true;
        }
    }
}