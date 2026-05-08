using UnityEngine;

namespace DoDoEng.Common
{
    [CreateAssetMenu(fileName = "SystemCursorConfigSO", menuName = "DoDoEng/ConfigSO/SystemCursorConfigSO", order = 0)]
    public class SystemCursorConfigSO : ScriptableObject
    {
        // Properties
        public bool EnableSystemCursor => enableSystemCursor;
        public Texture2D NormalCursorTexture => normalCursorTEX;
        public Texture2D PressCursorTexture => pressCursorTEX;
        public Vector2 HotSpot => hotSpot;




        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private bool enableSystemCursor = false;
        [SerializeField] private Texture2D normalCursorTEX;
        [SerializeField] private Texture2D pressCursorTEX;
        [SerializeField] private Vector2 hotSpot = Vector2.zero;
    }
} 