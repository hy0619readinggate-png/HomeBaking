using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    [RequireComponent(typeof(Animator))]
    public class Booster : MonoBehaviour
    {
        // Methods
        public void Disappear()
        {
            LOG.Info($"Disappear()", this);

            Destroy(gameObject);
        }
        public void Consume()
        {
            LOG.Info($"Use()", this);

            consume();
        }

        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();

        // Functions
        private void consume()
        {
            AudioMGR.One.PlayEffect(getCLIP);

            boostFxPS.gameObject.SetActive(true);
            col.enabled = false;
            anim.SetTrigger("Hide");

            Destroy(gameObject, boostFxPS.main.duration);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private ParticleSystem boostFxPS = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip getCLIP = null;

        // Unity Messages
        private void Awake()
        {
            boostFxPS.gameObject.SetActive(false);
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                consume();

                player.BoostByItem();
            }
        }
    }
}