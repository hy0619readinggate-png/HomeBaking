using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.C3_A09
{
    public class Intro : MonoBehaviour
    {
        // Methods
        public void Setup(IntroData[] introDatas)
        {
            LOG.Function(this);

            this.introDatas = introDatas;

            this.gameObject.SetActive(isIntroDataExist);

            if (isIntroDataExist)
            {
                cardIn1TL.time = 0;
                cardIn1TL.Evaluate();
                cardIn1TL.Stop();

                introDatas.ForEach(d => LOG.Info($"{d}", this));
                cards.ForEach((i, c) => c.Setup(introDatas[i]));

                word1Sentence1.text = introDatas[0].Sentence1;
                word1Sentence2.text = introDatas[0].Sentence2;

                word2Sentence1.text = introDatas[1].Sentence1;
                word2Sentence2.text = introDatas[1].Sentence2;
            }
        }
        public Coroutine StartPlay()
        {
            LOG.Function(this);

            if (isIntroDataExist)
            {
                crPlay = StartCoroutine(coPlay());
                return crPlay;
            }
            else return null;
        }
        public void FinishPlay()
        {
            LOG.Function(this);

            this.StopCoroutineSafe(ref crPlay);

            cardIn1TL.Stop();
            cardIn2TL.Stop();
            card1_1TL.Stop();
            card1_2TL.Stop();
            card2_1TL.Stop();
            card2_2TL.Stop();

            outroTL.time = outroTL.duration;
            outroTL.Evaluate();
            outroTL.Stop();

            this.gameObject.SetActive(false);
        }



        // Fields : caching
        private IntroCard[] _cards = null;
        private IntroCard[] cards => _cards ??= GetComponentsInChildren<IntroCard>();

        // Fields
        private IntroData[] introDatas;
        private Coroutine crPlay = null;

        // Functions
        private bool isIntroDataExist => introDatas.Length > 0;



        // Unity Inspectors
        [SerializeField] private TextMeshProUGUI word1Sentence1 = null;
        [SerializeField] private TextMeshProUGUI word1Sentence2 = null;
        [SerializeField] private TextMeshProUGUI word2Sentence1 = null;
        [SerializeField] private TextMeshProUGUI word2Sentence2 = null;
        [Header("¡Ú TimeLine")]
        [SerializeField] private PlayableDirector cardIn1TL = null;
        [SerializeField] private PlayableDirector cardIn2TL = null;
        [SerializeField] private PlayableDirector card1_1TL = null;
        [SerializeField] private PlayableDirector card1_2TL = null;
        [SerializeField] private PlayableDirector card2_1TL = null;
        [SerializeField] private PlayableDirector card2_2TL = null;
        [SerializeField] private PlayableDirector outroTL = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coPlay()
        {
            using (LOG.Coroutine($"coPlay()", this))
            {
                // Card1
                cardIn1TL.time = 0;
                cardIn1TL.Play();
                yield return new WaitForSeconds((float)cardIn1TL.duration);

                // Word1 Sound
                yield return AudioMGR.One.PlayNarrationAndWait(introDatas[0].WordCLIP);
                yield return new WaitForSeconds(0.5f);

                // Card2
                cardIn2TL.time = 0;
                cardIn2TL.Play();
                yield return new WaitForSeconds((float)cardIn2TL.duration);

                // Word2 Sound
                yield return AudioMGR.One.PlayNarrationAndWait(introDatas[1].WordCLIP);
                yield return new WaitForSeconds(0.5f);


                // Sentence1-1
                card1_1TL.time = 0;
                card1_1TL.Play();
                yield return new WaitForSeconds((float)card1_1TL.duration);

                // (Word1) Sentence1 Sound
                yield return AudioMGR.One.PlayNarrationAndWait(introDatas[0].SentenceCLIP1);
                yield return new WaitForSeconds(0.5f);

                // Sentence1-2
                card1_2TL.time = 0;
                card1_2TL.Play();
                yield return new WaitForSeconds((float)card1_2TL.duration);

                // (Word1) Sentence2 Sound
                yield return AudioMGR.One.PlayNarrationAndWait(introDatas[0].SentenceCLIP2);
                yield return new WaitForSeconds(0.5f);

                // Sentence2-1
                card2_1TL.time = 0;
                card2_1TL.Play();
                yield return new WaitForSeconds((float)card2_1TL.duration);

                // (Word1) Sentence1 Sound
                yield return AudioMGR.One.PlayNarrationAndWait(introDatas[1].SentenceCLIP1);
                yield return new WaitForSeconds(0.5f);

                // Sentence2-2
                card2_2TL.time = 0;
                card2_2TL.Play();
                yield return new WaitForSeconds((float)card2_2TL.duration);

                // (Word1) Sentence2 Sound
                yield return AudioMGR.One.PlayNarrationAndWait(introDatas[1].SentenceCLIP2);
                yield return new WaitForSeconds(0.5f);

                outroTL.time = 0;
                outroTL.Play();
                yield return new WaitForSeconds((float)outroTL.duration);

            }
        }
    }
}