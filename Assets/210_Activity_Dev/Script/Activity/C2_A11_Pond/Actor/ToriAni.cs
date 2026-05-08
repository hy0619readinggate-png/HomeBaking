using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A11
{
    public enum ToriAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("jump")] Jump,
        [Animation("walk")] Walk,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,
    }

    public class ToriAni : AnimationSpineUI<ToriAnimation>
    {
    }
}