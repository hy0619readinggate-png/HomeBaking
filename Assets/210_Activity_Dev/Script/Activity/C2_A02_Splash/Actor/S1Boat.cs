using beyondi.Util;
using DoDoEng.Common;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Activity.C2_A02
{
    [RequireComponent(typeof(Animator))]
    public class S1Boat : MonoBehaviour
    {
        // Properties
        public S1ExampleText SubmitExampleText => submitExampleText;
        public TextArea SubmitTextArea => submitTextArea;
        public bool IsCorrect => SubmitExampleText.Text == SubmitTextArea.Text;
        public bool IsComplete => activeTextArea.All(t => t.IsComplete);
        public int[] CharacterIDs => activeSeat.CharacterIDs;
        public TextArea[] ActiveTextAreas => activeTextArea
            .Where(t => !t.IsComplete)
            .ToArray();

        // Methods
        public void Setup(ProblemData dData)
        {
            LOG.Info($"Setup()", this);

            this.pData = dData;

            seat2.Setup();
            seat3.Setup();
            seat2.gameObject.SetActive(subjectCount == 2);
            seat3.gameObject.SetActive(subjectCount == 3);

            textArea2.ForEach(t => t.gameObject.SetActive(subjectCount == 2));
            textArea3.ForEach(t => t.gameObject.SetActive(subjectCount == 3));

            activeTextArea.ForEach((i, t) => t.Setup(pData.Subjects[i]));
            answer.Setup(dData.Word);
        }
        public void Correct(int characterID)
        {
            LOG.Info($"Correct()", this);

            movingCharacter.Setup(characterID);

            SubmitExampleText.ReturnAndHide();
            SubmitTextArea.Correct();
            activeSeat.Sit(SubmitTextArea.ID, characterID);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);
        }
        public Coroutine StartWaitSubmit()
        {
            LOG.Info($"StartWaitSubmit()", this);

            crStartWaitSubmit = StartCoroutine(coStartWaitSubmit());
            return crStartWaitSubmit;
        }
        public void FinishWaitSubmit()
        {
            LOG.Info($"FinishWaitSubmit()", this);

            this.StopCoroutineSafe(ref crStartWaitSubmit);
        }
        public Coroutine StartRide(int characterID)
        {
            LOG.Info($"StartRide() | {characterID}", this);

            crStartRide = StartCoroutine(coStartRide(characterID));
            return crStartRide;
        }
        public void FinishRide()
        {
            LOG.Info($"FinishRide()", this);

            this.StopCoroutineSafe(ref crStartRide);
            var tlPlayer = getJumpTLPlayer();
            tlPlayer.FinishPlaying();

            activeSeat.IdleOnShip();
        }



        // Fields : caching
        private Animator anim_ = null;
        private Animator anim => anim_ ??= GetComponent<Animator>();

        // Fields
        private ProblemData pData = null;
        private bool isSubmit = false;
        private S1ExampleText submitExampleText = null;
        private TextArea submitTextArea = null;
        private Coroutine crStartWaitSubmit = null;
        private Coroutine crStartRide = null;

        // Functions
        private S1Seat activeSeat => subjectCount == 2 ? seat2 : seat3;
        private TextArea[] activeTextArea => subjectCount == 2 ? textArea2 : textArea3;
        private int subjectCount => pData.Subjects.Length;
        private S1JumpTLPlayer getJumpTLPlayer()
        {
            if (subjectCount == 2)
                return s1Jump2TLPlayerCollection[submitTextArea.ID - 1].RandomPlayer;
            else return s1Jump3TLPlayerCollection[submitTextArea.ID - 1].RandomPlayer;
        }

        // Event Handlers
        private void textArea_OnDrop(S1ExampleText exampleText, TextArea textArea)
        {
            LOG.Info($"textArea_OnDrop() | {exampleText.Text} | {textArea.Text}", this);

            submitExampleText = exampleText;
            submitTextArea = textArea;

            isSubmit = true;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Character movingCharacter = null;
        [SerializeField] private S1Seat seat2 = null;
        [SerializeField] private S1Seat seat3 = null;
        [SerializeField] private TextArea[] textArea2 = null;
        [SerializeField] private TextArea[] textArea3 = null;
        [SerializeField] private S1Answer answer = null;
        [SerializeField] private S1JumpTLPlayerConfig[] s1Jump2TLPlayerCollection = null;
        [SerializeField] private S1JumpTLPlayerConfig[] s1Jump3TLPlayerCollection = null;
        [Header("★ Config")]
        [SerializeField] private float answerFadeDuration = 1f;

        // Unity Messages
        private void Awake()
        {
            textArea2.AutoFillID();
            textArea3.AutoFillID();
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            textArea2.ForEach(t => t.OnDrop += textArea_OnDrop);
            textArea3.ForEach(t => t.OnDrop += textArea_OnDrop);
        }
        private void OnDisable()
        {
            textArea2.ForEach(t => t.OnDrop -= textArea_OnDrop);
            textArea3.ForEach(t => t.OnDrop -= textArea_OnDrop);
        }

        // Unity Coroutine
        IEnumerator coStartWaitSubmit()
        {
            using (LOG.Coroutine($"coStartWaitSubmit()", this))
            {
                isSubmit = false;

                yield return new WaitUntil(() => isSubmit == true);
            }
        }
        IEnumerator coStartRide(int characterID)
        {
            using (LOG.Coroutine($"coStartRide() | {characterID}", this))
            {
                var tlPlayer = getJumpTLPlayer();
                yield return tlPlayer.PlayAndWait(characterID);

                activeSeat.IdleOnShip();
                yield return null;

                if (IsComplete)
                {
                    answer.Show(answerFadeDuration);
                    activeTextArea.ForEach(ta => ta.Hide(answerFadeDuration));

                    yield return AudioMGR.One.PlayNarrationAndWait(pData.WordCLIP);
                }
            }
        }
    }

    [Serializable]
    public class S1JumpTLPlayerConfig
    {
        // Methods
        public S1JumpTLPlayer RandomPlayer => UtilArray.ExtractOne(s1JumpTLPlayers);

        [Header("★ Bindings")]
        public S1JumpTLPlayer[] s1JumpTLPlayers;
    }
}