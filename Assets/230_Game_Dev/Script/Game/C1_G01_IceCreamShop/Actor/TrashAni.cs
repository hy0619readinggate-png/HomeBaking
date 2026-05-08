using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G01
{
    public enum TrashAnimation
    {
        [DefaultAnimation]
        [Animation("idle_2")] Idle2,
        [Animation("idle")] Idle,
        [Animation("eat")] Eat1,
        [Animation("eat2")] Eat2,
        [Animation("eat3")] Eat3,
        [Animation("eat4")] Eat4,
        [Animation("eat5")] Eat5,
        [Animation("eat6")] Eat6,

    }
    public class TrashAni : AnimationSpineUI<TrashAnimation>
    {
    }
}