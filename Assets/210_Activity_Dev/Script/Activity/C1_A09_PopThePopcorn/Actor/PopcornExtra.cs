using beyondi.Util;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public class PopcornExtra : Popcorn
    {
        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            Util.SetActiveAllChildren(transform, false);

            var idx = Random.Range(0, transform.childCount);
            transform.GetChild(idx).gameObject.SetActive(true);
        }
        private void Start()
        {
        }
    }
}