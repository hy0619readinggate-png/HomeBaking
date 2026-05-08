using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A08
{
    public class CollectFirefly : MonoBehaviour
    {
        // Methods
        public void Setup()
        {
            LOG.Info($"Setup()", this);

            cg.alpha = 0;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            fireflyAni.PlayAnimationLoop(FireflyAnimation.Idle);
            cg.alpha = 1;
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private FireflyAni fireflyAni = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}