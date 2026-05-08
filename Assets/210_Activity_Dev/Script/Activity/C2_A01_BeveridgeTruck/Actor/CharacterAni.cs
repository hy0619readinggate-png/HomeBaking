using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A01
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle1,
        [Animation("idle2")] Idle2,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,
    }

    public class CharacterAni : AnimationSpineUI<CharacterAnimation>
    {
    }
}