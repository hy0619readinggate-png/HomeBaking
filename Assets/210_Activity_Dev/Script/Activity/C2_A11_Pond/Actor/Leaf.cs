using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A11
{
    [RequireComponent(typeof(Animator))]
    public class Leaf : MonoBehaviour
    {
        // Methods
        public void Show()
        {
            LOG.Info($"Show()", this);

            anim.SetTrigger("Show");

            if (appearCLIP != null)
                AudioMGR.One.PlayEffect(appearCLIP);
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            anim.SetTrigger("Hide");
        }
        public void Idle()
        {
            LOG.Info($"Idle()", this);

            anim.SetTrigger("Idle");
        }
        public void Hidden()
        {
            LOG.Info($"Hidden()", this);

            anim.SetTrigger("Hidden");
        }
        public void Land()
        {
            LOG.Info($"Land()", this);

            anim.SetTrigger("Land");
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private AudioClip appearCLIP = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}