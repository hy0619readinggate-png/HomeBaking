using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A09
{
    public enum EdmondAnimation
    {
        [DefaultAnimation]
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("swing")] Swing,
        [Animation("correct")] Correct,
        [Animation("wrong")] Wrong,
        [Animation("end")] End,
        [Animation("end_loop")] EndLoop,
    }
    public class EdmondAni : AnimationSpineUI<EdmondAnimation>
    {
    }
}