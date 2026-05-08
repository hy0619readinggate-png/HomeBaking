using UnityEngine;

namespace DoDoEng.Game.C4_G01
{
	public class ChagePosition : MonoBehaviour
	{
		[SerializeField] Transform SP, EP;
		[SerializeField] int N = 2;

		float DX;

		void Start()
		{
			DX = (EP.position.x - SP.position.x) * N;
		}

		void Update()
		{
			if (Camera.main.WorldToViewportPoint(EP.position).x < -1) Change();
		}

		protected void _change()
		{
			transform.Translate(DX, 0, 0);
		}

		protected virtual void Change()
		{
			_change();
			RailGroupManager manager = transform.parent.GetComponent<RailGroupManager>();
			if (manager != null)
			{
				manager.AddRail(transform.position);
				Destroy(gameObject);
			}
		}
	}
}