using beyondi.Util;
using DoDoEng.Common;
using Spine.Unity;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle_2")] Idle2,
        [Animation("idle_3")] Idle3,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("wrong")] Wrong,
    }
    public class Character : AnimationSpineUI<CharacterAnimation>
    {
        // Definition
        private const int PopcornBoxFullCount = 3;

        // Properties
        public CharacterAnimation DefaultIdle => defaultIdle;

        // Methods
        public void Correct()
        {
            LOG.Info($"Correct()", this);

            var idx = UtilArray.RandomOne(0, correctAnis.Length - 1);
            AudioMGR.One.PlayEffectLL(correctCLIP[idx]);
            PlayAnimation(correctAnis[idx], defaultIdle);
        }
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            AudioMGR.One.PlayEffectLL(wrongCLIP);
            PlayAnimation(CharacterAnimation.Wrong, defaultIdle);
        }
        public void ClearPopcornBox()
        {
            LOG.Info($"ClearPopcornBox()", this);

            count = 0;
            updateSkin();
        }
        public void SetupPopcornBox(bool C3_A07, int emptyTextCount)
        {
            LOG.Info($"ClearPopcornBox() | {C3_A07} {emptyTextCount}", this);

            this.C3_A07 = C3_A07;
            this.emptyTextCount = emptyTextCount;
        }
        public void FillPopcornBox()
        {
            LOG.Info($"FillPopcornBox()", this);

            count++;

            // C3_A07 only
            if (C3_A07)
                count = count < emptyTextCount ? count : PopcornBoxFullCount;

            updateSkin();
        }
        public void PlayCompleteSFX()
        {
            LOG.Info($"PlayCompleteSFX()", this);

            AudioMGR.One.PlayEffect(completeCLIP);

        }


        // Fields
        private int count = 0;
        private CharacterAnimation[] correctAnis = { CharacterAnimation.Correct1, CharacterAnimation.Correct2, CharacterAnimation.Correct3 };

        // Fields : C3_A07 only
        private bool C3_A07 = false;
        private int emptyTextCount = 3;

        // Functions
        private void updateSkin()
        {
            var skinName = $"popcorn box{count + 1}";
            SkeletonGraphic.Skeleton.SetSkin(skinName);
            SkeletonGraphic.Skeleton.SetSlotsToSetupPose();
            SkeletonGraphic.Update();
        }



        // Unity Inspectors
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;
        [SerializeField] private AudioClip completeCLIP = null;
        [Header("★ Config")]
        [SerializeField] private CharacterAnimation defaultIdle = CharacterAnimation.Idle2;
    }
}