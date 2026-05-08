using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A07
{
    public enum RivalAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("correct")] Correct,
        [Animation("wrong")] Wrong,
        [Animation("correct")] Finish
    }

    public class RivalAni : AnimationSpineUI<RivalAnimation>
    {
    }
}