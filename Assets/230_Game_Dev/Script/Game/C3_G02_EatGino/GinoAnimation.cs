using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C3_G02
{
    public enum JinoAnimation : byte
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,

        [Animation("walk_B")] Top_Walk = 10,
        [Animation("walk_F")] Bottom_Walk,
        [Animation("walk_S")] Left_Walk,
        [Animation("walk_S")] Right_Walk,

        [Animation("eat_Wall_B")] Top_WalkEat = 20,
        [Animation("eat_Wall_F")] Bottom_WalkEat,
        [Animation("eat_Wall_S")] Left_WalkEat,
        [Animation("eat_Wall_S")] Right_WalkEat,

        [Animation("eat_Wall_B_Stand_Still")] Top_Eat = 30,
        [Animation("eat_Wall_F_Stand_Still")] Bottom_Eat,
        [Animation("eat_Wall_S_Stand_Still")] Left_Eat,
        [Animation("eat_Wall_S_Stand_Still")] Right_Eat,

        [Animation("eat_block_B")] Top_Jelly = 40,
        [Animation("eat_block_F")] Bottom_Jelly,
        [Animation("eat_block_S")] Left_Jelly,
        [Animation("eat_block_S")] Right_Jelly,

    }

    public class GinoAnimation : AnimationSpine<JinoAnimation>
    {


    }
}