using beyondi.Util;
using DoDoEng.Common;
using UnityEngine;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Game.C4_G02
{
	public enum CharacterAnimation
	{
		[DefaultAnimation]
		[Animation("idle")] Idle,
		[Animation("correct")] Correct,
		[Animation("correct2")] Correct2,
		[Animation("correct3")] Correct3,
		[Animation("correct_outro")] CorrectOutro,
		[Animation("fire")] Fire,
		[Animation("load")] Load,
		[Animation("load_idle")] LoadIdle,
		[Animation("wrong")] Wrong,
		[Animation("wrong_outro")] WrongOutro,
		[Animation("wrong_freeze")] Freeze,
    }

	public class Character : AnimationSpineUI<CharacterAnimation>
	{
	}
}