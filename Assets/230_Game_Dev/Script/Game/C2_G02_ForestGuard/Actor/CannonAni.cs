using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    public enum CannonAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("drag")] Drag,
        [Animation("attack")] Attack,
        [Animation("demage")] Damage,
        [Animation("death")] Death,
        [Animation("spawn")] Spawn,
        [Animation("despawn")] Despawn,
    }

    public class CannonAni : AnimationSpineUI<CannonAnimation>
    {
    }
}