using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A04
{
    public enum StingrayAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("wrong")] Wrong
    }

    public class StingrayAni : AnimationSpineUI<StingrayAnimation>
    {
    }
}