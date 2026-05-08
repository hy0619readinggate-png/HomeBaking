using beyondi.Coroutine;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class T2Example : MonoBehaviour, ISubmitable
    {
        // Properties
        public bool IsAnswer { get; private set; }
        public bool IsSubmit { get; private set; }

        // Methods
        public void Setup(string example, bool isAnswer)
        {
            LOG.Info($"Setup() | {example} {isAnswer}", this);

            IsAnswer = isAnswer;
            IsSubmit = false;

            if (questionTXT != null)
                questionTXT.text = example.Replace("\\n", "\n");
            correctGO.SetActive(false);
            wrongGO.SetActive(false);
            answerGO.SetActive(false);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            examBTN.interactable = enable;
        }
        public void Feedback()
        {
            LOG.Info($"Feedback()", this);

            if (IsAnswer)
            {
                AudioMGR.One.PlayEffect(correctSFX);
                correctGO.SetActive(true);
            }
            else
            {
                AudioMGR.One.PlayEffect(wrongSFX);
                wrongGO.SetActive(true);
            }
        }
        public void ShowAnswer()
        {
            LOG.Info($"ShowAnswer()", this);

            AudioMGR.One.PlayEffect(answerSFX);
            answerGO.SetActive(true);
        }



        // Event Handlers
        private void examBTN_OnClick()
        {
            LOG.Info($"examBTN_OnClick()", this);

            IsSubmit = true;
            AudioMGR.One.PlayEffect(submitSFX);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button examBTN = null;
        [SerializeField] private TextMeshProUGUI questionTXT = null;
        [SerializeField] private GameObject correctGO = null;
        [SerializeField] private GameObject wrongGO = null;
        [SerializeField] private GameObject answerGO = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip correctSFX = null;
        [SerializeField] private AudioClip wrongSFX = null;
        [SerializeField] private AudioClip submitSFX = null;
        [SerializeField] private AudioClip answerSFX = null;

        // Unity Messages
        private void Awake()
        {
            correctGO.SetActive(false);
            wrongGO.SetActive(false);
            answerGO.SetActive(false);

            examBTN.onClick.AddListener(examBTN_OnClick);
        }
        private void Start()
        {
        }



        // ISubmitable
        bool ISubmitable.IsSubmit => IsSubmit;
    }
}