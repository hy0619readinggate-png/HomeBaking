using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C1_A12
{
    public enum PapaAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("idle2")] Idle2,
        [Animation("quiz")] Quiz1,
        [Animation("quiz2")] Quiz2,
        [Animation("correct")] Correct1,
        [Animation("correct2")] Correct2,
        [Animation("correct3")] Correct3,
        [Animation("wrong")] Wrong,
        [Animation("laugh")] Laugh,
        [Animation("walk_L")] WalkL,
        [Animation("walk_R_To_idle")] WalkR_toIdle,
        [Animation("walk_R")] WalkR,
    }

    public class PapaAni : AnimationSpine<PapaAnimation>
    {
        // Methods
        public IEnumerator Correct()
        {
            LOG.Info($"Correct()", this);

            var ani = UtilArray.ExtractOne(correctAnimations);
            yield return PlayAnimationAndWait(ani);
        }
        public IEnumerator Wrong()
        {
            LOG.Info($"Wrong()", this);

            var ani = UtilArray.ExtractOne(wrongAnimations);
            yield return PlayAnimationAndWait(ani);
        }



        // Unity Inspectors
        [Header("¡Ú Config")]
        [SerializeField] private PapaAnimation[] correctAnimations = null;
        [SerializeField] private PapaAnimation[] wrongAnimations = null;
    }
}