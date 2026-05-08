using System;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Common
{
    public class SystemCursor : MonoBehaviour
    {
        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private SystemCursorConfigSO configSO;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
            if (configSO.EnableSystemCursor)
            {
                StartCoroutine(coStartCursor());
                StartCoroutine(coUpdatePress());
            }
        }
        private void Update()
        {

        }

        // Unity Coroutine
        IEnumerator coStartCursor()
        {
            yield return new WaitForEndOfFrame();

            var tex = configSO.NormalCursorTexture;
            var hotspot = configSO.HotSpot;
            Cursor.SetCursor(tex, hotspot, CursorMode.ForceSoftware);
        }
        IEnumerator coUpdatePress()
        {
            var hotspot = configSO.HotSpot;

            while (true)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var tex = configSO.PressCursorTexture;
                    Cursor.SetCursor(tex, hotspot, CursorMode.ForceSoftware);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    var tex = configSO.NormalCursorTexture;
                    Cursor.SetCursor(tex, hotspot, CursorMode.ForceSoftware);
                }

                yield return null;
            }
        }
    }
}