using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A03
{
    [RequireComponent(typeof(Animator))]
    public class Speaker : MonoBehaviour
    {
        // Methods
        public void Setup(AudioClip clip)
        {
            LOG.Info($"Setup()", this);

            phoneticCLIP = clip;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            btn.interactable = enable;
        }

        // Methods
        public Coroutine PlayWordSoundAndWait()
        {
            LOG.Info($"PlayWordSoundAndWait()", this);

            stopWordSound();
            crWordAni = StartCoroutine(coPlayWordSound());
            return crWordAni;
        }
        public void StopWordSound()
        {
            LOG.Info($"AbortAnimation()", this);
            stopWordSound();

            btn.interactable = true;

            anim.SetBool("Play", false);
        }

        // Events
        public event Action<bool> OnStateChanged;



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponentInChildren<Button>();

        // Fields
        private Coroutine crWordAni = null;
        private AudioClip phoneticCLIP = null;

        // Functions
        private void stopWordSound()
        {
            if (crWordAni != null)
            {
                StopCoroutine(crWordAni);
                AudioMGR.One.StopNarration();
            }
        }



        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(() => PlayWordSoundAndWait());
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coPlayWordSound()
        {
            using (LOG.Coroutine("coPlayWordSound()", this))
            {
                LOG.Info($"{phoneticCLIP.name}", this);

                OnStateChanged?.Invoke(true);
                anim.SetBool("Play", true);

                AudioMGR.One.PlayNarration(phoneticCLIP);
                yield return new WaitForSeconds(phoneticCLIP.length);

                AudioMGR.One.PlayNarration(phoneticCLIP);
                yield return new WaitForSeconds(phoneticCLIP.length);

                anim.SetBool("Play", false);
                OnStateChanged?.Invoke(false);
            }
        }
    }
}