using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

#pragma warning disable 0414

namespace DoDoEng.Game.C3_G01
{
    public enum CharacterAnimation
	{
		[DefaultAnimation]
		[Animation("idle")] Idle,
		[Animation("correct")] Correct,
		[Animation("demage")] Damage,
        [Animation("item")] Broom,
        [Animation("move_left")] Left,
        [Animation("move_right")] Right,
        [Animation("success")] Success,
        [Animation("wrong")] AlphabetWrong,
        [Animation("wrong2")] Crash,
    }

    public class Character : AnimationSpineUI<CharacterAnimation>
    {
    }
}