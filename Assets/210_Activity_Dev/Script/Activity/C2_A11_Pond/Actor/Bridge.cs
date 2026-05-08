using DG.Tweening;
using DoDoEng.Activity.C2_A11;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng
{
    public class Bridge : MonoBehaviour
    {
        // Methods
        public void Show()
        {
            LOG.Info($"Show()", this);

            foreach (var (l, i) in leaves.Select((l, i) => (l, i)))
                DOVirtual.DelayedCall(delay * i, () => l.Show());
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Leaf[] leaves = null;
        [Header("★ Config")]
        [SerializeField] private float delay = 0.2f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}
