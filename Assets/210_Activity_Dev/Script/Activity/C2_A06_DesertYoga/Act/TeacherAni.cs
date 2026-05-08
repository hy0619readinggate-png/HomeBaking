using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A06
{
    public enum TeacherAnimation
    {
        [DefaultAnimation]
        [Animation("yoga_1")] Idle1,
        [Animation("yoga_2")] Idle2,
        [Animation("correct_1")] Correct1,
        [Animation("correct_2")] Correct2,
        [Animation("wrong_1")] Wrong1,
        [Animation("wrong_2")] Wrong2,
        [Animation("out_1")] Out1,
        [Animation("out_2")] Out2,
    }

    public class TeacherAni : AnimationSpineUI<TeacherAnimation>
    {
    }
}