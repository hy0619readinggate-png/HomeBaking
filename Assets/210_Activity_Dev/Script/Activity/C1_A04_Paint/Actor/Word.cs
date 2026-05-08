using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A04
{
    [RequireComponent(typeof(Button))]
    public class Word : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup() | {pData}", this);

            phoneticCLIP = pData.PhoneticCLIP;
            wordCLIP = pData.WordCLIP;

            var alphabetIsFirst = pData.Alphabet != "x";
            if (alphabetIsFirst)
                wordAni = wordAnis[0];
            else wordAni = wordAnis[1];

            wordAnis.ForEach(w => w.gameObject.SetActive(w == wordAni));
            wordAni.Setup(pData.Word, pData.Alphabet, pData.TrimWord);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            btn.interactable = enable;
        }

        // Methods
        public void PlayWordAnimation()
        {
            LOG.Info($"PlayWordAnimation()", this);

            stopWordAnimation();
            crWordAni = StartCoroutine(PlayWordAnimationAndWait());
        }
        public IEnumerator PlayWordAnimationAndWait()
        {
            using (LOG.Coroutine("PlayWordAnimationAndWait()", this))
            {
                LOG.Info($"{phoneticCLIP.name} | {wordCLIP.name}", this);

                OnStateChanged?.Invoke(this, true);

                wordAni.PlayAnimation(WordAnimation.Phonetic);
                AudioMGR.One.PlayNarration(phoneticCLIP);
                yield return new WaitForSeconds(phoneticCLIP.length);

                wordAni.PlayAnimation(WordAnimation.Phonetic);
                AudioMGR.One.PlayNarration(phoneticCLIP);
                yield return new WaitForSeconds(phoneticCLIP.length);

                wordAni.PlayAnimation(WordAnimation.Word);
                AudioMGR.One.PlayNarration(wordCLIP);
                yield return new WaitForSeconds(wordCLIP.length);

                wordAni.PlayAnimation(WordAnimation.Completion);
                yield return null;

                OnStateChanged?.Invoke(this, false);
            }
        }
        public void ShowLastFrame()
        {
            LOG.Info($"ShowLastFrame()", this);

            stopWordAnimation();
            wordAni.PlayAnimation(WordAnimation.Completion);

            OnStateChanged?.Invoke(this, false);
        }

        // Events
        public event Action<Word, bool> OnStateChanged;



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();
        private WordAni[] wordAnis_ = null;
        private WordAni[] wordAnis => wordAnis_ ??= GetComponentsInChildren<WordAni>(true);

        // Fields
        private WordAni wordAni = null;
        private AudioClip phoneticCLIP = null;
        private AudioClip wordCLIP = null;
        private Coroutine crWordAni = null;

        // Functions
        private void stopWordAnimation()
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
            btn.onClick.AddListener(() => PlayWordAnimation());
        }
        private void Start()
        {

        }
    }
}