using beyondi.Coroutine;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;


namespace DoDoEng.Activity.C1_A03
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Smoke : MonoBehaviour, ICompletable
    {
        // Methods
        public void Setup(SmokeParam smokeParam, DryGun gun)
        {
            LOG.Info($"Setup() ", this);

            param = smokeParam;
            life = param.startLife;
            this.gun = gun;
        }
        public void Clear()
        {
            LOG.Info($"Clear() ", this);

            life = 0;
            var emiation = vfx.emission;
            emiation.enabled = false;
        }



        // Fields : caching
        private ParticleSystem vfx_ = null;
        private ParticleSystem vfx => vfx_ ??= GetComponentInParent<ParticleSystem>();

        // Fields
        private SmokeParam param = null;
        private DryGun gun = null;
        //private float life = 100;
        private Coroutine blowCR = null;



        // Unity Inspectors
        [Header("¡Ú Config")]
        [SerializeField] private float life = 100;



        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }


        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {name} with {collision.name}", this);

            if (collision.gameObject != gun.BlowArea)
                return;

            if (blowCR != null)
                StopCoroutine(blowCR);
            blowCR = StartCoroutine(blow());
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerExit2D() | {name}", this);

            if (gun.BlowArea != collision.gameObject)
                return;

            if (blowCR != null)
                StopCoroutine(blowCR);
            blowCR = null;
        }

        private IEnumerator blow()
        {
            while (life > 0)
            {
                
                life -= Time.deltaTime * param.blowRatioPerSec;

                if (life <= 0)
                {
                    var emiation = vfx.emission;
                    emiation.enabled = false;
                }
                yield return null;
            }
            
            yield return null;
        }


        // Interface : ICompletable
        bool ICompletable.IsComplete => life <= 0;
    }
}