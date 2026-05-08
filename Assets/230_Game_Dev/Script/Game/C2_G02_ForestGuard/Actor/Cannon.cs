using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C2_G02
{
    public class Cannon : MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        // Properties 
        public bool IsAlive { get; private set; } = true;

        // Methods
        public void Resurrect()
        {
            LOG.Info($"Resurrect()", this);

            IsAlive = true;
            StartCoroutine(coResurrect());
        }
        public void Kill()
        {
            LOG.Info($"Kill()", this);

            IsAlive = false;
            StartCoroutine(coDie());
        }

        // Methods
        public void DEV_Fire(BulletData bulletData)
        {
            LOG.Info($"DEV_Fire() | {bulletData}", this);

            StartCoroutine(coFireBullet(bulletData));
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Collider2D col_ = null;
        private Collider2D col => col_ ??= GetComponent<Collider2D>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CannonAni ani = null;
        [SerializeField] private PeachBullet peachBulletPB = null;
        [SerializeField] private Transform bulletParentTR = null;
        [SerializeField] private Transform bulletTakeTR = null;
        [SerializeField] private Transform bulletSpawnTR = null;
        [SerializeField] private Transform bulletLimitTR = null;
        [SerializeField] private GameObject fireFxGO = null;
        [SerializeField] private GameObject respawnFxGO = null;
        [SerializeField] private GameObject overGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip chargeCLIP = null;
        [SerializeField] private AudioClip crashCLIP = null;
        [Header("★ Config")]
        [SerializeField] private Vector2 bulletVelocity = new Vector2(10, 0.3f);
        [SerializeField] private float fireFxDelay = 0f;
        [SerializeField] private float fireBulletDelay = 0f;
        [SerializeField] private float fireBulletPostDelay = 0.5f;
        [Header("★ Dev")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private bool fireAtClick = true;

        // Unity Messages
        private void Awake()
        {
            overGO.SetActive(false);
            fireFxGO.SetActive(false);
            respawnFxGO.SetActive(false);
        }
        private void Start()
        {

        }
        private void OnDrawGizmos()
        {
            if (drawGizmos)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(bulletSpawnTR.position, bulletLimitTR.position);

                Gizmos.color = Color.red;
                Gizmos.DrawRay(bulletSpawnTR.position, bulletVelocity);
            }
        }

        // Unity Coroutine
        IEnumerator coFireBullet(BulletData bulletData)
        {
            using (LOG.Coroutine($"coFireBullet() | {bulletData}", this))
            {
                cg.blocksRaycasts = false;
                yield return null;

                AudioMGR.One.PlayEffect(chargeCLIP);
                yield return ani.PlayAnimationAndWait(CannonAnimation.Drag);

                ani.PlayAnimation(CannonAnimation.Attack);
                yield return new WaitForSeconds(fireFxDelay);

                fireFxGO.SetActive(true);
                yield return new WaitForSeconds(fireBulletDelay);

                // Fire Bullet
                var peachBullet = Instantiate(
                    peachBulletPB,
                    bulletSpawnTR.position,
                    Quaternion.identity,
                    bulletParentTR);

                peachBullet.Setup(
                    bulletData.SoundPhonetic,
                    bulletData.WordCLIP,
                    bulletVelocity,
                    bulletLimitTR.position.x);
                yield return new WaitForSeconds(fireBulletPostDelay);

                cg.blocksRaycasts = true;
                yield return null;
            }
        }
        IEnumerator coDie()
        {
            using (LOG.Coroutine($"coDie()", this))
            {
                cg.blocksRaycasts = false;
                col.enabled = false;
                yield return null;

                AudioMGR.One.PlayEffect(crashCLIP);
                yield return ani.PlayAnimationAndWait(CannonAnimation.Death, false);
                yield return null;
            }
        }
        IEnumerator coResurrect()
        {
            using (LOG.Coroutine($"coResurrect()", this))
            {
                yield return ani.PlayAnimationAndWait(CannonAnimation.Despawn, false);
                yield return ani.PlayAnimationAndWait(CannonAnimation.Spawn, false);
                yield return null;

                cg.blocksRaycasts = true;
                col.enabled = true;
                yield return null;
            }
        }



        // Interface : IDropHandler, IPointerEnterHandler, IPointerExitHandler
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            LOG.Info($"OnDrop() | {eventData.pointerDrag.name}", this);

            var peach = eventData.pointerDrag.GetComponent<PeachItem>();
            if (peach != null)
            {
                eventData.Use();

                overGO.SetActive(false);
                peach.Take(bulletTakeTR.position);
                StartCoroutine(coFireBullet(peach.BulletData));
            }
        }
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (PeachItem.CurrentDrag != null || fireAtClick)
                overGO.SetActive(true);
        }
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            //if (PeachItem.CurrentDrag != null || fireAtClick)
                overGO.SetActive(false);
        }
    }
}