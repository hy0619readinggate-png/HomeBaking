using DoDoEng.Common;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Activity.C4_A07
{
    public class IceProblem : MonoBehaviour,
        IDropHandler
    {
        // Properties
        public bool IsBlank => isBlank;

        // Methods
        public void Setup(SubjectData sData)
        {
            LOG.Function(this, $"{sData.Text}, {sData.IsBlank}");

            isBlank = sData.IsBlank;

            if (isBlank)
            {
                anim.SetTrigger("Piece");
                chunkTXT.text = "";
            }
            else chunkTXT.text = sData.Text;
        }

        // Events
        public event Action<bool> OnCorrect;
        public event Action OnWrong;



        // Fields
        private bool isBlank = false;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator anim = null;
        [SerializeField] private TextMeshProUGUI chunkTXT = null;
        [SerializeField] private TextMeshProUGUI wrongTXT = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip fallCLIP = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coCorrect(IceExample example)
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                OnCorrect?.Invoke(true);

                anim.SetTrigger("Correct");

                example.Correct();
                chunkTXT.text = example.Text;
                yield return null;

                OnCorrect?.Invoke(false);
            }
        }
        IEnumerator coWrong(IceExample example)
        {
            using (LOG.Coroutine($"coCorrect()", this))
            {
                OnWrong?.Invoke();

                anim.SetTrigger("Wrong");
                wrongTXT.text = example.Text;
                example.Wrong();
                yield return new WaitForSeconds(0.7f);

                AudioMGR.One.PlayEffect(fallCLIP);
                yield return null;

                yield return new WaitForSeconds(1f);

                anim.SetTrigger("Piece");
                wrongTXT.text = "";
                example.Respawn();
                yield return null;
            }
        }



        // Interface : IDropHandler, IPointerEnterHandler, IPointerExitHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            if (!isBlank)
                return;

            var example = eventData.pointerDrag.GetComponent<IceExample>();
            if (example != null)
            {
                eventData.Use();

                if (example.IsAnswer)
                    StartCoroutine(coCorrect(example));
                else StartCoroutine(coWrong(example));
            }
        }
    }
}