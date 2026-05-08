using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DoDoEng.Common
{
    public static class LOG
    {
        private const string PREFIX = "DoDoEng";

        // Methods
        // http://msdn.microsoft.com/en-us/library/system.diagnostics.conditionalattribute.aspx
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void Info<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()}({goName}) | {message}");
            }
            else UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()} | {message}");
        }
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void Warning<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.LogWarning($"<color=yellow>{PREFIX} | {nameOf<T>()}({goName}) | {message}</color>");
            }
            else UnityEngine.Debug.LogWarning($"<color=yellow>{PREFIX} | {nameOf<T>()} | {message}</color>");
        }
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void Error<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.LogError($"<b><color=red>{PREFIX} | {nameOf<T>()}({goName}) | {message}</color></b>");
            }
            else UnityEngine.Debug.LogError($"<b><color=red>{PREFIX} | {nameOf<T>()} | {message}</color></b>");
        }
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void Important<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()}({goName}) | <b><color=#FF8888>{message}</color></b>");
            }
            else UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()} | <b><color=#FF8888>{message}</color></b>");
        }
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void VeryImportant<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()}({goName}) | <b><color=#FFAAAA>{message}</color></b>");
            }
            else UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()} | <b><color=#FFAAAA>{message}</color></b>");
        }
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void Debug<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()}({goName}) | <b><color=cyan>{message}</color></b>");
            }
            else UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()} | <b><color=cyan>{message}</color></b>");
        }
        public static void Assert<T>(bool condition, string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Assert(condition, $"{PREFIX} | {nameOf<T>()}({goName}) | <b><color=red>{message}</color></b>");
            }
            else UnityEngine.Debug.Assert(condition, $"{PREFIX} | {nameOf<T>()} | <b><color=red>{message}</color></b>");
        }

        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void Audio<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()}({goName}) | <b><color=cyan>{message}</color></b>");
            }
            else UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()} | <b><color=cyan>{message}</color></b>");
        }
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void Addressable<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()}({goName}) | <b><color=#FFAAAA>{message}</color></b>");
            }
            else UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()} | <b><color=#FFAAAA>{message}</color></b>");
        }
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void LMS<T>(string message, T instance = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()}({goName}) | <b><color=#AAFFAA>{message}</color></b>");
            }
            else UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()} | <b><color=#AAFFAA>{message}</color></b>");
        }

        // Methods
        [System.Diagnostics.Conditional("DODOENG_LOG_ENABLE")]
        public static void Function<T>(
            T instance,
            string message = null,
            [CallerMemberName] string method = null) where T : class
        {
            var mono = instance as MonoBehaviour;
            if (mono != null)
            {
                var goName = mono.gameObject.name;
                UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()}({goName}) | {method}() {message}");
            }
            else UnityEngine.Debug.Log($"{PREFIX} | {nameOf<T>()} | {method}() {message}");
        }


        // Functions
        private static string nameOf<T>()
        {
            var t = typeof(T);
            return t.Name.Split('`')[0];
        }



        // For Coroutine
        public static Logger<T> Coroutine<T>(string message, T instance) where T : class
        {
            return new Logger<T>(message, instance);
        }
        public class Logger<T> : IDisposable where T : class
        {
            // Methods
            internal Logger(string message, T instance)
            {
                this.instance = instance;
                this.message = message;
                this.time = Time.time;
                LOG.Info($"<b><color=#FFFFAA>Coroutine START</color></b> | {message}", instance);
            }
            public void Dispose()
            {
                var duration = Time.time - time;
                LOG.Info($"<b><color=#FFFFAA>Coroutine FINISH</color></b> | {message} duration:<color=#FFFFAA><b>{duration:F3}s</b></color>", instance);
            }


            // Fields
            private float time;
            private string message;
            private T instance;
        }
    }
}