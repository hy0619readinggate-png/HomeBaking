using beyondi.Behaviour;
using System;

namespace DoDoEng.Game.C2_G02
{
    public static class EventBus
    {
        // Game Events
        public delegate void MonsterDiedEvent(Monster monster);
        public delegate void MonsterDefendEvent(Monster monster);
        public delegate void PlantBeAttackedEvent(int lane);
        public delegate void PlantDiedEvent();
        public delegate void CannonBeAttackedEvent();



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