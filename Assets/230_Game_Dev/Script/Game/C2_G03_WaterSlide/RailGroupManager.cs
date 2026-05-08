using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C2_G03
{
	public class RailGroupManager : MonoBehaviour
	{
		[SerializeField] GameObject[] rails;
		[SerializeField] GameObject flatRail;
		GameObject flatRailNoItem;
		System.Random random;
		void Awake()
		{
			random = new System.Random();
		}

		int N = 0;
		string preRailName = "rail-3";
		public void ResetN()
		{
			N = 0;
			preRailName = "rail-3";
		}
		void Start()
		{
			foreach (GameObject R in rails) {
				if (R.name != "rail-3") continue;
				flatRailNoItem = R;
				break;
			}
			ResetN();
		}

		public void AddRail(Vector3 pos)
		{
			GameObject rail = null;
			if (++N < 3)
			{
				rail = (N == 1) ? Instantiate(flatRail, transform) : Instantiate(flatRailNoItem, transform);
			}
			else
			{
				rails = rails.OrderBy(x => random.Next()).ToArray();
				if (N < 6 || C2_G03_Main.Instance.currentRound == 1)
				{
					foreach (GameObject R in rails)
					{
						switch (R.name)
						{
							case "rail-1":
							case "rail-2":
							case "rail-3":
								if (preRailName == R.name) break;
								rail = Instantiate(R, transform);
								break;
						}
						if (rail != null) break;
					}
				}
				else
				{
					foreach (GameObject R in rails)
					{
						if (preRailName == R.name) continue;
						rail = Instantiate(R, transform);
						break;
					}
				}
				preRailName = rail.name;
			}
			rail.transform.position = pos;
			rail.transform.localScale = new Vector3(1, 1, 1);
			if (N == 1)
			{
				Character.Instance.SetItemRail(rail.GetComponent<RailManager>());
			}
			
			Character.Instance.Sprinklers.Add(rail.GetComponent<RailManager>());
		}
	}
}