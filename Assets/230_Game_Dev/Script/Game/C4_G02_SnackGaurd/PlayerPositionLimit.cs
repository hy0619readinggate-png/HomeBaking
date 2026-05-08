using UnityEngine;

namespace DoDoEng.Game.C4_G02
{
	public class PlayerPositionLimit : MonoBehaviour
	{
		// Properties
		public Transform YMIN => min;
		public Transform MAX => max;
		public Transform XMIN => xMin;
		public Transform XMAX => xMax;


		// Unity Inspectors
		[Header("★ Bindings")]
		[SerializeField] private Transform min = null;
		[SerializeField] private Transform max = null;
		[SerializeField] private Transform xMin = null;
		[SerializeField] private Transform xMax = null;
	}
}