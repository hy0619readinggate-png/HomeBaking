using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Activity.C1_A09
{
    public enum MachineCapAnimation
    {
        [DefaultAnimation]
        [Animation("Idle")] Idle,
        [Animation("CapOpen")] Open,
        [Animation("CapClose")] Close,
    }

    public class MachineCap : AnimationMecanim<MachineCapAnimation>
    {
    }
}