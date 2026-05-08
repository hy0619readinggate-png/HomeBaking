using beyondi.Util;
using DoDoEng.Common;
using Spine.Unity;
using UnityEngine;

namespace DoDoEng.Activity.C1_A10
{
    public enum DodoAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("idle2")] Idle2,
        [Animation("gogo")] Gogo,
        [Animation("join")] Join,
        [Animation("correct")] Correct,
        [Animation("wrong")] Wrong,
        [Animation("turn_L")] TurnLeft,
        [Animation("turn_R")] TurnRight,
        [Animation("walk_L")] WalkLeft,
        [Animation("walk_R")] WalkRight
    }

    public class DodoAni : AnimationSpine<DodoAnimation>
    {
    }
}