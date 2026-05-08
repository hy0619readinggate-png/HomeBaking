using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A12
{
    public class Subject : MonoBehaviour,
        IPointerDownHandler,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        // Properties
        public bool IsComplete { get; private set; } = false;

        // Methods
        public void Setup(SubjectData sData)
        {
            LOG.Info($"Setup()", this);

            this.sData = sData;

            wordTXT.text = sData.Word;
            IsComplete = false;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private SubjectData sData = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI wordTXT = null;
        [SerializeField] private GameObject vfxCorrectGO = null;
        [SerializeField] private GameObject overGO = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = true;
            vfxCorrectGO.SetActive(false);
            overGO.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCorrect(Example example)
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                cg.blocksRaycasts = false;
                IsComplete = true;
                yield return null;
                
                example.transform.position = transform.position;
                example.transform.SetParent(transform);
                vfxCorrectGO.SetActive(true);
                Millo.One.Correct();
                yield return new WaitForSeconds(0.5f);

                yield return AudioMGR.One.PlayNarrationAndWait(example.WordCLIP);
            }
        }
        IEnumerator coWrong(Example example)
        {
            using (LOG.Coroutine($"coWrong()", this))
            {
                ActivityProgress.One.Wrong();
                Millo.One.Wrong();
                example.Wrong();
                yield return null;
            }
        }



        // Interface : IDropHandler
        public void OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            AudioMGR.One.PlayNarration(sData.WordCLIP);
        }

        // Interface : IDropHandler, IPointerEnterHandler, IPointerExitHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var example = eventData.pointerDrag.GetComponent<Example>();
            if (example != null)
            {
                overGO.SetActive(false);

                var correct = example.Word == sData.Word;
                if (correct)
                {
                    eventData.Use();

                    StartCoroutine(coCorrect(example));
                }
                else StartCoroutine(coWrong(example));
            }
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (Example.CurrentDrag != null)
                overGO.SetActive(true);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (Example.CurrentDrag != null)
                overGO.SetActive(false);
        }
    }
}