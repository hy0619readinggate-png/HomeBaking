using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A12
{
    public enum MilloAnimation
    {
        [DefaultAnimation]
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2,

        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("correct4")] Correct4,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,

        [Animation("burton_side_idle")] IdleWithButton,

    }
    public class MilloAni : AnimationSpineUI<MilloAnimation>
    {
    }
}