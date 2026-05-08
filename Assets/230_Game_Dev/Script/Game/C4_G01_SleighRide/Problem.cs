using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C4_G01
{
	public class Problem : MonoBehaviour
	{
		[SerializeField] Drone drone;
		[SerializeField] ProblemBubble bubbleA, bubbleB;

		void Start()
		{
			SetQuiz();
		}

		bool bCheck = false;
		public void SetQuiz()
		{
			bCheck = false;
			StartCoroutine(IESetQuiz());
		}

		IEnumerator IESetQuiz()
		{
			yield return new WaitWhile(() => C4_G01_Main.Instance == null);
			yield return new WaitUntil(() => C4_G01_Main.Instance.IsSetupQuiz);
			Quiz quiz = C4_G01_Main.Instance.GetQuiz();
			drone.SetQuiz(quiz.Words[0].Word == quiz.Word ? quiz.Words[0] : quiz.Words[1]);
			bubbleA.SetQuiz(quiz.Words[0], quiz.Word);
			bubbleB.SetQuiz(quiz.Words[1], quiz.Word);
		}

		void Update()
		{
			if (bCheck) return;
			if (Camera.main.WorldToViewportPoint(transform.position).x < -0.5)
			{
				bCheck = true;
				C4_G01_Main.Instance.CheckQuizCount();
			}
		}
	}
}