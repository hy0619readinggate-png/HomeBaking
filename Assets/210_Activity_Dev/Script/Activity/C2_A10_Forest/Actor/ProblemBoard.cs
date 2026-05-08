using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A10
{
    [RequireComponent(typeof(Button))]
    public class ProblemBoard : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup() | {pData}", this);

            img.sprite = pData.WordSPR;

            wordCLIP = pData.WordCLIP;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Image img_ = null;
        private Image img => img_ ??= GetComponent<Image>();
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private AudioClip wordCLIP = null;

        // Event Handlers
        private void btn_OnClick()
        {
            LOG.Info($"btn_OnClick()", this);

            AudioMGR.One.PlayNarration(wordCLIP);
        }



        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            btn.onClick.AddListener(btn_OnClick);
        }
        private void Start()
        {
        }
    }
}