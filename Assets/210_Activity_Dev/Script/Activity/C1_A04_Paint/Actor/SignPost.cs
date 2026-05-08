using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A04
{
    [RequireComponent(typeof(Button))]
    public class SignPost : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup() | {pData}", this);

            phoneticCLIP = pData.PhoneticCLIP;
            wordCLIP = pData.WordCLIP;

            var alphabetIsFirst = pData.Alphabet != "x";
            if (alphabetIsFirst)
                word = words[0];
            else word = words[1];

            words.ForEach(w => w.gameObject.SetActive(w == word));
            word.Setup(pData.Text, pData.TrimWord);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            btn.interactable = enable;
        }

        // Methods
        public void PlayWordSound()
        {
            LOG.Info($"PlayWordSound()", this);

            stopWordSound();
            crWordAni = StartCoroutine(PlayWordSoundAndWait());
        }
        public IEnumerator PlayWordSoundAndWait()
        {
            using (LOG.Coroutine("PlayWordSoundAndWait()", this))
            {
                LOG.Info($"{phoneticCLIP.name} | {wordCLIP.name}", this);

                OnStateChanged?.Invoke(true);

                AudioMGR.One.PlayNarration(phoneticCLIP);
                yield return new WaitForSeconds(phoneticCLIP.length);

                AudioMGR.One.PlayNarration(phoneticCLIP);
                yield return new WaitForSeconds(phoneticCLIP.length);

                AudioMGR.One.PlayNarration(wordCLIP);
                yield return new WaitForSeconds(wordCLIP.length);

                OnStateChanged?.Invoke(false);
            }
        }
        public void StopWordSound()
        {
            LOG.Info($"AbortAnimation()", this);

            stopWordSound();

            btn.interactable = true;

            OnStateChanged?.Invoke(false);
        }

        // Events
        public event Action<bool> OnStateChanged;



        // Fields : caching
        private Button btn => GetComponent<Button>();
        private SignPostWord[] words_ = null;
        private SignPostWord[] words => words_ ??= GetComponentsInChildren<SignPostWord>(true);

        // Fields
        private SignPostWord word = null;
        private Coroutine crWordAni = null;
        private AudioClip phoneticCLIP = null;
        private AudioClip wordCLIP = null;

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
            btn.onClick.AddListener(() => PlayWordSound());
        }
        private void Start()
        {

        }
    }
}