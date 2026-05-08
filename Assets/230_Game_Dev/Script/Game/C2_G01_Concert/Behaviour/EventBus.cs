using beyondi.Behaviour;
using System;

namespace DoDoEng.Game.C2_G01
{
    public static class EventBus
    {
        // Activity Events
        public delegate void NoteHit(Note note);
        public delegate void NoteCorrect(Note note);
        public delegate void NoteWrong(Note note);
        public delegate void NoteFloor(Note note);

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