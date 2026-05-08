using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A06
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("idle_3")] Idle3,
        [Animation("pick")] Pick,
        [Animation("walk")] Walk,
        [Animation("correct")] Correct,
        [Animation("correct_ALT")] Correct_ALT,
        [Animation("wrong_1")] Sit,
        [Animation("wrong_2")] Shake1,
        [Animation("wrong_2_roop")] Shake2,
        [Animation("wrong_3_L")] ThrowL,
        [Animation("wrong_3_R")] ThrowR,
        [Animation("wrong")] Wrong,

        [Animation("sit_idle_1")] SitIdle1,
        [Animation("sit_idle_2")] SitIdle2,
        [Animation("sit_correct")] SitCorrect1,
        [Animation("sit_correct2")] SitCorrect2,
        [Animation("sit_correct3")] SitCorrect3,
        [Animation("sit_correct_ALT")] SitCorrect_ALT,
        [Animation("sit_wrong")] SitWrong,
    }
    public class CharacterAni : AnimationSpineUI<CharacterAnimation>
    {
        // Methods
        public void Correct(bool submit)
        {
            LOG.Info($"Correct() | {submit}", this);

            var idx = UtilArray.RandomOne(0, correctAnimations.Length - 1);
            if (submit)
                AudioMGR.One.PlayEffect(correctCLIP[idx]);
            PlayAnimationLoop(correctAnimations[idx]);
        }
        public void PlayWrongShakeSFX()
        {
            LOG.Info($"PlayWrongShakeSFX()", this);

            var clip = UtilArray.ExtractOne(wrongShakeCLIP);
            AudioMGR.One.PlayEffect(clip);
        }



        // Unity Inspectors
        [Header("¡Ú Sound")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip[] wrongShakeCLIP = null;
        [Header("¡Ú Config")]
        [SerializeField] private CharacterAnimation[] correctAnimations = null;
    }
}