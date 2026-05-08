using UnityEngine;

namespace DoDoEng.Common
{
    public class StatePlayAudio : StateMachineBehaviour
    {
        // Fields
        private bool isPlayed = false;
        private int currentLoopIndex = 0;

        // Overrides
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            isPlayed = false;
            currentLoopIndex = 0;
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (clip == null)
                return;

            if (loop &&
                Mathf.FloorToInt(stateInfo.normalizedTime) > currentLoopIndex)
            {
                currentLoopIndex++;
                isPlayed = false;
            }

            if (!isPlayed &&
                (stateInfo.normalizedTime - currentLoopIndex) * stateInfo.length >= delay)
            {
                isPlayed = true;
                if (longLastingEffect)
                    AudioMGR.One.PlayEffectLL(clip);
                else AudioMGR.One.PlayEffect(clip);
            }
        }
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (longLastingEffect)
                AudioMGR.One.StopEffectLL();
        }


        // Unity Inspectors
        [Header("¡Ú Sound")]
        [SerializeField] private AudioClip clip = null;
        [Header("¡Ú Config")]
        [Range(0f, 5f)]
        [SerializeField] private float delay = 0.5f;
        [SerializeField] private bool loop = false;
        [SerializeField] private bool longLastingEffect = false;

    }
}