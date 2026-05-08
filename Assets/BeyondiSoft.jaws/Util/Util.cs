using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace beyondi.Util
{
    public static class Util
    {
        public static Quaternion CharacterLookAt(Transform origin, Transform target)
        {
            var dir = target.position - origin.position;
            dir.y = 0;

            return Quaternion.LookRotation(dir);
        }
        public static Dictionary<string, string> ParseQuery(string url)
        {
            var dict = new Dictionary<string, string>();

            var uri = new Uri(url);
            var query = uri.Query.TrimStart('?').Split('&');

            foreach (var pair in query)
            {
                if (string.IsNullOrEmpty(pair)) continue;

                var kv = pair.Split('=');
                if (kv.Length == 2)
                    dict[kv[0]] = Uri.UnescapeDataString(kv[1]);
            }

            return dict;
        }

        public static bool IsPointerOverUI(string layerName)
        {
            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = Input.mousePosition;

            var raysastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raysastResults);

            var layer = LayerMask.NameToLayer(layerName);
            foreach (RaycastResult raysastResult in raysastResults)
            {
                if (raysastResult.gameObject.layer == layer)
                    return true;
            }

            return false;
        }
        public static void RemoveAllChildren(Transform parent)
        {
            foreach (Transform t in parent)
                GameObject.Destroy(t.gameObject);
        }
        public static void SetActiveAllChildren(Transform parent, bool active)
        {
            foreach (Transform t in parent)
                t.gameObject.SetActive(active);
        }
        public static GameObject[] GetAllChildren(Transform parent)
        {
            var layerList = new List<GameObject>();
            foreach (Transform t in parent)
                layerList.Add(t.gameObject);
            return layerList.ToArray();
        }
        public static Dictionary<TKey, TValue> GetDictionaryOfChildren<TKey, TValue>(GameObject baseGO, Func<TValue, TKey> keyFunc)
        {
            var dict = new Dictionary<TKey, TValue>();
            var children = baseGO.GetComponentsInChildren<TValue>(true);
            foreach (var child in children)
            {
                var key = keyFunc.Invoke(child);
                dict[key] = child;
            }

            return dict;
        }


        // Screen Capture
        public static Sprite CaptureOld(RectTransform rt)
        {
            var bounds = new Vector3[4];
            rt.GetWorldCorners(bounds);

            var leftBottom = Camera.main.WorldToScreenPoint(bounds[0]);
            var rightTop = Camera.main.WorldToScreenPoint(bounds[2]);

            var width = rightTop.x - leftBottom.x;
            var height = rightTop.y - leftBottom.y;
            var startX = leftBottom.x;
            var startY = leftBottom.y;

            var texture2D = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
            texture2D.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
            texture2D.Apply();

            var rect = new Rect(0, 0, texture2D.width, texture2D.height);
            return Sprite.Create(texture2D, rect, Vector2.one / 2, 100);
        }
        public static Sprite CaptureCanvas(Canvas canvas)
        {
            var rt = canvas.GetComponent<RectTransform>();
            var bounds = new Vector3[4];
            rt.GetWorldCorners(bounds);

            var leftBottom = Camera.main.WorldToScreenPoint(bounds[0]);
            var rightTop = Camera.main.WorldToScreenPoint(bounds[2]);

            var width = rightTop.x - leftBottom.x;
            var height = rightTop.y - leftBottom.y;
            var startX = leftBottom.x;
            var startY = leftBottom.y;

            var texture2D = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
            var renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            canvas.worldCamera.targetTexture = renderTexture;
            canvas.worldCamera.Render();
            canvas.worldCamera.targetTexture = null;

            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;

            GameObject.Destroy(renderTexture);

            var rect = new Rect(0, 0, texture2D.width, texture2D.height);
            return Sprite.Create(texture2D, rect, Vector2.one / 2, 100);
        }
    }
}
