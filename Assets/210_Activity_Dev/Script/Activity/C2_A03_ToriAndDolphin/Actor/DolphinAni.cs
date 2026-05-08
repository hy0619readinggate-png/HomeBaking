using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A03
{
    // 코드에서 사용하지 않음
    public enum DolphinAnimation
    {
        [DefaultAnimation]
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2
    }

    public class DolphinAni : AnimationSpineUI<DolphinAnimation>
    {
        // Methods
        public void SetSkin(int type)
        {
            var skinName = type switch
            {
                1 => "Whale_1",
                2 => "Whale_2",
                3 => "Whale_3",
                _ => "default"
            };

            SkeletonGraphic.Skeleton.SetSkin(skinName);
            SkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            SkeletonGraphic.Update();
        }
    }
}