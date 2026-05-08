using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

namespace DoDoEng.Game.C1_G01
{
    public enum CharacterAnimation
    {
        [DefaultAnimation]
        [Animation("idle")] Idle,
        [Animation("unpleasant")] Unpleasant,
        [Animation("appear")] Appear,
        [Animation("annoyed")] Annoyed,
        [Animation("correct")] Correct,
        [Animation("wrong")] Wrong,
        [Animation("reaction_correct")] ReactionCorrect,
        [Animation("reaction_wrong")] ReactionWrong,
    }
    public class Character : AnimationSpineUI<CharacterAnimation>
    {
    }
}