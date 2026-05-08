using UnityEngine;

namespace beyondi.Util
{
    public static class UtilRandom
    {
        public static bool RandomSuccess(float prob)
        {
            return Random.Range(0f, 1f) < prob;
        }

        public static Vector3 RandomPositionIn(RectTransform rt)
        {
            var bounds = new Vector3[4];
            rt.GetWorldCorners(bounds);

            return new Vector3(
                Random.Range(bounds[0].x, bounds[3].x),
                Random.Range(bounds[0].y, bounds[1].y), 0);
        }
        public static Quaternion RandomRotation(bool random = true, float min = 0, float max = 360)
        {
            return random
                ? Quaternion.Euler(0, 0, Random.Range(min, max))
                : Quaternion.identity;
        }
        public static Vector3 RandomScale(bool randomScale, float scaleMin, float scaleMax)
        {
            return randomScale
                ? Vector3.one * Random.Range(scaleMin, scaleMax)
                : Vector3.one;
        }

        public static T RandomEnum<T>()
        {
            var values = System.Enum.GetValues(typeof(T));
            return (T)values.GetValue(Random.Range(0, values.Length));
        }
    }
}