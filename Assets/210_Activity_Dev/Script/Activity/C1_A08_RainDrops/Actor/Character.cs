using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A08
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("backward")] Backward,
        [Animation("forward")] Forward,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,
        [Animation("correct_clear")] Happy,

        [Animation("ending")] Ending,
        [Animation("opening")] Opening,
    }

    public class Character : AnimationSpineUI<CharacterAnimation>
    {
    }
}