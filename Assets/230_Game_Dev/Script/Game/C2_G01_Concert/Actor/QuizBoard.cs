using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.C2_G01
{
    [RequireComponent(typeof(Button))]
    public class QuizBoard : MonoBehaviour
    {
        // Methods
        public void ShowProblem(string[] phrases, AudioClip wordCLIP, bool isBlank = false)
        {
            LOG.Info($"{nameof(ShowProblem)}() | {string.Join(",", phrases)} | {isBlank}", this);

            AudioMGR.One.PlayEffect(showCLIP);

            quizPhrases = phrases;
            this.isBlank = isBlank;
            this.wordCLIP = wordCLIP;

            showProblem();
        }
        public void HideProblem()
        {
            LOG.Info($"{nameof(HideProblem)}()", this);

            t1GO.SetActive(false);
            t2GO.SetActive(false);
        }
        public void ShowAnswer(bool showProblemAgain)
        {
            LOG.Info($"ShowAnswer(), {showProblemAgain}", this);

            t1Anim.SetTrigger("Idle");

            t2BlankImageGo.SetActive(false);
            t2Layout.spacing = 0;

            this.StopCoroutineSafe(ref crShowProblemAgain);
            if (showProblemAgain)
                crShowProblemAgain = StartCoroutine(coShowProblemAgain(quizPhrases));
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Function(this, $"{enable}");

            cg.blocksRaycasts = enable;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private float t1LayoutSpacingOrigin = 0;
        private float t2LayoutSpacingOrigin = 0;
        private string[] quizPhrases = null;
        private bool isBlank = false;
        private AudioClip wordCLIP = null;
        private Coroutine crPlaySound = null;
        private Coroutine crShowProblemAgain = null;

        // Functions
        private void showProblem()
        {

            t1GO.SetActive(!isBlank);
            t2GO.SetActive(isBlank);
            if (!isBlank)
            {
                t1Layout.spacing = t1LayoutSpacingOrigin;
                for (var i = 0; i < quizPhrases.Length; i++)
                    t1ProblemTXT[i].text = quizPhrases[i];

                t1Anim.SetTrigger("Show");
            }
            else
            {
                t2BlankImageGo.SetActive(true);
                t2Layout.spacing = t2LayoutSpacingOrigin;

                for (var i = 0; i < quizPhrases.Length; i++)
                    t2ProblemTXT[i].text = quizPhrases[i];
            }
        }

        // Event Handlers
        private void btn_OnClick()
        {
            LOG.Function(this);

            crPlaySound = StartCoroutine(coPlaySound());
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject t1GO = null;
        [SerializeField] private GameObject t2GO = null;
        [SerializeField] private TextMeshProUGUI[] t1ProblemTXT = null;
        [SerializeField] private TextMeshProUGUI[] t2ProblemTXT = null;
        [SerializeField] private Animator t1Anim = null;
        [SerializeField] private GameObject t2BlankImageGo = null;
        [SerializeField] private HorizontalLayoutGroup t1Layout = null;
        [SerializeField] private HorizontalLayoutGroup t2Layout = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip showCLIP = null;
        [Header("★ Config")]
        [SerializeField] private Color blankColor = Color.gray;
        [SerializeField] private float showProblemAgainDelay = 1f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            t1LayoutSpacingOrigin = t1Layout.spacing;
            t2LayoutSpacingOrigin = t2Layout.spacing;

            t1GO.SetActive(false);
            t2GO.SetActive(false);

            btn.onClick.AddListener(btn_OnClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coPlaySound()
        {
            using (LOG.Coroutine($"coPlaySound()", this))
            {
                btn.enabled = false;
                yield return AudioMGR.One.PlayNarrationAndWait(wordCLIP);

                btn.enabled = true;
            }
        }
        IEnumerator coShowProblemAgain(string[] phrases)
        {
            using (LOG.Coroutine($"coShowProblemAgain()", this))
            {
                yield return new WaitForSeconds(showProblemAgainDelay);

                showProblem();
            }

        }
    }
}