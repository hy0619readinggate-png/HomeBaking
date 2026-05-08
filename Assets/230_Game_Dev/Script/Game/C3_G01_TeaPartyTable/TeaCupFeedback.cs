using UnityEngine;


namespace DoDoEng.Game.C3_G01
{
	public class TeaCupFeedback : MonoBehaviour
	{

		// Methods
		public void Wrong()
		{
			foreach (var fb in _TeaCupFeedbackANI)
				fb.SetTrigger(hashkey_Wrong);
		}
		public void Correct()
		{
			foreach (var fb in _TeaCupFeedbackANI)
				fb.SetTrigger(hashkey_Correct);
		}



        // Fields : readonly
        private readonly int hashkey_Correct = Animator.StringToHash("Correct");
        private readonly int hashkey_Wrong = Animator.StringToHash("Wrong");



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private Animator[] _TeaCupFeedbackANI = null;
	}
}