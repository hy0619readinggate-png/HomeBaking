using beyondi.Coroutine;
using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A01
{
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(Animator))]
    public class RecordButton : MonoBehaviour, ISubmitable
    {
        // Definitions
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;

            if (enable)
                isSubmit = false;
        }

        // Methods
        public void Show()
        {
            LOG.Info($"Show()", this);

            anim.SetTrigger("Idle");
        }
        public void Recording()
        {
            LOG.Info($"Recording()", this);

            anim.SetTrigger("Recording");
        }
        public void StopAndHide()
        {
            LOG.Info($"StopAndHide()", this);

            anim.SetTrigger("Hidden");
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private bool isSubmit = false;

        // Event Handlers
        public Action OnClick;



        // Unity Messages
        private void Awake()
        {
            EnableInteraction(false);

            btn.onClick.AddListener(() => isSubmit = true);

            anim.SetTrigger("Hidden");
        }
        private void Start()
        {
        }



        // Interface : ISubmitable
        public bool IsSubmit => isSubmit;
    }
}