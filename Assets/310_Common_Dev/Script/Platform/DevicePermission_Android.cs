/*
// ÇÃ·§Æû ÄÚµå È®ÀÎ½Ã »ç¿ë
#undef UNITY_EDITOR
#define UNITY_ANDROID
//*/

#if !UNITY_EDITOR && UNITY_ANDROID

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

namespace DoDoEng
{
    public class DevicePermission_Android : IDevicePermissionImpl
    {
        // Methods
        public bool HasPermission(DevicePermission devicePermission)
        {
            var permission = dicAndroid[devicePermission];

            return Permission.HasUserAuthorizedPermission(permission);
        }
        public async UniTask<bool> RequestPermissionAsync(DevicePermission devicePermission)
        {
            var permission = dicAndroid[devicePermission];

            if (Permission.HasUserAuthorizedPermission(permission))
                return true;

            var result = (bool?)null;
            var cb = new PermissionCallbacks();
            cb.PermissionGranted += _ => result = true;
            cb.PermissionDenied += _ => result = false;
            cb.PermissionDeniedAndDontAskAgain += _ => result = false;
            Permission.RequestUserPermission(permission, cb);

            await UniTask.WaitWhile(() => result == null);

            return result.Value;
        }
        public void GoToSettings(DevicePermission permission)
        {
            try
            {
                using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    string packageName = currentActivityObject.Call<string>("getPackageName");

                    using (var uriClass = new AndroidJavaClass("android.net.Uri"))
                    using (AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null))
                    using (var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject))
                    {
                        intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                        intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                        currentActivityObject.Call("startActivity", intentObject);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }



        // Methods : ctor.
        public DevicePermission_Android()
        {
            dicAndroid = new Dictionary<DevicePermission, string>();
            dicAndroid[DevicePermission.WebCam] = Permission.Camera;
            dicAndroid[DevicePermission.Microphone] = Permission.Microphone;
        }


        // Fields
        private static Dictionary<DevicePermission, string> dicAndroid;
    }
}

#endif