/*
// �÷��� �ڵ� Ȯ�ν� ���
#undef UNITY_EDITOR
#define UNITY_ANDROID
#define UNITY_IOS
//*/

using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System;
using System.Collections;
#if UNITY_ANDROID || UNITY_IOS
using Unity.Notifications;
#endif

namespace DoDoEng
{
    public enum DevicePermission { WebCam, Microphone };

    public class DevicePermissionMGR : BYDSingleton<DevicePermissionMGR>
    {
        // Methods
        public async UniTask<bool> CheckPermissionAsync(DevicePermission permission)
        {
            LOG.Function(this, $"{permission}");

            if (impl.HasPermission(permission))
                return true;

            // �н��� ���� xxx ������ �ʿ��մϴ�.
            var msgID1 = messageID_Request(permission);
            await SystemUI.One.MessagePU.ShowPopupOK(msgID1, "BUTTON_7"); // OK

            // ���� ��û
            if (await impl.RequestPermissionAsync(permission))
                return true;

            // ������ ȹ������ ���߽��ϴ�. 
            var msgID2 = messageID_Fail(permission);
            var result = await SystemUI.One.MessagePU.ShowPopupYesNo(msgID2, "BUTTON_7", "BUTTON_28"); // OK, �������� ����
            if (result == SimplePopupResult.No)
            {
                impl.GoToSettings(permission);
            }

            return false;
        }
        public async UniTask<bool> RequestNotificationPermission()
        {
            LOG.Function(this);

#if UNITY_ANDROID || UNITY_IOS
            var result = false;
            await coRequestNotificationPermission(r => result = r).ToUniTask();
            return result;
#else
            LOG.Info("Notification permission not supported on this platform.", this);
            return false;
#endif
        }





        // Fields
        private IDevicePermissionImpl impl = null;

        // Functions
        private string messageID_Request(DevicePermission permission)
        {
            switch (permission)
            {
                case DevicePermission.WebCam: return "POPUP_21";      // �н� ������ ���� ī�޶� ��� ������ �ʿ��մϴ�.
                case DevicePermission.Microphone: return "POPUP_23";  // �н� ������ ���� ����ũ ��� ������ �ʿ��մϴ�.
            }

            return null;
        }
        private string messageID_Fail(DevicePermission permission)
        {
            switch (permission)
            {
                case DevicePermission.WebCam: return "POPUP_22";      // ī�޶� ��� ������ ȹ������ ���߽��ϴ�.
                case DevicePermission.Microphone: return "POPUP_24";  // ����ũ ��� ������ ȹ������ ���߽��ϴ�.
            }

            return null;
        }




        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

#if UNITY_ANDROID || UNITY_IOS
            var args = NotificationCenterArgs.Default;
            args.AndroidChannelId = "default";
            args.AndroidChannelName = "Notifications";
            args.AndroidChannelDescription = "Main notifications";
            NotificationCenter.Initialize(args);
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
            impl = new DevicePermission_Android();
#elif !UNITY_EDITOR && UNITY_IOS
            impl = new DevicePermission_iOS();
#else
            //impl = new DevicePermission_AlwaysOK();
            impl = new DevicePermission_AlwaysFail();
#endif
        }

        // Unity Coroutine
#if UNITY_ANDROID || UNITY_IOS
        IEnumerator coRequestNotificationPermission(Action<bool> cb)
        {
            var request = NotificationCenter.RequestPermission();
            if (request.Status == NotificationsPermissionStatus.RequestPending)
                yield return request;

            cb.Invoke(request.Status == NotificationsPermissionStatus.Granted);
            yield return null;
        }
#endif
    }

    public interface IDevicePermissionImpl
    {
        bool HasPermission(DevicePermission permission);
        UniTask<bool> RequestPermissionAsync(DevicePermission permission);
        void GoToSettings(DevicePermission permission);
    }

    public class DevicePermission_AlwaysOK : IDevicePermissionImpl
    {
        public bool HasPermission(DevicePermission permission)
        {
            return true;
        }
        public async UniTask<bool> RequestPermissionAsync(DevicePermission permission)
        {
            await UniTask.Yield();
            return true;
        }
        public void GoToSettings(DevicePermission permission)
        {
            // Do nothing
        }
    }
    public class DevicePermission_AlwaysFail : IDevicePermissionImpl
    {
        public bool HasPermission(DevicePermission permission)
        {
            return false;
        }
        public async UniTask<bool> RequestPermissionAsync(DevicePermission permission)
        {
            await UniTask.Yield();
            return false;
        }
        public void GoToSettings(DevicePermission permission)
        {
            // Do nothing
        }
    }
}