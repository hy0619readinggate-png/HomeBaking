using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C3_A05
{
    public class ProblemBoard : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup() | {pData.Sentence}", this);

            this.pData = pData;

            problemIMG.sprite = pData.SentenceSPR;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction()", this);

            cg.blocksRaycasts = enable;
        }
        public void AbortPlaySound()
        {
            LOG.Info($"AbortPlaySound()", this);

            this.StopCoroutineSafe(ref crPlaySound);
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= problemGroupGO.AddComponent<CanvasGroup>();

        // Fields
        private ProblemData pData = null;
        private Coroutine crPlaySound = null;

        // Event Handlers
        private void btn_onClick()
        {
            LOG.Info($"btn_onClick()", this);

            crPlaySound = StartCoroutine(coPlaySound());
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject problemGroupGO = null;
        [SerializeField] private Image problemIMG = null;
        [SerializeField] private Button btn = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            btn.onClick.AddListener(btn_onClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coPlaySound()
        {
            using (LOG.Coroutine($"coPlaySound()", this))
            {
                cg.blocksRaycasts = false;
                yield return null;

                yield return AudioMGR.One.PlayNarrationAndWait(pData.SentenceCLIP);

                cg.blocksRaycasts = true;
                yield return null;
            }
        }
    }
}