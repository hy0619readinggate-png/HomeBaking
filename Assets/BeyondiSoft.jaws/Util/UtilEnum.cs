using System;
using System.Linq;

namespace beyondi.Util
{
    public class UtilEnum
    {
        public static T[] GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }
        public static string[] GetValues(Type t)
        {
            return
                Enum.GetValues(t)
                    .OfType<object>()
                    .Select(o => o.ToString())
                    .ToArray();
        }

        public static T FromString<T>(string str) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), str);
        }
    }
}
