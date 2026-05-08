using UnityEngine;

namespace DoDoEng.Game.C4_G01
{
	public class DoDo : MonoBehaviour
	{
		[SerializeField] Character character;
		void OnTriggerEnter2D(Collider2D other)
		{
			if (character.CurAni == CharacterAnimation.Fall) return;
			switch (other.gameObject.tag)
			{
				case "ItemStar":
					other.gameObject.GetComponent<Item>().Hit(character);
					break;
				case "ItemBooster":
					other.gameObject.GetComponent<Item>().Hit(character);
					character.Boost();
					break;
			}
		}
	}
}