using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

namespace DoDoEng.Game.C2_G03
{
	public class Countdown : MonoBehaviour
	{
		Animator animator;
		PlayableDirector playableDirector;
		private void Awake()
		{
			animator = GetComponent<Animator>();
			playableDirector = GetComponent<PlayableDirector>();
		}
		public Coroutine StartCountdown()
		{
			return StartCoroutine(IECountdown());
		}

		IEnumerator IECountdown()
		{
			yield return null;
			animator.Play("Countdown");
			playableDirector.Play();
			yield return new WaitForSeconds(3.3f);
		}
	}
}