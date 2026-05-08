using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A05
{
    public class ProblemSign : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup() | {pData}", this);

            wordCLIP = pData.WordCLIP;

            var blankWord = pData.BlankWord;

            problemIMG.sprite = pData.ImageWord;

            problemTextGO.SetActive(true);
            problemTXT1.text = blankWord.Substring(0, blankWord.IndexOf("_"));
            problemTXT2.text = pData.Text;
            problemTXT3.text = blankWord.Substring(blankWord.LastIndexOf("_") + 1);

            answerTextGO.SetActive(false);
            answerTXT.text = pData.Word;

            vfxAnswerGO.SetActive(false);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;

            this.StopCoroutineSafe(ref crPlayWord);
        }
        public void ShowAnswer()
        {
            LOG.Info($"ShowAnswer()", this);

            problemTextGO.SetActive(false);
            answerTextGO.SetActive(true);

            vfxAnswerGO.transform.position = problemTXT2.transform.position;
            vfxAnswerGO.SetActive(true);
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private AudioClip wordCLIP = null;
        private Coroutine crPlayWord = null;

        // Event Handlers
        private void onClick()
        {
            crPlayWord = StartCoroutine(coPlayWord());
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button btn = null;
        [Header("★ Bindings - Problem")]
        [SerializeField] private Image problemIMG = null;
        [SerializeField] private GameObject problemTextGO = null;
        [SerializeField] private TextMeshProUGUI problemTXT1 = null;
        [SerializeField] private TextMeshProUGUI problemTXT2 = null;
        [SerializeField] private TextMeshProUGUI problemTXT3 = null;
        [Header("★ Bindings - Answer")]
        [SerializeField] private GameObject answerTextGO = null;
        [SerializeField] private TextMeshProUGUI answerTXT = null;
        [SerializeField] private GameObject vfxAnswerGO = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            vfxAnswerGO.SetActive(false);

            btn.onClick.AddListener(onClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coPlayWord()
        {
            using (LOG.Coroutine($"coPlayWord()", this))
            {
                cg.blocksRaycasts = false;
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(wordCLIP);

                cg.blocksRaycasts = true;
                yield return null;
            }
        }
    }
}