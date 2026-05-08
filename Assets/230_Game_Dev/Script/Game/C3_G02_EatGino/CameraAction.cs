using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Game.C3_G02
{
    public class CameraAction : MonoBehaviour
    {
        // Fields
        private float offsetX = 0f;
        private float offsetY = 0f;



        // Functions
        private bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject player = null;

        // Unity Messages
        private void Start()
        {
            offsetX = transform.position.x - player.transform.position.x;
            offsetY = transform.position.y - player.transform.position.y;
        }
        void LateUpdate()
        {
            var x = Mathf.Abs(transform.position.x - player.transform.position.x);
            var y = Mathf.Abs(transform.position.y - player.transform.position.y);

            if (x > offsetX || y > offsetY)
            {
                transform.position = new Vector3(
                    player.transform.position.x - offsetX,
                    player.transform.position.y - offsetY,
                    transform.position.z);
            }
        }
        void FixedUpdate()
        {

            if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
            {
                Vector3 mousePoint = Input.mousePosition;
                mousePoint = GetComponent<Camera>().ScreenToWorldPoint(mousePoint);

                RaycastHit2D hit = Physics2D.Raycast(mousePoint, transform.forward, 15f);
                if (hit.collider != null)
                {
                    if (hit.collider.GetComponentInParent<JellyObject>() != null)
                        hit.collider.GetComponentInParent<JellyObject>().PlaySound();
                }
            }
        }
    }
}