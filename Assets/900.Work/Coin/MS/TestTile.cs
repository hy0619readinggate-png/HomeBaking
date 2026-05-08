using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace DoDoEng
{
    public class TestTile : MonoBehaviour
    {
        // Definitions
        // Properties
        // Methods
        // Events



        // Fields : caching
        // Fields
        private Vector3 playerPos;
        private Transform movingTR;

        // Functions
        private void moveTop()
        {
            playerPos = new Vector3(playerPos.x, playerPos.y + 2, playerPos.z);
            Vector3Int cellPosition = tilemap.WorldToCell(playerPos);

            tilemap.SetTile(cellPosition, null);
        }
        private void moveBottom()
        {
            playerPos = new Vector3(playerPos.x, playerPos.y - 2, playerPos.z);
            Vector3Int cellPosition = tilemap.WorldToCell(playerPos);

            tilemap.SetTile(cellPosition, null);
        }
        private void moveLeft()
        {
            playerPos = new Vector3(playerPos.x - 2, playerPos.y, playerPos.z);
            Vector3Int cellPosition = tilemap.WorldToCell(playerPos);

            tilemap.SetTile(cellPosition, null);
        }
        private void moveRight()
        {
            playerPos = new Vector3(playerPos.x + 2, playerPos.y, playerPos.z);
            Vector3Int cellPosition = tilemap.WorldToCell(playerPos);

            tilemap.SetTile(cellPosition, null);
        }
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Tilemap tilemap = null;
        [SerializeField] private GameObject player = null;
        [Header("★ Arrow Key")]
        [SerializeField] private Button top = null;
        [SerializeField] private Button bottom = null;
        [SerializeField] private Button left = null;
        [SerializeField] private Button right = null;

        // Unity Messages
        private void Awake()
        {
            top.onClick.AddListener(moveTop);
            bottom.onClick.AddListener(moveBottom);
            left.onClick.AddListener(moveLeft);
            right.onClick.AddListener(moveRight);

            movingTR = player.transform.GetChild(0);
        }
        private void Start()
        {

        }
        private void Update()
        {
            playerPos = player.transform.position;
        }

        // Unity Coroutine
    }
}