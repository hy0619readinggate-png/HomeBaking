using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G02
{
    public enum PlantAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("death")] Death,
        [Animation("demage")] Damage,
        [Animation("produce")] Produce,
    }

    public class PlantAni : AnimationSpineUI<PlantAnimation>
    {
    }
}