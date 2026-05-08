using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A05
{
    public enum LeoniAnimation
    {
        [DefaultAnimation]
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("correct_0")] Correct0_1,
        [Animation("correct_0_2")] Correct0_2,
        [Animation("correct_0_3")] Correct0_3,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,

        [Animation("correct_1")] Correct1,
        [Animation("correct_2")] Correct2,
        [Animation("intro")] Intro,
        [Animation("walk")] Walk,
    }

    public class Leoni : AnimationSpineUI<LeoniAnimation>
    {
    }
}