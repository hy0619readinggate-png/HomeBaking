using beyondi.Util;
using DoDoEng.Common;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public class Progress : MonoBehaviour
    {
        // Methods
        public void SetProgress(int progress)
        {
            LOG.Info($"SetProgress() | {progress}", this);

            foreach (var (item, idx) in items.Select((it, idx) => (it, idx)))
                item.Switch(idx < progress);
        }



        // Fields : caching
        private ProgressItem[] items_ = null;
        private ProgressItem[] items => items_ ??= GetComponentsInChildren<ProgressItem>(true);



        // Unity Messages
        private void Awake()
        {
            items.ForEach(it => it.Switch(false));
        }
        private void Start()
        {
        }
    }
}