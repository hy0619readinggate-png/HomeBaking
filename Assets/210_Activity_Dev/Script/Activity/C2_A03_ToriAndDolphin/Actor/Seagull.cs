using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A03
{
    public class Seagull : MonoBehaviour, IPointerDownHandler
    {
        // Methods
        public void Setup(ProblemData problem)
        {
            LOG.Info($"Setup() | {problem}", this);

            wordClip = problem.WordCLIP;
            problemIMG.sprite = problem.WordSPR;

            problemTXT[0].text = problem.BlankWord.Substring(0, problem.BlankWord.IndexOf("_"));
            problemTXT[1].text = problem.Phonics;
            problemTXT[2].text = problem.BlankWord.Substring(problem.BlankWord.LastIndexOf("_") + 1);

            ansewrTXT.text = problem.Word;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private AudioClip wordClip = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image problemIMG = null;
        [SerializeField] private TextMeshProUGUI[] problemTXT = null;
        [SerializeField] private TextMeshProUGUI ansewrTXT = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }



        // Interface : IPointerDownHandler
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            LOG.Info($"OnPointerDown()", this);

            AudioMGR.One.PlayNarration(wordClip);
        }
    }
}