using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C3_A04
{
    public enum LeoniAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("idle_3")] Idle3,
        [Animation("idle_eye")] IdleEye,
        [Animation("annoyed")] Annoyed,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("correct4")] Correct4,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,
    }

    public class LeoniAni : AnimationSpine<LeoniAnimation>
    {
    }
} 