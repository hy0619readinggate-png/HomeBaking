using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    public enum PapaAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("idle2")] Idle2,
        [Animation("skill")] Skill,
        [Animation("correct")] Correct,
        [Animation("wrong")] Wrong,
        [Animation("laugh")] Laugh,
        [Animation("walk_L")] WalkL,
        [Animation("walk_R_To_idle")] WalkR_toIdle,
        [Animation("walk_R")] WalkR,
        [Animation("demage")] Damage,
        [Animation("mid_idle")] Caution,
    }

    public class PapaAni : AnimationSpineUI<PapaAnimation>
    {
    } 
}