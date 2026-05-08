using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C2_G03
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
			yield return new WaitWhile(() => C2_G03_Main.Instance == null);
			yield return new WaitUntil(() => C2_G03_Main.Instance.IsSetupQuiz);
			Quiz quiz = C2_G03_Main.Instance.GetQuiz();
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
				C2_G03_Main.Instance.CheckQuizCount();
			}
		}
	}
}