using beyondi.Util;
using DoDoEng.Common;

namespace DoDoEng.Activity.C1_A02
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("correct")] Correct,
        [Animation("correct2")] Correct2,
        [Animation("wrong")] Wrong,
    }
    public class S2CharacterAni : AnimationSpineUI<CharacterAnimation>
    {
    }
}