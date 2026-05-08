using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A09
{
    public class ConstellationSet : MonoBehaviour
    {
        // Properties
        public Constellation ActiveConstellation => activeConstellation;

        // Methods
        public void Next()
        {
            LOG.Info($"Next()", this);

            activeConstellation = getNextConstellation();
            constellations.ForEach(c => c.gameObject.SetActive(c == activeConstellation));
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private int[] constellationSeqs = null;
        private Constellation activeConstellation = null;

        // Functions
        private Constellation getNextConstellation()
        {
            if (constellationSeqs == null || constellationSeqs.Length == 0)
                constellationSeqs = UtilArray.Sequential(0, constellations.Length - 1);

            var seq = UtilArray.ExtractWithRemain(constellationSeqs, 1, out var remains);
            constellationSeqs = remains;

            return constellations[seq[0]];
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Constellation[] constellations = null;

        // Unity Messages
        private void Awake()
        {
            activeConstellation = getNextConstellation();
            constellations.ForEach(c => c.gameObject.SetActive(c == activeConstellation));
        }
        private void Start()
        {
        }
    }
}