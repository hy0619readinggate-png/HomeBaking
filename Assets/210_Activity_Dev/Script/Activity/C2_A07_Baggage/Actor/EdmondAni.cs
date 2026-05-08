using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A07
{
    public enum EdmondAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle1,
        [Animation("idle2")] Idle2,

        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,

        [Animation("walk")] Walk,
        [Animation("outto")] Outro,
    }

    public class EdmondAni : AnimationSpineUI<EdmondAnimation>
    {
    }
}