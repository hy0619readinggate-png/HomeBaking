using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C4_A05
{
    public class Wall : MonoBehaviour
    {
        // Properties
        public bool IsComplete => blanks.All(b => b.IsCorrect);
        public float[] AvailableBlanksPosX => blanks.Where(b => !b.IsCorrect).Select(b => b.PosX).ToArray();

        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup()", this);

            sentenceTXT.text = pData.Sentence;
            blanks.ForEach((i, b) => b.Setup(pData.Texts[i]));
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction()", this);

            cg.blocksRaycasts = enable;
        }
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            anim.SetTrigger("Correct");
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private Blank[] blanks_ = null;
        private Blank[] blanks => blanks_ ??= GetComponentsInChildren<Blank>(true);



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator anim = null;
        [SerializeField] private TextMeshProUGUI sentenceTXT = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }
    }
}