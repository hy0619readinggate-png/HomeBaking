using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G03
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("boost")] Boost,
        [Animation("corrent")] Correct, // 아마도 Correct 오기인 듯.
        [Animation("corrent2")] Correct2, // 아마도 Correct 오기인 듯.
        [Animation("corrent3")] Correct3, // 아마도 Correct 오기인 듯.
        [Animation("crash")] Crash,
        [Animation("fall")] Fall,
        [Animation("idle")] Idle,
        [Animation("jump_down")] JumpDown,
        [Animation("jump_end")] JumpEnd,
        [Animation("jump_start")] JumpStart,
        [Animation("jump_up")] JumpUp,
        [Animation("move")] Move,
        [Animation("wrong")] Wrong,
        [Animation("wrong2")] Wrong2,
    }
    public class CharacterAni : AnimationSpineUI<CharacterAnimation>
    {

    }
}