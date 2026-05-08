using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C3_A06
{
    public class PuzzleMGR : MonoBehaviour
    {
        // Properties
        public PuzzlePiece[] AvaliablePieces => pieces.Where(p => p.IsAvailable).ToArray();
        public PuzzleSlot[] Slots => slots;

        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup()", this);

            pieces.ForEach((i, p) => p.Setup(pData.ExampleIDs[i], pData.SentenceSPR));
            slots.ForEach((i, t) => t.Setup(pData.SentenceSPR));
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public Coroutine StartShow()
        {
            LOG.Info($"StartShow()", this);

            crShow = StartCoroutine(coShow());
            return crShow;
        }
        public void FinishShow()
        {
            LOG.Info($"FinishShow()", this);

            this.StopCoroutineSafe(ref crShow);

            pieces.ForEach(p => p.Shown());
        }
        public Coroutine StartPuzzle()
        {
            LOG.Info($"StartPuzzle()", this);

            crPuzzle = StartCoroutine(coPuzzle());
            return crPuzzle;
        }
        public void FinishPuzzle()
        {
            LOG.Info($"FinishPuzzle()", this);

            this.StopCoroutineSafe(ref crPuzzle);

            pieces.ForEach(p => p.Hidden());
            slots.ForEach(s => s.Completed());
            cg.blocksRaycasts = false;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();
        private PuzzlePiece[] pieces_ = null;
        private PuzzlePiece[] pieces => pieces_ ??= GetComponentsInChildren<PuzzlePiece>(true);
        private PuzzleSlot[] slots_ = null;
        private PuzzleSlot[] slots => slots_ ??= GetComponentsInChildren<PuzzleSlot>(true);

        // Fields
        private Coroutine crShow;
        private Coroutine crPuzzle = null;



        // Unity Inspectors
        [Header("★ Audios")]
        [SerializeField] private AudioClip completeCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float showDelay = 0.3f;

        // Unity Messages
        private void Awake()
        {
            slots.AutoFillID();
            cg.blocksRaycasts = false;
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coShow()
        {
            using (LOG.Coroutine($"coShow()", this))
            {
                var rand = UtilArray.Random(0, pieces.Length - 1);
                foreach (var (p, i) in pieces.Select((p, i) => (p, i)))
                {
                    pieces[rand[i]].Show();
                    yield return new WaitForSeconds(showDelay);
                }
            }
        }
        IEnumerator coPuzzle()
        {
            using (LOG.Coroutine($"coPuzzle()", this))
            {
                cg.blocksRaycasts = true;
                yield return null;

                yield return new WaitUntil(() => slots.Count(t => t.IsComplete) == slots.Length);
                yield return new WaitForSeconds(0.7f);    // 전체 정답 효과

                AudioMGR.One.PlayEffect(completeCLIP);
                yield return null;

                slots.ForEach(s => s.Complete());
                yield return new WaitForSeconds(0.5f);  // 전환 효과 딜레이

                cg.blocksRaycasts = false;
                yield return null;
            }
        }
    }
}