using System.Collections;
using UnityEngine;
using TMPro;

namespace DoDoEng.Game.C4_G01
{
	public class ProblemBubble : MonoBehaviour
	{
		[SerializeField] GameObject star, fxStar, fxBubble;
		[SerializeField] TextMeshProUGUI quizText;

		bool OX = false;
		public bool IsCorrect => OX;

		bool bHit = false;
		public bool IsHit => bHit;
		bool bSkip = false;
		public void Skip()
		{
			bSkip = true;
			if (!IsCorrect) return;
			Character.Instance.IsShowQuiz = false;
		}

		public AudioClip wordClip = null;
		public void SetQuiz(QuizData quiz, string word)
		{
			bSkip = false;
			bHit = false;
			Character.Instance.IsAliveQuiz = true;
			gameObject.SetActive(true);
			star.SetActive(true);
			fxStar.SetActive(false);
			fxBubble.SetActive(false);
			quizText.text = quiz.Word;
			OX = quiz.Word == word;
			wordClip = quiz.WordCLIP;
			Debug.Log($"QUIZ: {quizText.text} => {OX}");
		}

		public void Hit()
		{
			if (!Character.Instance.IsAliveQuiz) return;
			Character.Instance.IsAliveQuiz = false;
			bHit = true;
			StartCoroutine(IEhit());
		}

		IEnumerator IEhit()
		{
			yield return null;
			star.SetActive(false);
			fxStar.SetActive(OX);
			fxBubble.SetActive(true);
			yield return new WaitForSeconds(1.0f);
			gameObject.SetActive(false);
		}

		void Update()
		{
			if (bSkip || !IsCorrect) return;
			Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
			if (pos.x > 1f) return;
			if (pos.x < 0f)
			{
				Skip();
				return;
			}
			Character.Instance.IsShowQuiz = true;
		}
	}
}
