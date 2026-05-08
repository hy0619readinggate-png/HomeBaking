using beyondi.Behaviour;
using System;

namespace DoDoEng.Tester
{
    public static class EventBus
    {
        // Activity Events
        public delegate void Cateogry_Select(Category category);
        public delegate void Contents_Select(IndexBase index);

        // Methods
        public static void Subscribe<T>(T handler) where T : Delegate
        {
            BYDEventBus<T>.Register(handler);
        }
        public static void Unsubscribe<T>(T handler) where T : Delegate
        {
            BYDEventBus<T>.Unregister(handler);
        }
        public static void Raise<T>(params object[] args) where T : Delegate
        {
            BYDEventBus<T>.Trigger.DynamicInvoke(args);
        }
    }
}