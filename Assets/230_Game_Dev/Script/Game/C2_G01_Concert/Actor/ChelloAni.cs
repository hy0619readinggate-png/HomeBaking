using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C2_G01
{
    public enum ChelloAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle1,
        [Animation("idle2")] Idle2,
        [Animation("dance_idle")] Dance_Idle1,
        [Animation("dance_idle2")] Dance_Idle2,
        [Animation("dance_1")] Dance1,
        [Animation("dance_2")] Dance2,
        [Animation("dance_3")] Dance3,
        [Animation("dance_4")] Dance4,
        [Animation("dance_5")] Dance5,
        [Animation("dance_6")] Dance6,
        [Animation("dance_7")] Dance7,
        [Animation("wrong1")] Wrong1,
        [Animation("wrong2")] Wrong2,
    }

    public class ChelloAni : AnimationSpineUI<ChelloAnimation>
    {
        static readonly ChelloAnimation[] Dances = new ChelloAnimation[]
        {
            ChelloAnimation.Dance1,
            ChelloAnimation.Dance2,
            ChelloAnimation.Dance3,
            ChelloAnimation.Dance4,
            ChelloAnimation.Dance5,
            ChelloAnimation.Dance6,
            ChelloAnimation.Dance7,
        };

        static readonly ChelloAnimation[] DanceIdles = new ChelloAnimation[]
        {
            ChelloAnimation.Dance_Idle1,
            ChelloAnimation.Dance_Idle2,
        };

        static readonly ChelloAnimation[] Idles = new ChelloAnimation[]
        {
            ChelloAnimation.Idle1,
            ChelloAnimation.Idle2,
        };
        static readonly ChelloAnimation[] Wrongs = new ChelloAnimation[]
        {
            ChelloAnimation.Wrong1,
            ChelloAnimation.Wrong2,
        };


        public void PlayIdle()
        {
            PlayAnimationLoopT2(Idles);
        }
        public void PlayDance()
        {
            PlayAnimationLoopT2(DanceIdles);
        }
        public void PlayAction()
        {
            var dance = UtilArray.ExtractOne(Dances);
            var danceIdle = UtilArray.ExtractOne(DanceIdles);
            PlayAnimation(dance, danceIdle);
        }
        public void PlayWrong()
        {
            var wrong = UtilArray.ExtractOne(Wrongs);
            var danceIdle = UtilArray.ExtractOne(DanceIdles);
            PlayAnimation(wrong, danceIdle);
        }
        public void PlayFever()
        {
            PlayAnimationLoopT2(Dances);
        }
    }
}