using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("idle2")] Idle2,
        [Animation("idle_L")] IdleL,
        [Animation("idle_R")] IdleR,
        [Animation("gogo")] Gogo,
        [Animation("join")] Join,
        [Animation("appear")] Appear,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,
        [Animation("turn_L")] TurnLeft,
        [Animation("turn_R")] TurnRight,
        [Animation("walk_L")] WalkLeft,
        [Animation("walk_R")] WalkRight,
        [Animation("key")] Key
    }

    public class CharacterAni : AnimationSpine<CharacterAnimation>
    {
    }
}