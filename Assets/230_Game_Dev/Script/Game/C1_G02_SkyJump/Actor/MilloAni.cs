using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G02
{
    public enum MilloAnimation
    {
        [DefaultAnimation]
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("idle_3")] Idle3,

        [Animation("jump_down")] Jump_Down,
        [Animation("jump_fail")] Jump_Fail,
        [Animation("jump_land")] Jump_Land,
        [Animation("jump_start")] Jump_Start,
        [Animation("jump_up")] Jump_Up,

        // #1674 점프 모션 추가
        [Animation("jump_ready")] Jump_Ready,
        [Animation("jump_ready_hold")] Jump_ReadyHold,

        [Animation("boost_1_start")] Boost1_Start,
        [Animation("boost_1_up")] Boost1_Up,
        [Animation("boost_1_end")] Boost1_End,
        [Animation("boost_2_start")] Boost2_Start,
        [Animation("boost_2_up")] Boost2_Up,
        [Animation("boost_2_end")] Boost2_End,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("wrong")] Wrong1,
        [Animation("jump_fail")] Wrong2,

        [Animation("intro")] Intro,
    }

    public class MilloAni : AnimationSpine<MilloAnimation>
    {
    }
}