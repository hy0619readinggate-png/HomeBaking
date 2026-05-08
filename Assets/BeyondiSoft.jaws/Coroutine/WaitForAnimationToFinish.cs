using UnityEngine;

namespace beyondi.Coroutine
{

    /// <summary>
    ///     Waits for an animation to finish
    /// </summary>
    public class WaitForAnimationToFinish : CustomYieldInstruction
    {

        private readonly string animationName;

        private readonly Animator animator;
        private readonly int layerIndex;
        private bool isFirst = false;

        private AnimatorStateInfo StateInfo => animator.GetCurrentAnimatorStateInfo(layerIndex);

        private bool CorrectAnimationIsPlaying => StateInfo.IsName(animationName);

        private bool AnimationIsDone => StateInfo.normalizedTime >= 1;

        public override bool keepWaiting
        {
            get
            {
                // 완료된 애니메이션을 기다리려고 할때, 1프레임 지연 by swon
                if (isFirst)
                {
                    isFirst = false;
                    return true;
                }
                return !CorrectAnimationIsPlaying || !AnimationIsDone;
            }
        }


        /// <summary>
        ///     Creates a new yield-instruction
        /// </summary>
        /// <param name="animator">The animator to track</param>
        /// <param name="animationName">The name of the animation</param>
        /// <param name="layerIndex">The layer the animation is playing on</param>
        public WaitForAnimationToFinish(Animator animator, string animationName, int layerIndex = 0)
        {
            this.isFirst = true;

            this.animator = animator;
            this.layerIndex = layerIndex;
            this.animationName = animationName;
        }

    }

}