using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C2_A02
{
    public class TextArea : MonoBehaviour,
        IDropHandler,
        IID
    {
        // Definitions
        private enum State { Blank, Focus, Correct }

        // Properties
        public string Text => sData.Text;
        public bool IsComplete => isComplete;

        // Methods
        public void Setup(SubjectData sData)
        {
            LOG.Info($"Setup() | {sData.Text} | {sData.IsFix}", this);

            this.sData = sData;

            isComplete = sData.IsFix;

            blankTXT.text = sData.Text;
            focusTXT.text = sData.Text;
            correctTXT.text = sData.Text;
            correctTXT.DOFade(1, 0);

            updateState(sData.IsFix ? State.Correct : State.Blank);
        }
        public void Focus()
        {
            LOG.Info($"Focus()", this);

            updateState(State.Focus);
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            isComplete = true;

            vfxCorrectGO.SetActive(false);
            vfxCorrectGO.SetActive(true);

            updateState(State.Correct);
        }
        public void Hide(float duration)
        {
            correctTXT.DOFade(0, duration * 0.5f);
        }

        // Events
        public event Action<S1ExampleText, TextArea> OnDrop;



        // Fields
        private SubjectData sData = null;
        private bool isComplete = false;

        // Functions
        private void updateState(State state)
        {
            blankTXT.gameObject.SetActive(state == State.Blank);
            focusTXT.gameObject.SetActive(state == State.Focus);
            correctTXT.gameObject.SetActive(state == State.Correct);
        }




        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI focusTXT = null;
        [SerializeField] private TextMeshProUGUI blankTXT = null;
        [SerializeField] private TextMeshProUGUI correctTXT = null;
        [SerializeField] private GameObject vfxCorrectGO = null;

        // Unity Messages
        private void Awake()
        {
            vfxCorrectGO.SetActive(false);
        }
        private void Start()
        {
        }



        // Interface : IDropHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var exampleText = eventData.pointerDrag.GetComponent<S1ExampleText>();

            LOG.Info($"-------------- {exampleText}, {IsComplete}", this);
            if (exampleText != null && !isComplete)
            {
                OnDrop?.Invoke(exampleText, this);

                eventData.Use();
            }
        }

        // Interface : IID
        public int ID { get; set; }
    }
}