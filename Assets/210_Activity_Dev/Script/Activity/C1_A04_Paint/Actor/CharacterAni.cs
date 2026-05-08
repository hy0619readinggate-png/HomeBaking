using beyondi.Util;
using DoDoEng.Common;

namespace DoDoEng.Activity.C1_A04
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("idle_3")] Idle3,
        [Animation("idle_eye")] Idle4,
        [Animation("correct")] Correct,
        [Animation("wrong")] Wrong,
        [Animation("3.happy")] Happy,
    }
    public class CharacterAni : AnimationSpineUI<CharacterAnimation>
    {
    }
}