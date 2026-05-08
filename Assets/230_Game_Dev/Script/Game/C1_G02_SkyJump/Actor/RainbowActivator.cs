using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public class RainbowActivator : MonoBehaviour
    {
        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator anim = null;
        [Header("★ Config")]
        [SerializeField] private string triggerName = "Activate";

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null && anim.gameObject.activeSelf)
                anim.SetTrigger(triggerName);
        }
    }
}