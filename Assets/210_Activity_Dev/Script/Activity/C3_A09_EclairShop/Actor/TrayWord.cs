using DoDoEng.Common;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C3_A09
{
    [RequireComponent(typeof(Animator))]
    public class TrayWord : MonoBehaviour
    {
        // Properties
        public string Text => txt.text;

        // Methods
        public void Setup(string text)
        {
            LOG.Function(this);

            txt.text = text;
            anim.SetTrigger("Hidden");
        }
        public void Show()
        {
            LOG.Function(this);

            anim.SetTrigger("Show");
        }
        public void Hidden()
        {
            LOG.Function(this);

            anim.SetTrigger("Hidden");
        }
        public void Idle()
        {
            LOG.Function(this);

            anim.SetTrigger("Idle");
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI txt = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}