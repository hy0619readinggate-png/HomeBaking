using beyondi.Util;
using DoDoEng.Common;
using Spine.Unity;
using UnityEngine;

namespace DoDoEng.Activity.C1_A11
{
    public enum FlowerAnimation
    {
        [DefaultAnimation]
        [Animation("catch")] Catch,
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("idle_3")] Idle3
    }

    public class FlowerAni : AnimationSpineUI<FlowerAnimation>
    {
        // Methods
        public void SetSkin(int typeIdx, int colorIdx)
        {
            var colorName = colorIdx switch
            {
                0 => "blue",
                1 => "red",
                2 => "purple",
                3 => "yellow",
                4 => "pink",
                _ => "default"
            };

            var typeStr = typeIdx == 0 ? "petal" : "tulip";
            var skinName = $"{typeStr}_{colorName}";
            SkeletonGraphic.Skeleton.SetSkin(skinName);
            SkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            SkeletonGraphic.Update();
        }
    }
}