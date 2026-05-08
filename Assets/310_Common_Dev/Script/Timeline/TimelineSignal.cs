using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace DoDoEng.Common
{
    public class TimelineSignal : MonoBehaviour, INotificationReceiver
    {
        // Events
        public event Action<string> OnSignal;



        // Interface : INotificationReceiver
        void INotificationReceiver.OnNotify(Playable origin, INotification notification, object context)
        {
            var signal = notification as SignalEmitter;
            if (signal != null && signal.asset != null)
            {
                OnSignal?.Invoke(signal.asset.name);
            }
        }
    }
}