using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A01
{
    public enum OvenAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("baking")] Bake,
        [Animation("baked_idle")] BakedIdle,
        [Animation("baked_idle_affordance")] BakedAff,
        [Animation("open")] Open,
        [Animation("open_idle")] Opened,
    }

    public class Oven : AnimationSpineUI<OvenAnimation>
    {
        private void Start()
        {
        }
    }
}