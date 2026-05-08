using System;
using UnityEngine;

namespace DoDoEng.Common
{
    public class SystemManager
    {
        // Methods
        public static void Pause()
        {
            LOG.Info<SystemManager>($"Pause()");

            Time.timeScale = 0;
            AudioListener.pause = true;
            OnPause?.Invoke();
        }
        public static void Resume()
        {
            LOG.Info<SystemManager>($"Resume()");

            Time.timeScale = 1f;
            AudioListener.pause = false;

            OnResume?.Invoke();
        }

        // Events
        public static event Action OnPause;
        public static event Action OnResume;
    }
}