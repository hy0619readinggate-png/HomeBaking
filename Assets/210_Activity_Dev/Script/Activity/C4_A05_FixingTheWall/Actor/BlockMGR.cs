using beyondi.Util;
using DoDoEng.Activity.Framework;
using DoDoEng.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C4_A05
{
    public class BlockMGR : MonoBehaviour
    {
        // Methods
        public void Init()
        {
            LOG.Info($"Init()", this);
            blockParentTR.RemoveAllChildren();
        }
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup()", this);

            clearBlocks();

            var exampleTexts = UtilArray.Shuffled(pData.Texts);
            foreach (var text in exampleTexts)
            {
                var block = blockPool.Get();
                block.gameObject.name = $"Block - {text}";
                block.Setup(text);
                block.onDropWrong += block_onDropWrong;

                blocks.Add(block);
                blocksAff.Add(block.AffordanceGO);
            }
            sentenceCLIP = pData.SentenceCLIP;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void FixBlocksLayout(bool enable)
        {
            LOG.Info($"FixBlocksLayout() | {enable}", this);

            horizontalLayout.enabled = enable;
        }
        public Coroutine StartShow()
        {
            LOG.Info($"StartShow()", this);

            blocks.ForEach(b => b.SetOriginPosition());

            crShow = StartCoroutine(coShow());
            return crShow;
        }
        public void FinishShow()
        {
            LOG.Info($"FinishShow()", this);

            this.StopCoroutineSafe(ref crShow);

            blocks.ForEach(b => b.Idle());
        }
        public void HideBlocks()
        {
            LOG.Info($"HideBlocks()", this);

            this.StopCoroutineSafe(ref crReturnWrongBlock);
            blocks.ForEach(b => b.Hide());
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private List<Block> blocks = new List<Block>();
        private Coroutine crShow = null;
        private Coroutine crReturnWrongBlock = null;
        private AudioClip sentenceCLIP = null;

        // Functions
        private void clearBlocks()
        {
            blocks.ForEach(b => b.onDropWrong -= block_onDropWrong);
            blocks.ForEach(b => { blockPool.Release(b); });
            blocks.Clear();
            blocksAff.Clear();
        }
        private bool isCompleteAll => blocks.ToArray().All(b => b.IsComplete);

        // Event Handlers
        private void block_onDropWrong(Block block)
        {
            LOG.Info($"block_onDropWrong()", this);

            ActivityProgress.One.Wrong();

            crReturnWrongBlock = StartCoroutine(coReturnWorngBlock(block));
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private BlockPool blockPool = null;
        [SerializeField] private Transform blockParentTR = null;
        [SerializeField] private Gino gino = null;
        [SerializeField] private AudioClip showCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;
        [SerializeField] private BlocksAff blocksAff = null;
        [SerializeField] private HorizontalLayoutGroup horizontalLayout = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            //blocks.ForEach(b => b.onDropWrong += block_onDropWrong);
        }
        private void OnDisable()
        {
            //blocks.ForEach(b => b.onDropWrong -= block_onDropWrong);
        }

        // Unity Coroutine
        IEnumerator coShow()
        {
            using (LOG.Coroutine($"coShow()", this))
            {
                yield return new WaitForSeconds(0.3f);

                AudioMGR.One.PlayEffect(showCLIP);
                blocks.ForEach(b => b.Show());

                yield return new WaitForSeconds(0.3f);
            }
        }
        IEnumerator coReturnWorngBlock(Block block)
        {
            using (LOG.Coroutine($"coReturnWorngBlock() | {block}", this))
            {
                cg.blocksRaycasts = false;
                gino.StopAppear();
                yield return null;

                gino.StartWrong();
                yield return null;

                AudioMGR.One.PlayEffect(wrongCLIP);
                block.WrongReturn();
                yield return new WaitForSeconds(1f);

                yield return AudioMGR.One.PlayNarrationAndWait(sentenceCLIP);
                yield return null;

                cg.blocksRaycasts = true;
                gino.FinishWrong();
                yield return null;

                if (!isCompleteAll)
                    gino.StartAppear();
            }
        }
    }
}