using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A06
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle1,
        [Animation("idle2")] Idle2,
        [Animation("drag")] Drag,
        [Animation("walk")] Walk,
        [Animation("correct_1_ready")] CorrectReady1,
        [Animation("correct_1")] Correct1,
        [Animation("correct_1_idle")] CorrectIdle1,
        [Animation("correct_2_ready")] CorrectReady2,
        [Animation("correct_2")] Correct2,
        [Animation("correct_2_idle")] CorrectIdle2,
        [Animation("wrong_1")] Wrong1,
        [Animation("wrong_2")] Wrong2,
        [Animation("out_1")] Out1,
        [Animation("out_2")] Out2
    }

    public class CharacterAni : AnimationSpineUI<CharacterAnimation>
    {
    }
}