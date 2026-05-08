using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    public enum MonsterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("walk")] Walk,
        [Animation("attack")] Attack1,
        [Animation("attack2")] Attack2,
        [Animation("block")] Block,
        [Animation("demage")] Damage,
        [Animation("death")] Death,
        [Animation("out")] Out,
        [Animation("spawn")] Spawn,
    }

    public class MonsterAni : AnimationSpineUI<MonsterAnimation>
    {
    }
}