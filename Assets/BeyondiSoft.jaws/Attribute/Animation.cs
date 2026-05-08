using System;
using System.Linq;

namespace beyondi.Util
{
    public class DefaultAnimationAttribute : Attribute
    {
        public static T GetDefault<T>() where T : struct, IConvertible
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            foreach (var value in values)
            {
                var valueEnum = value as Enum;
                var defaultAttribute = valueEnum.GetAttribute<DefaultAnimationAttribute>();
                if (defaultAttribute != null)
                    return value;
            }

            return values.First();
        }
    }

    public class AnimationAttribute : Attribute
    {
        // Properties
        public string Name { get; private set; }

        // Methods : ctor.
        public AnimationAttribute(string name)
        {
            Name = name;
        }
    }
}