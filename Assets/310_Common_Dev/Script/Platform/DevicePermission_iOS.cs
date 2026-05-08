/*
// ÇÃ·§Æû ÄÚµå È®ÀÎ½Ã »ç¿ë
#undef UNITY_EDITOR
#define UNITY_IOS
//*/

#if !UNITY_EDITOR && UNITY_IOS

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng
{
    public class DevicePermission_iOS : IDevicePermissionImpl
    {
        // Methods
        public bool HasPermission(DevicePermission permission)
        {
            var userAuth = dicIOS[permission];
            return Application.HasUserAuthorization(userAuth);
        }
        public async UniTask<bool> RequestPermissionAsync(DevicePermission permission)
        {
            var userAuth = dicIOS[permission];

            await Application.RequestUserAuthorization(userAuth);
            return Application.HasUserAuthorization(userAuth);
        }
        public void GoToSettings(DevicePermission permission)
        {
            // https://stackoverflow.com/questions/30010334/open-settings-application-on-unity-ios
            MyNativeBindings.OpenSettings();
        }



        // Fields
        private static Dictionary<DevicePermission, UserAuthorization> dicIOS;

        // static ctor.
        static DevicePermission_iOS()
        {
            dicIOS = new Dictionary<DevicePermission, UserAuthorization>();
            dicIOS[DevicePermission.WebCam] = UserAuthorization.WebCam;
            dicIOS[DevicePermission.Microphone] = UserAuthorization.Microphone;
        }
    } 
}

#endif