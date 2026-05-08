using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DoDoEng.MyRoom.Popup
{
    public enum GotchaMachineAnimation
    {
        [DefaultAnimation]
        [Animation("Gotcha_BRS")] BRS,
        [Animation("Gotcha_BSR")] BSR,
        [Animation("Gotcha_RBS")] RBS,
        [Animation("Gotcha_RSB")] RSB,
        [Animation("Gotcha_SBR")] SBR,
        [Animation("Gotcha_SRB")] SRB,
        [Animation("idle")] Idle,
        [Animation("shuffle")] Shuffle,
    }

    public class GotchaMachine : AnimationSpineUI<GotchaMachineAnimation>
    {
    }
}