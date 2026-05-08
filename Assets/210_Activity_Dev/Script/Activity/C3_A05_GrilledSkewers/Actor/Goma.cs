using DoDoEng.Common;
using Spine.Unity;

namespace DoDoEng.Activity.C3_A05
{
    public enum GomaAnimation
    {
    }

    public class Goma : AnimationSpineUI<GomaAnimation>
    {
        // Methods
        public void Empty()
        {
            LOG.Info($"Empty()", this);

            var skinName = $"Stick_0";
            updateSkin(skinName);
        }
        public void TakeOut(int ingredient)
        {
            LOG.Info($"TakeOut() | {ingredient}", this);

            var skinName = $"Stick_{ingredient}_{1}";
            updateSkin(skinName);
        }
        public void Grilled(int ingredient)
        {
            LOG.Info($"Grilled() | {ingredient}", this);

            var skinName = $"Stick_{ingredient}_{2}";
            updateSkin(skinName);
        }
        public void Wrong(int ingredient)
        {
            LOG.Info($"Wrong()", this);

            var skinName = $"Stick_{ingredient}_{3}";
            updateSkin(skinName);
        }



        // Functions
        private void updateSkin(string skinName)
        {
            SkeletonGraphic.Skeleton.SetSkin(skinName);
            SkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            SkeletonGraphic.Update();
        }
    }
}