using UnityEngine;

namespace DoDoEng.Game.C3_G01
{
	public class PlayerPointChecker : MonoBehaviour
	{
		// Properties
		public float XPos => xPos;



		// Fields
		private float xPos = 0f;



		// Unity Messages
        private void OnTriggerEnter(Collider other)
        {
			var col = other.gameObject.GetComponent<PlayerPoint>();
			if (col != null)
				xPos = col.transform.position.x;
        }

    }
}