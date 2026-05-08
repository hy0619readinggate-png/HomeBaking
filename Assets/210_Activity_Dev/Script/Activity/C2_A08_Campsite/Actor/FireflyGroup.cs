using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C2_A08
{
    public class FireflyGroup : MonoBehaviour
    {
        // Properties
        public Transform[] ActiveFireFlyTRs => fireflies
                                            .Where(f => f.IsActivated)
                                            .Select(f => f.transform)
                                            .ToArray();

        // Methods
        public void Setup(int count)
        {
            LOG.Info($"Setup()", this);

            activateCount = count;
            activateIndices = UtilArray.Random(0, fireflies.Length - 1);

            activateIndices.ForEach((i, a) => fireflies[i].Setup(a < activateCount));

            cg.blocksRaycasts = false;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            fireflies.ForEach(f => f.Hide());
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private int activateCount;
        private int[] activateIndices = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Firefly[] fireflies = null;

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