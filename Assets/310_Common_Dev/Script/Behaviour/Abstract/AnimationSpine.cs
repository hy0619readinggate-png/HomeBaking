using beyondi.Util;
using Spine.Unity;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Common
{
    public abstract class AnimationSpine<T> : MonoBehaviour where T : struct, System.IConvertible
    {
        // Methods
        public bool IsAnimationPlaying(T animation)
        {
            return isAnimationPlaying(animation);
        }
        public void FlipX(bool defaultDirection = true)
        {
            skeletonAnimation.Skeleton.ScaleX = defaultDirection ? 1 : -1;
        }

        // Methods
        public IEnumerator PlayAnimationAndWait(T animation, T animationFin)
        {
            using (LOG.Coroutine($"PlayAnimationAndWait() | {animation}, {animationFin}", this))
            {
                loopT2Playing = false;

                playSound(animation);
                yield return playAnimationAndWait(animation);
                yield return null;

                playAnimation(animationFin);
                yield return null;
            }
        }
        public IEnumerator PlayAnimationAndWait(T animation, bool defaultAtFin = true)
        {
            using (LOG.Coroutine($"PlayAnimationAndWait() | {animation}", this))
            {
                loopT2Playing = false;

                playSound(animation);
                yield return playAnimationAndWait(animation);
                yield return null;

                if (defaultAtFin)
                {
                    playAnimation(DefaultAnimationAttribute.GetDefault<T>());
                    yield return null;
                }
            }
        }
        public void PlayAnimationLoop(T animation)
        {
            LOG.Info($"PlayAnimationLoop() | {animation}", this);

            loopT2Playing = false;

            var aniName = getAnimationName(animation);
            skeletonAnimation.AnimationState.SetAnimation(0, aniName, true);

            playSound(animation);
        }
        public void PlayAnimationLoopT1(params T[] animation)
        {
            LOG.Info($"PlayAnimationLoopT1() | {animation}", this);

            loopT2Playing = false;

            UtilArray.Shuffle(animation);

            var aniF = animation.First();
            var aniFName = getAnimationName(aniF);
            var te = skeletonAnimation.AnimationState.SetAnimation(0, aniFName, true);
            te.TrackTime = Random.Range(0f, 2f);

            foreach (var ani in animation.Skip(1))
            {
                var aniName = getAnimationName(ani);
                skeletonAnimation.AnimationState.AddAnimation(0, aniName, true, 0);
            }
        }
        public void PlayAnimationLoopT2(params T[] animation)
        {
            LOG.Info($"PlayAnimationLoopT2() | {animation}", this);

            loopT2Playing = true;
            playAnimationLoopRecursive(animation, Random.Range(0f, 2f));
        }
        public void PlayAnimation(T animation, T animationFin)
        {
            LOG.Info($"PlayAnimation() | {animation}, {animationFin}", this);

            loopT2Playing = false;

            var aniName = getAnimationName(animation);
            var aniNameFin = getAnimationName(animationFin);

            skeletonAnimation.AnimationState.SetAnimation(0, aniName, false);
            skeletonAnimation.AnimationState.AddAnimation(0, aniNameFin, true, 0);

            playSound(animation);
        }
        public void PlayAnimation(T animation, bool defaultAtFin = true)
        {
            LOG.Info($"PlayAnimation() | {animation}", this);

            loopT2Playing = false;

            var aniName = getAnimationName(animation);
            skeletonAnimation.AnimationState.SetAnimation(0, aniName, false);

            if (defaultAtFin)
            {
                var animationDefault = DefaultAnimationAttribute.GetDefault<T>();
                var aniNameDefault = getAnimationName(animationDefault);
                skeletonAnimation.AnimationState.AddAnimation(0, aniNameDefault, true, 0);
            }

            playSound(animation);
        }
        public void AbortAnimation()
        {
            LOG.Info($"AbortAnimation()", this);

            loopT2Playing = false;

            var animationDefault = DefaultAnimationAttribute.GetDefault<T>();
            if (!isAnimationPlaying(animationDefault))
                playAnimation(animationDefault);
        }

        // Methods
        public void SetAnimation(T animation)
        {
            var aniName = getAnimationName(animation);
            SkeletonAnimation.AnimationState.SetAnimation(0, aniName, false);
            SkeletonAnimation.Skeleton.SetSlotsToSetupPose();
            SkeletonAnimation.LateUpdate();
        }

        // Methods
        public IEnumerator WaitForCurrent()
        {
            var trackEntry = skeletonAnimation.AnimationState.GetCurrent(0);
            if (trackEntry != null)
                yield return new WaitForSpineAnimationComplete(trackEntry);
        }



        // Fields : caching
        private SkeletonAnimation skeletonAnimation_ = null;
        private SkeletonAnimation skeletonAnimation => skeletonAnimation_ ??= GetComponent<SkeletonAnimation>();
        private SpineAudioClips clips_ = null;
        private SpineAudioClips clips => clips_ ??= GetComponent<SpineAudioClips>();

        // Fields
        private bool loopT2Playing = false;

        // Functions
        protected SkeletonAnimation SkeletonAnimation => skeletonAnimation;

        // Functions
        private string getAnimationName(T animation)
        {
            var aniEnum = animation as System.Enum;
            return aniEnum.GetAttribute<AnimationAttribute>().Name;
        }
        private bool isAnimationPlaying(T animation)
        {
            var aniName = getAnimationName(animation);
            return aniName == skeletonAnimation.AnimationState.GetCurrent(0).Animation.Name;
        }

        // Functions
        private IEnumerator playAnimationAndWait(T animation)
        {
            var aniName = getAnimationName(animation);
            var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, aniName, false);
            yield return new WaitForSpineAnimationComplete(trackEntry);

        }
        private void playAnimation(T animation, bool loop = true)
        {
            var aniName = getAnimationName(animation);
            skeletonAnimation.AnimationState.SetAnimation(0, aniName, loop);
        }

        // Functions
        private void playSound(T animation)
        {
            if (clips != null)
            {
                var aniName = getAnimationName(animation);
                var clip = clips.ClipOf(aniName);
                if (clip != null)
                    AudioMGR.One.PlayEffect(clip);
            }
        }

        // Functions : recursive
        private void playAnimationLoopRecursive(T[] animation, float skipSecond = 0)
        {
            var aniF = UtilArray.ExtractOne(animation);
            var aniFName = getAnimationName(aniF);
            var te = skeletonAnimation.AnimationState.SetAnimation(0, aniFName, true);
            te.TrackTime = skipSecond;
            te.Complete += (te) =>
            {
                if (loopT2Playing)
                    playAnimationLoopRecursive(animation);
            };
        }



        // Unity Messages
        protected virtual void Awake()
        {
            if (!typeof(T).IsEnum)
                throw new System.ArgumentException("T must be an enumerated type");
        }
    }
}