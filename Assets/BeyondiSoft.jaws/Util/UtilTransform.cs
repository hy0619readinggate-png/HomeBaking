using UnityEngine;

namespace beyondi.Util
{
    public static class UtilTransform
    {
        public static Vector2 ScreenToLocal(Vector2 ptScreen, RectTransform rt, Canvas canvas)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, ptScreen,
                canvas.worldCamera,
                out var pos);

            return pos;
        }
        public static Vector2 LocalToScreen(Vector3 ptLocal, RectTransform rt, Canvas canvas)
        {
            var ptWorld = rt.TransformPoint(ptLocal);
            return Camera.main.WorldToScreenPoint(ptWorld);
        }
        public static Vector2 WorldToLocal(Vector3 ptWorld, RectTransform rt, Canvas canvas)
        {
            var screenPos = Camera.main.WorldToScreenPoint(ptWorld);
            return ScreenToLocal(screenPos, rt, canvas);
        }
    }
}
