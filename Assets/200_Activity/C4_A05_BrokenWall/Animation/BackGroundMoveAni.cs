using UnityEngine;

namespace DoDoEng.Activity.C4_A05
{
    public class BackGroundMoveAni : MonoBehaviour
    {
        // Event Handler
        public void MoveX()
        {

            bg.transform.position = new Vector3(bg.transform.position.x - moveX, bg.transform.position.y, bg.transform.position.z);
        }

        // Unity Inspectors
        [SerializeField] private float moveX = 1f;
        [SerializeField] private GameObject bg = null;


    }
}