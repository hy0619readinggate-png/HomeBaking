using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C2_G03
{
	public class Item : MonoBehaviour
	{
		[SerializeField] GameObject effect;
		[SerializeField] GameObject body;

		RectTransform rectTransform;
		void Awake()
		{
			rectTransform = body.GetComponent<RectTransform>();
			effect.SetActive(false);
		}

		void Start()
		{
			GameObject itemGroup = GameObject.Find("item-group");
			transform.SetParent(itemGroup.transform);
		}

		bool bHit = false;
		public void Hit(Character character)
		{
			if (bHit) return;
			bHit = true;
			gameObject.GetComponent<CircleCollider2D>().enabled = false;
			rectTransform.sizeDelta *= 2;
			rectTransform.GetComponent<CanvasRenderer>().SetAlpha(0.25f);
			StartCoroutine(IEHit());
			switch (gameObject.tag)
			{
				case "ItemStar":
					character.PlayEffect("item1");
					C2_G03_Main.Instance.UpdateHP();
					break;
				case "ItemBooster":
					character.PlayEffect("item2");
					C2_G03_Main.Instance.UpdateHP();
					break;
			}
		}

		IEnumerator IEHit()
		{
			effect.SetActive(true);
			yield return new WaitForSeconds(1);
			gameObject.SetActive(false);
		}

		void Update()
		{
			if (Camera.main.WorldToViewportPoint(transform.position).x < -1)
			{
				Destroy(gameObject);
				return;
			}
			if (!effect.activeSelf) return;
			if (rectTransform.sizeDelta.x > 0)
			{
				rectTransform.sizeDelta -= rectTransform.sizeDelta * Time.deltaTime * 3.0f;
				if (rectTransform.sizeDelta.x < 0) rectTransform.sizeDelta = Vector2.zero;
			}
		}
	}
}