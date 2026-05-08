using DG.Tweening;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A11
{
    [RequireComponent(typeof(Animator))]
    public class Stepping : MonoBehaviour
    {
        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();



        [Header("¡Ú Config")]
        [SerializeField] private float landDelay = 0.15f;



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            DOVirtual.DelayedCall(landDelay, () => anim.SetTrigger("Land"));
        }
    }
}