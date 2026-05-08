using System.Collections;
using UnityEngine;

namespace DoDoEng.Game.C4_G01
{
	public class Sprinkler : MonoBehaviour
	{
		Rigidbody2D body;
		[SerializeField] GameObject[] objs;
		RectTransform rectTransform;
		GameObject obj;
		void Awake()
		{
			body = GetComponent<Rigidbody2D>();
			body.isKinematic = true;
		}
		void Start()
		{
			GameObject itemGroup = GameObject.Find("item-group");
			transform.SetParent(itemGroup.transform);
			foreach (GameObject obj in objs) obj.SetActive(false);
			int n = Random.Range(0, objs.Length);
			obj = objs[n];
			rectTransform = obj.GetComponent<RectTransform>();
			obj.SetActive(true);
		}

		bool bHit = false;
		public bool Hit(Character character)
		{
			if (bHit) return false;
			bHit = true;

			obj.GetComponent<Animator>().enabled = false;
			if (character.isBooster || character.IsOverSpeed)
			{
				body.isKinematic = false;
				body.AddForce(new Vector2(2000, 1000));
			}

			gameObject.GetComponent<BoxCollider2D>().enabled = false;
			StartCoroutine(IEHit());
			return true;
		}

		IEnumerator IEHit()
		{
			//obj.transform.Find("water").gameObject.SetActive(false);
			obj.SetActive(false);
			yield return new WaitForSeconds(0.1f);
			obj.SetActive(true);
			yield return new WaitForSeconds(0.2f);
			obj.SetActive(false);
			yield return new WaitForSeconds(0.1f);
			obj.SetActive(true);
			yield return new WaitForSeconds(0.2f);
			obj.SetActive(false);
			yield return new WaitForSeconds(0.1f);
			obj.SetActive(true);
			yield return new WaitForSeconds(0.2f);
			obj.SetActive(false);
			gameObject.SetActive(false);
		}

		void Update()
		{
			if (Camera.main.WorldToViewportPoint(transform.position).x < -1)
			{
				Destroy(gameObject);
				return;
			}
			if (!bHit) return;
			if (rectTransform.sizeDelta.x > 0)
			{
				rectTransform.sizeDelta -= rectTransform.sizeDelta * Time.deltaTime * 3.0f;
				if (rectTransform.sizeDelta.x < 0) rectTransform.sizeDelta = Vector2.zero;
			}
		}
	}
}