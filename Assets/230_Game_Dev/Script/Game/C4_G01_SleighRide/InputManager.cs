using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C4_G01
{
	public class InputManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		[SerializeField] Character character;
		bool bTouch = false;
		void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		{
			if (bTouch) return; bTouch = true;
			character.Jump();
		}
		void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
		{
			bTouch = false;
		}
	}
}