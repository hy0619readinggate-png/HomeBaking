using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Game.C2_G02
{
    public class Magic : MonoBehaviour
    {
        // Methods
        public Coroutine PlayMagic()
        {
            LOG.Info($"PlayMagic()", this);

            return StartCoroutine(coMagic());
        }



        // Field : caching
        private MagicSpike[] spikes_ = null;
        private MagicSpike[] spikes => spikes_ ??= GetComponentsInChildren<MagicSpike>(true);

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private PlayableDirector magicTL = null;

        // Unity Messages
        private void Awake()
        {
            spikes.ForEach(s => s.gameObject.SetActive(false));
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coMagic()
        {
            magicTL.time = 0;
            magicTL.Play();
            yield return new WaitForSeconds((float)magicTL.duration);
            yield return new WaitForSeconds(0.5f);

            spikes.ForEach(s => s.gameObject.SetActive(false));
            yield return null;
        }
    }
}