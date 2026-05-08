using beyondi.Behaviour;
using System;

namespace DoDoEng.Activity.C3_A04
{
    public static class EventBus
    {
        // Activity Events
        public delegate void SpaceShipReady(int seq);
        public delegate void SpaceShipCorrect(int seq);

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