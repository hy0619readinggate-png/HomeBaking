using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G03
{
    public enum JackAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("idle_f")] IdleFront,
        [Animation("idle_Empty")] IdleEmpty,
        [Animation("idle_Empty_f")] IdleEmptyFront,
        [Animation("mining")] Mining,
        [Animation("move_back")] MoveBack,
        [Animation("move_front")] MoveFront,
        [Animation("move_side")] MoveSide,
        [Animation("time_out")] Timeout,
        [Animation("time_out_f")] TimeoutFront,
        [Animation("time_out_idle")] TimeoutIdle,
        [Animation("time_out_idle_f")] TimeoutIdleFront,
        [Animation("correct")] Correct,
        [Animation("wrong")] Wrong,
        [Animation("correct_car_end")] End,
        [Animation("correct_car_end_f")] EndFront,
    }

    public class JackAni : AnimationSpine<JackAnimation>
    {
        public JackAnimation AnimationOf(Direction direction)
        {
            return direction switch
            {
                Direction.R => JackAnimation.MoveSide,
                Direction.L => JackAnimation.MoveSide,
                Direction.T => JackAnimation.MoveBack,
                Direction.B => JackAnimation.MoveFront,
                _ => JackAnimation.MoveSide
            };
        }
        public void PlayAnimationOfMoving(Direction direction)
        {
            var ani = AnimationOf(direction);
            PlayAnimationLoop(ani);
            FlipX(direction == Direction.L);
        }
    }
}