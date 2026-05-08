using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C2_A05
{
    public enum ChameleonAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("hit1")] Hit1,
        [Animation("hit2")] Hit2,
        [Animation("hit3")] Hit3,
        [Animation("hit4")] Hit4,
        [Animation("hit5")] Hit5,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("wrong")] Wrong
    }

    public class ChameleonAni : AnimationSpineUI<ChameleonAnimation>
    {
    }
}