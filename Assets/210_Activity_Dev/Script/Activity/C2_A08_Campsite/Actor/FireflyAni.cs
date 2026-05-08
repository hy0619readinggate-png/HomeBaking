using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A08
{
    public enum FireflyAnimation
    {
        [DefaultAnimation]
        [Animation("Drag")] Drag,
        [Animation("idle")] Idle,
        [Animation("move")] Move
    }

    public class FireflyAni : AnimationSpineUI<FireflyAnimation>
    {
    }
}