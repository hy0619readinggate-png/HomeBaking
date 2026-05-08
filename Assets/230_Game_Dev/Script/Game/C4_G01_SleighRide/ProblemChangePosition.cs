namespace DoDoEng.Game.C4_G01
{
	public class ProblemChangePosition : ChagePosition
	{
		Problem problem => GetComponent<Problem>();
		protected override void Change()
		{
			base._change();
			problem.SetQuiz();
			RailGroupManager manager = transform.parent.GetComponent<RailGroupManager>();
			if (manager != null) manager.ResetN();
		}
	}
}