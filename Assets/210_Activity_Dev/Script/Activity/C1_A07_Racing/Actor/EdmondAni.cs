using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;


namespace DoDoEng.Activity.C1_A07
{
    public enum EdmondAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("wrong_1")] Obstacle,
        [Animation("correct_1")] Correct1,
        [Animation("correct_2")] Correct2,
        [Animation("correct_3")] Correct3,
        [Animation("wrong_1")] Wrong1,
        [Animation("wrong_2")] Wrong2,
        [Animation("correct_2")] GoalIn
    }

    public class EdmondAni : AnimationSpineUI<EdmondAnimation>
    {
    }
}