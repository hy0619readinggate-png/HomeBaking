using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A04
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("correct_2")] Correct2
    }

    public class CharacterAni : AnimationSpineUI<CharacterAnimation>
    {
    }
}