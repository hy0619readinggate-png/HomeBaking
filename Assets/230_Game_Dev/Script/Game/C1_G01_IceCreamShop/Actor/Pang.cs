using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G01
{
    public enum PangAnimation
    {
        [Animation("appear")] Appear,
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("wand")] Wand,
        [Animation("wand_idle")] WandIdle,
        [Animation("smile_out")] SmileOut,
        [Animation("hammer")] Hammer,
        [Animation("sad_out")] SadOut,
    }
    public class Pang : AnimationSpineUI<PangAnimation>
    {
    }
}