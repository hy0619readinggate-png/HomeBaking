using beyondi.Coroutine;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A04
{
    public class Bubble : MonoBehaviour, ICompletable
    {
        // Methods
        public void Setup(ProblemData pData)
        {
            bubbleGO.SetActive(true);
            vfxCollisionGO.SetActive(false);

            if (wordIMG != null)
            {
                wordIMG.sprite = pData.WordSPR;
                problem = pData;
            }

            isComplete = false;
        }
        public void Pop()
        {
            LOG.Info($"Pop()", this);

            bubbleGO.SetActive(false);
            wordIMG = null;
            isComplete = true;
        }



        // Fields
        private bool isComplete = false;
        private ProblemData problem = null;

        // Functions
        private bool isProblem => problem != null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject bubbleGO = null;
        [SerializeField] private Image wordIMG = null;
        [SerializeField] private GameObject vfxCollisionGO = null;
        [SerializeField] private AudioClip bubllePopCLIP = null;

        // Unity Messages
        private void Awake()
        {
            isComplete = false;
            vfxCollisionGO.SetActive(false);
        }
        private void Start()
        {
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            LOG.Info($"OnTriggerEnter2D() | {collision.gameObject.name}", this);

            if (isComplete)
                return;

            bubbleGO.SetActive(false);
            isComplete = true;

            var character = collision.GetComponent<Character>();
            if (character == null)
                return;

            vfxCollisionGO.SetActive(true);
            AudioMGR.One.PlayEffect(bubllePopCLIP);
            if (isProblem)
            {
                AudioMGR.One.PlayNarration(problem.WordCLIP);
                wordIMG.gameObject.SetActive(false);
            }
        }



        // Interface : ICompletable
        public bool IsComplete => isComplete;
    }
}