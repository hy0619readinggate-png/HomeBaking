using beyondi.Util;
using DoDoEng.Common;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace DoDoEng.Activity.C1_A11
{
    public enum GinoAnimation
    {
        [DefaultAnimation]
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("idle_3")] Idle3,
        [Animation("correct")] Correct,
        [Animation("surprise")] Surprise,
        [Animation("Run")] Run,
        [Animation("relax")] Relax,
        [Animation("walk")] Walk,
    }
    public class GinoAni : AnimationSpineUI<GinoAnimation>
    {
        // Properties
        public float ScaleX
        {
            get => SkeletonGraphic.Skeleton.ScaleX;
            set => SkeletonGraphic.Skeleton.ScaleX = value;
        }
    }
}