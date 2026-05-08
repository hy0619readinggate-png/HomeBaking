using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Game.C4_G01
{
	public class Drone : MonoBehaviour
	{
		[SerializeField] Image image;
		Vector3 orgPos;
		void Start()
		{
			orgPos = transform.position;
		}

		bool bNarr = false;
		AudioClip wordClip = null;
		bool bDroneAudio = false;
		public void SetQuiz(QuizData quiz)
		{
			bNarr = false;
			bDroneAudio = false;
			transform.position = orgPos;
			image.sprite = quiz.WordSPR;
			wordClip = quiz.WordCLIP;
		}

		void Update()
		{
			Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
			if (pos.x > 1f) return;
			if (!bDroneAudio)
			{
				bDroneAudio = true;
				AudioMGR.One.PlayEffect(Character.Instance.GetAudioClip("question_in"));
			}
			bool bFixed = true;
			if (transform.localPosition.y > 450)
			{
				transform.Translate(0, -Character.Instance.Speed * Time.deltaTime, 0);
				bFixed = false;
			}
			if (transform.localPosition.x < 0 && pos.x < 0.5f)
			{
				Vector3 p1 = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0, 0));
				Vector3 p2 = Camera.main.ViewportToWorldPoint(pos);
				transform.Translate(p1.x - p2.x, 0, 0);
				if (!bNarr && bFixed)
				{
					bNarr = true;
					AudioMGR.One.PlayNarration(wordClip);
					C4_G01_Main.Instance.ShowQuizIndigatorUP();
				}
			}
		}
	}
}