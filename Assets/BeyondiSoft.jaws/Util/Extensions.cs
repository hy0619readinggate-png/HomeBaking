using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityObject = UnityEngine.Object;

namespace beyondi.Util
{
    public static class Extensions
    {
        // GameObject
        public static void SetLayer(this GameObject parent, int layer, bool includeChildren = true)
        {
            parent.layer = layer;
            if (includeChildren)
            {
                foreach (Transform tr in parent.transform.GetComponentsInChildren<Transform>(true))
                    tr.gameObject.layer = layer;
            }
        }
        public static void SetActiveAll(this Component[] components, bool active)
        {
            components.ForEach((idx, com) => com.gameObject.SetActive(active));
        }
        public static void SetActiveOnly(this Component[] components, int activeIdx)
        {
            components.ForEach((idx, com) => com.gameObject.SetActive(activeIdx == idx));
        }
        public static void SetActiveAll(this GameObject[] gameObjects, bool active)
        {
            gameObjects.ForEach((idx, go) => go.SetActive(active));
        }
        public static void SetActiveOnly(this GameObject[] gameObjects, int activeIdx)
        {
            gameObjects.ForEach((idx, go) => go.SetActive(activeIdx == idx));
        }
        public static T SetActiveOnly<T>(this T[] components, int activeIdx) where T : Component
        {
            components.ForEach((idx, com) => com.gameObject.SetActive(activeIdx == idx));
            return components.SingleOrDefault(c => c.gameObject.activeSelf);
        }
        public static void SetChildActiveOnly<T>(this T component, int childIndex) where T : Component
        {
            for (var i = 0; i < component.transform.childCount; i++)
                component.transform.GetChild(i).gameObject.SetActive(i == childIndex);
        }

        // Object
        public static bool IsDestroyed(this UnityObject target)
        {
            return !ReferenceEquals(target, null) && target == null;
        }
        public static bool IsUnityNull(this object obj)
        {
            return obj == null || ((obj is UnityObject) && ((UnityObject)obj) == null);
        }

        // Transforms
        public static IEnumerable<Transform> GetChildren(this Transform t)
        {
            var i = 0;
            while (i < t.childCount)
            {
                yield return t.GetChild(i);
                ++i;
            }
        }
        public static IEnumerable<GameObject> GetChildrenAsGameObject(this Transform t)
        {
            return t.GetChildren().Select(x => x.gameObject);
        }
        public static void SetActiveAllChildren(this Transform parent, bool active)
        {
            var children = parent.GetChildren();
            foreach (var ch in children)
                ch.gameObject.SetActive(active);
        }
        public static void RemoveAllChildren(this Transform parent, bool immediate = false)
        {
            if (immediate)
            {
                var children = parent.GetChildren();
                foreach (var ch in children)
                    GameObject.DestroyImmediate(ch.gameObject);
            }
            else
            {
                foreach (Transform t in parent)
                    GameObject.Destroy(t.gameObject);
            }
        }
        public static void SetLocalＸ(this Transform t, float x)
        {
            var pos = t.localPosition;
            t.localPosition = new Vector3(x, pos.y, pos.z);
        }
        public static void SetLocalY(this Transform t, float y)
        {
            var pos = t.localPosition;
            t.localPosition = new Vector3(pos.x, y, pos.z);
        }
        public static void SetLocalZ(this Transform t, float z)
        {
            var pos = t.localPosition;
            t.localPosition = new Vector3(pos.x, pos.y, z);
        }
        public static void SetXYOnly(this Transform t, Vector3 position)
        {
            t.position = new Vector3(position.x, position.y, t.position.z);
        }

        // Layout
        public static void RebuildLayoutImmediate(this Component component)
        {
            var rt = component.transform as RectTransform;
            if (rt != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }
        public static void MarkLayoutForRebuild(this Component component)
        {
            var rt = component.transform as RectTransform;
            if (rt != null)
                LayoutRebuilder.MarkLayoutForRebuild(rt);
        }
        public static void RebuildLayout(this MonoBehaviour m)
        {
            m.StartCoroutine(RebuildLayout(m.transform as RectTransform));
        }
        private static IEnumerator RebuildLayout(RectTransform rt)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        }

        // Coroutine
        public static void StopCoroutineSafe(this MonoBehaviour m, ref UnityEngine.Coroutine routine)
        {
            if (routine != null)
                m.StopCoroutine(routine);
            routine = null;
        }

        // Texture
        public static Texture2D ToTexture2D(this RenderTexture texRT)
        {
            var tex = new Texture2D(texRT.width, texRT.height, TextureFormat.RGBA32, false);
            var oldRT = RenderTexture.active;
            RenderTexture.active = texRT;

            tex.ReadPixels(new Rect(0, 0, texRT.width, texRT.height), 0, 0);
            tex.Apply();

            RenderTexture.active = oldRT;
            return tex;
        }
        public static Texture2D Crop(this Texture2D tex, int x, int y, int width, int height)
        {
            var pixels = tex.GetPixels(x, y, width, height);
            var newTex = new Texture2D(width, height, tex.format, false);
            newTex.SetPixels(pixels);
            newTex.Apply();
            return newTex;
        }
        public static Texture2D Rotate90(this Texture2D tex)
        {
            Color32[] origpix = tex.GetPixels32(0);
            Color32[] newpix = new Color32[tex.width * tex.height];
            for (int c = 0; c < tex.height; c++)
            {
                for (int r = 0; r < tex.width; r++)
                {
                    newpix[tex.width * tex.height - (tex.height * r + tex.height) + c] =
                      origpix[tex.width * tex.height - (tex.width * c + tex.width) + r];
                }
            }
            Texture2D newtex = new Texture2D(tex.height, tex.width, tex.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
        public static Texture2D Rotate180(this Texture2D tex)
        {
            Color32[] origpix = tex.GetPixels32(0);
            Color32[] newpix = new Color32[tex.width * tex.height];
            for (int i = 0; i < origpix.Length; i++)
            {
                newpix[origpix.Length - i - 1] = origpix[i];
            }
            Texture2D newtex = new Texture2D(tex.width, tex.height, tex.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
        public static Texture2D Rotate270(this Texture2D tex)
        {
            Color32[] origpix = tex.GetPixels32(0);
            Color32[] newpix = new Color32[tex.width * tex.height];
            int i = 0;
            for (int c = 0; c < tex.height; c++)
            {
                for (int r = 0; r < tex.width; r++)
                {
                    newpix[tex.width * tex.height - (tex.height * r + tex.height) + c] = origpix[i];
                    i++;
                }
            }
            Texture2D newtex = new Texture2D(tex.height, tex.width, tex.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
        public static Texture2D FlipVertical(this Texture2D tex)
        {
            Color32[] origpix = tex.GetPixels32(0);
            Color32[] newpix = new Color32[tex.width * tex.height];

            for (int c = 0; c < tex.height; c++)
            {
                for (int i = 0; i < tex.width; i++)
                {
                    newpix[(tex.width * (tex.height - 1 - c)) + i] = origpix[(c * tex.width) + i];
                }
            }

            Texture2D newtex = new Texture2D(tex.width, tex.height, tex.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }
        public static Texture2D FlipHorizontal(this Texture2D tex)
        {
            Color32[] origpix = tex.GetPixels32(0);
            Color32[] newpix = new Color32[tex.width * tex.height];

            for (int c = 0; c < tex.height; c++)
            {
                for (int i = 0; i < tex.width; i++)
                {
                    newpix[(c * tex.width) + (tex.width - 1 - i)] = origpix[(c * tex.width) + i];
                }
            }

            Texture2D newtex = new Texture2D(tex.width, tex.height, tex.format, false);
            newtex.SetPixels32(newpix, 0);
            newtex.Apply();
            return newtex;
        }

        // Enum
        public static TAttribute GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);

            return enumType.GetField(name)
                .GetCustomAttributes(false)
                .OfType<TAttribute>()
                .SingleOrDefault();
        }
        public static string GetInspectorName(this Enum enumValue)
        {
            return enumValue.GetType()?
                            .GetMember(enumValue.ToString())?
                            .First()?
                            .GetCustomAttribute<InspectorNameAttribute>()?
                            .displayName;
        }



        // Array
        public static void ForEach<T>(this T[] array, Action<T> action)
        {
            Array.ForEach<T>(array, action);
        }
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            sequence.ToArray().ForEach(action);
        }
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<int, T> action)
        {
            var i = 0;
            foreach (T item in sequence)
            {
                action(i, item);
                i++;
            }
        }
        public static bool Contain<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value) != -1;
        }
        public static int FindIndex<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value);
        }
        public static bool Exists<T>(this T[] array, Predicate<T> match)
        {
            return Array.Exists(array, match);
        }

        // Position
        public static Vector3 CanvasPosition(this GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            var world = rt.parent.TransformPoint(rt.localPosition);
            return rt.root.InverseTransformPoint(world);
        }
        public static Vector3 CanvasPosition(this MonoBehaviour m)
        {
            var rt = m.GetComponent<RectTransform>();
            var world = rt.parent.TransformPoint(rt.localPosition);
            return rt.root.InverseTransformPoint(world);
        }

        // Particle System
        public static void PlayAtPosition(this ParticleSystem ps, Vector3 screenPos)
        {
            var rt = ps.GetComponent<RectTransform>();
            var canvas = ps.GetComponentInParent<Canvas>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt.parent as RectTransform, screenPos,
                canvas.worldCamera,
                out var pos);

            rt.localPosition = pos;
            ps.gameObject.SetActive(false);
            ps.gameObject.SetActive(true);
        }
        public static void PlayAtMousePosition(this ParticleSystem ps)
        {
            PlayAtPosition(ps, Input.mousePosition);
        }
        public static void SetStopAction(this ParticleSystem ps, ParticleSystemStopAction stopAction)
        {
            var main = ps.main;
            main.stopAction = stopAction;
        }

        // Reflection
        public static bool IsOverride(this Type type, string methodName)
        {
            return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).IsOverride();
        }
        public static bool IsOverride(this MethodInfo m)
        {
            return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        // DateTime
        public static DateTime FirstDayOfWeek(this DateTime dateTime, DayOfWeek dayOfWeek = DayOfWeek.Monday)
        {
            return dateTime.AddDays(-((dateTime.DayOfWeek + 7 - dayOfWeek) % 7));
        }
        public static DateTime LastDayOfWeek(this DateTime dateTime, DayOfWeek dayOfWeek = DayOfWeek.Monday)
        {
            return dateTime.FirstDayOfWeek(dayOfWeek).AddDays(6);
        }
        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            return dateTime.AddDays(1 - dateTime.Day);
        }
    }
}
