using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A11
{
    public enum MonsterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("touch")] Touch
    }

    public class MonsterAni : AnimationSpineUI<MonsterAnimation>
    {
        // Methods
        public void SetColor(int colorIdx)
        {
            var skinName = $"monster_{colorIdx}";
            SkeletonGraphic.Skeleton.SetSkin(skinName);
            SkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            SkeletonGraphic.Update();
        }
    }
}