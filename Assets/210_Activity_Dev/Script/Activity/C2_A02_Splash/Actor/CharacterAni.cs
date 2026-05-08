using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A02
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle_1")] Idle1,
        [Animation("idle_2")] Idle2,
        [Animation("idle_3")] Idle3,
        [Animation("idle_ship")] IdleOnShip,
        [Animation("start")] Start,
        [Animation("walk")] Walk,
        [Animation("jump")] Jump,
        [Animation("happy")] Happy1,
        [Animation("happy2")] Happy2,
        [Animation("happy3")] Happy3,
        [Animation("wrong")] Wrong1,
        [Animation("wrong2")] Wrong2,
        [Animation("ending_1")] Cheese1,
        [Animation("ending_2")] Cheese2,
        [Animation("ending_3")] Cheese3
    }

    public class CharacterAni : AnimationSpineUI<CharacterAnimation>
    {
        // Properties
        public AudioClip[] CheeseClips => cheeseCLIP;

        // Methods
        public void Wrong()
        {
            LOG.Info($"Wrong()", this);

            // Sound는 Spine Audio Clips로 제어
            var ani = UtilArray.ExtractOne(wrongAnimations);
            PlayAnimation(ani, false);

        }
        public void Cheese()
        {
            LOG.Info($"Cheese()", this);

            PlayAnimationLoopT2(cheeseAnimations);
        }



        // Unity Inspectors
        [Header("★ Sound")]
        [SerializeField] private AudioClip[] cheeseCLIP = null;
        [Header("★ Config")]
        [SerializeField] private CharacterAnimation[] wrongAnimations = null;
        [SerializeField] private CharacterAnimation[] cheeseAnimations = null;

    }
}