using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A03
{
    public class CorrectVFX : MonoBehaviour
    {
        // Methods
        public void Correct(int id)
        {
            LOG.Info($"Correct() | {id}", this);

            vfxs[id - 1].gameObject.SetActive(true);
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            vfxs.ForEach(v => v.gameObject.SetActive(false));
        }



        // Fields : caching
        private ParticleSystem[] vfxs_ = null;
        private ParticleSystem[] vfxs => vfxs_ ??= GetComponentsInChildren<ParticleSystem>(true);



        // Unity Messages
        private void Awake()
        {
            vfxs.ForEach(v => v.gameObject.SetActive(false));
        }
        private void Start()
        {
        }
    }
}