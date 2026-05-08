using System;

namespace beyondi.Behaviour
{
    // https://forum.unity.com/threads/recommended-event-system.856294/
    public static class BYDEventBus<T> where T : Delegate
    {
        public static void Register(T callback) => Trigger = Delegate.Combine(Trigger, callback) as T;
        public static void Unregister(T callback) => Trigger = Delegate.Remove(Trigger, callback) as T;
        public static T Trigger { get; private set; }
    }
}