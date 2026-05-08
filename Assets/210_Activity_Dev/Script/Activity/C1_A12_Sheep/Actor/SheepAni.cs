using beyondi.Util;
using DoDoEng.Common;
using Spine.Unity;
using UnityEngine;

namespace DoDoEng.Activity.C1_A12
{
    public enum SheepAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("idle2")] Idle2,
        [Animation("idle3")] Idle3,
        [Animation("walk_L")] WalkL,
        [Animation("walk_R")] WalkR,
        [Animation("turn_L")] TurnL,
        [Animation("turn_R")] TurnR,
        [Animation("correct")] Correct,
        [Animation("correct_in_L")] CorrectL,
        [Animation("correct_in_R")] CorrectR,
        [Animation("wrong_L")] WrongL,
        [Animation("wrong_R")] WrongR,
    }

    public class SheepAni : AnimationSpine<SheepAnimation>
    {
        // Properties
        public int SorderOrder
        {
            get => rdr.sortingOrder;
            set => rdr.sortingOrder = value;
        }

        // Methods
        public void SetSkin(int type)
        {
            var skinName = type switch
            {
                1 => "black",
                2 => "pink",
                3 => "white",
                _ => "default"
            };

            SkeletonAnimation.Skeleton.SetSkin(skinName);
            SkeletonAnimation.Skeleton.SetSlotsToSetupPose();
            //SkeletonAnimation.Update();
        }



        // Fields : caching
        private MeshRenderer rdr_ = null;
        private MeshRenderer rdr => rdr_ ??= GetComponent<MeshRenderer>();
    }
}