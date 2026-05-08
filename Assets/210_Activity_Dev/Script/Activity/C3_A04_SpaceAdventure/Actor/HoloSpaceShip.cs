using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    public class HoloSpaceShip : MonoBehaviour
    {
        // Definitions
        private const int MAX_AnswerCharacter = 20;

        // Properties
        public int CurrentSeq
        {
            get => cSeq;
            set
            {
                cSeq = value;
                updateCurrentSeq();
            }
        }

        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup() | {pData}", this);

            initialize();
            setupPhrases(pData.Phrases);

            answerPS.transform.position = phraseAnswerTR.position;
        }
        public Coroutine ScatterCharacters()
        {
            LOG.Info($"ScatterCharacters()", this);

            return StartCoroutine(coScatterCharacters());
        }
        public void Correct(int seq)
        {
            LOG.Info($"Correct() | {seq}", this);

            charactersActive[seq - 1].Mode = CharacterMode.Fill;
        }
        public void Complete()
        {
            LOG.Info($"Complete()", this);

            AudioMGR.One.PlayEffect(completeCLIP);

            CurrentSeq = 0;
            charactersActive.ForEach(ch => ch.Mode = CharacterMode.Fill);

            answerPS.transform.position = phraseAnswerTR.position;
            answerPS.gameObject.SetActive(true);
        }



        // Fields
        private HoloSpaceShip_Character[] charactersAll = null;
        private HoloSpaceShip_Character[] charactersActive = null;
        private bool initialized = false;
        private int cSeq = 0;

        // Functions
        private void initialize()
        {
            if (!initialized)
            {
                phraseAnswerTR.RemoveAllChildren(true);
                for (var i = 0; i < MAX_AnswerCharacter; i++)
                    Instantiate(characterSetPB, phraseAnswerTR);
                charactersAll = phraseAnswerTR.GetComponentsInChildren<HoloSpaceShip_Character>(true);
                charactersAll.ForEach(c => c.gameObject.SetActive(false));

                initialized = true;
            }
        }
        private void setupPhrases(string[] phrases)
        {
            //phraseL.text = $"{phrases[0]}\u00A0";
            phraseL.text = phrases[0];
            phraseL.gameObject.SetActive(!string.IsNullOrWhiteSpace(phrases[0]));
            phraseR.text = phrases[2];
            phraseR.gameObject.SetActive(!string.IsNullOrWhiteSpace(phrases[2]));
            phraseWhiteSpaceGO.SetActive(phrases[0].LastOrDefault() == ' ');

            charactersAll.ForEach(c => c.gameObject.SetActive(false));
            for (var i = 0; i < phrases[1].Length && i < charactersAll.Length; i++)
            {
                charactersAll[i].Setup(phrases[1][i]);
                charactersAll[i].Mode = CharacterMode.Fill;
                charactersAll[i].gameObject.SetActive(true);
            }
            charactersActive = phraseAnswerTR.GetComponentsInChildren<HoloSpaceShip_Character>();

            phraseAnswerTR.RebuildLayoutImmediate();
        }
        private void updateCurrentSeq()
        {
            for (var i = 0; i < charactersActive.Length; i++)
            {
                var mode = CharacterMode.Empty;
                if (i < cSeq - 1) mode = CharacterMode.Fill;
                if (i == cSeq - 1) mode = CharacterMode.Active;

                charactersActive[i].Mode = mode;
            }

            phraseAnswerTR.RebuildLayoutImmediate();
        }

        // Event Handlers
        private void spaceShip_OnReady(int seq)
        {
            LOG.Info($"spaceShip_OnReady() | {seq}", this);

            CurrentSeq = seq;
        }
        private void spaceShip_OnCorrect(int seq)
        {
            LOG.Info($"spaceShip_OnCorrect() | {seq}", this);

            Correct(seq);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private HoloSpaceShip_Character characterSetPB = null;
        [SerializeField] private TextMeshProUGUI phraseL = null;
        [SerializeField] private TextMeshProUGUI phraseR = null;
        [SerializeField] private Transform phraseAnswerTR = null;
        [SerializeField] private GameObject phraseWhiteSpaceGO = null;
        [SerializeField] private ParticleSystem answerPS = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip scatterCLIP = null;
        [SerializeField] private AudioClip completeCLIP = null;

        // Unity Messages
        private void Awake()
        {
            answerPS.gameObject.SetActive(false);
            answerPS.SetStopAction(ParticleSystemStopAction.Disable);

            initialize();
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            EventBus.Subscribe<EventBus.SpaceShipReady>(spaceShip_OnReady);
            EventBus.Subscribe<EventBus.SpaceShipCorrect>(spaceShip_OnCorrect);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<EventBus.SpaceShipReady>(spaceShip_OnReady);
            EventBus.Unsubscribe<EventBus.SpaceShipCorrect>(spaceShip_OnCorrect);
        }

        // Unity Coroutine
        IEnumerator coScatterCharacters()
        {
            using (LOG.Coroutine($"coScatterCharacters()", this))
            {
                AudioMGR.One.PlayEffect(scatterCLIP);

                var scatterVariation = Random.Range(0, 2);
                var list = new List<HoloSpaceShip_Character>();
                foreach (var c in charactersActive)
                {
                    var scatterC = Instantiate(c, transform);
                    scatterC.transform.position = c.transform.position;
                    scatterC.Scatter(scatterVariation);
                    list.Add(scatterC);

                    c.Mode = CharacterMode.Empty;
                }
                phraseAnswerTR.RebuildLayoutImmediate();

                yield return new WaitUntil(() => list.All(c => c == null));
            }
        }
    }
}