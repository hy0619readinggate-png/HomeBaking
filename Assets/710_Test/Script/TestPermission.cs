using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using UnityEngine.UI;

namespace DoDoEng.Tester
{
    public class TestPermission : MonoBehaviour
    {
        // Event Handlers
        private void hasMicPermissionBTN_OnClick()
        {
            LOG.Function(this);

#if UNITY_ANDROID
            var permission = Permission.Microphone;
            var authorized = Permission.HasUserAuthorizedPermission(permission);

            LOG.Info($"Authorized[{permission}] : {authorized}", this);
#elif UNITY_IOS
            var permission = UserAuthorization.Microphone;
            var authorized = Application.HasUserAuthorization(permission);

            LOG.Info($"Authorized[{permission}] : {authorized}", this);
#endif
        }
        private void requestMicPermissionBTN_OnClick()
        {
            LOG.Function(this);
#if UNITY_ANDROID
            var permission = Permission.Microphone;

            var cb = new PermissionCallbacks();
            cb.PermissionGranted += str => LOG.Info($"PermissionGranted() | {str}", this);
            cb.PermissionDenied += str => LOG.Info($"PermissionDenied() | {str}", this);
            cb.PermissionDeniedAndDontAskAgain += str => LOG.Info($"PermissionDeniedAndDontAskAgain() | {str}", this);

            // 권한 요청
            Permission.RequestUserPermission(permission, cb);
#elif UNITY_IOS
            var permission = UserAuthorization.Microphone;
            Application.RequestUserAuthorization(permission);
#endif

        }
        private void hasWebCamPermissionBTN_OnClick()
        {
            LOG.Function(this);

#if UNITY_ANDROID
            var permission = Permission.Camera;
            var authorized = Permission.HasUserAuthorizedPermission(permission);

            LOG.Info($"Authorized[{permission}] : {authorized}", this);
#elif UNITY_IOS
            var permission = UserAuthorization.WebCam;
            var authorized = Application.HasUserAuthorization(permission);

            LOG.Info($"Authorized[{permission}] : {authorized}", this);
#endif
        }
        private void requestWebCamPermissionBTN_OnClick()
        {
            LOG.Function(this);

#if UNITY_ANDROID
            var permission = Permission.Camera;

            var cb = new PermissionCallbacks();
            cb.PermissionGranted += str => LOG.Info($"PermissionGranted() | {str}", this);
            cb.PermissionDenied += str => LOG.Info($"PermissionDenied() | {str}", this);
            cb.PermissionDeniedAndDontAskAgain += str => LOG.Info($"PermissionDeniedAndDontAskAgain() | {str}", this);

            // 권한 요청
            Permission.RequestUserPermission(permission, cb);
#elif UNITY_IOS
            var permission = UserAuthorization.WebCam;
            Application.RequestUserAuthorization(permission);
#endif
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button hasMicPermissionBTN = null;
        [SerializeField] private Button requestMicPermissionBTN = null;
        [SerializeField] private Button mgrMicPermissionBTN = null;
        [SerializeField] private Button hasWebCamPermissionBTN = null;
        [SerializeField] private Button requestWebCamPermissionBTN = null;
        [SerializeField] private Button mgrWebCamPermissionBTN = null;

        // Unity Messages
        private void Awake()
        {
            hasMicPermissionBTN.onClick.AddListener(hasMicPermissionBTN_OnClick);
            requestMicPermissionBTN.onClick.AddListener(requestMicPermissionBTN_OnClick);
            hasWebCamPermissionBTN.onClick.AddListener(hasWebCamPermissionBTN_OnClick);
            requestWebCamPermissionBTN.onClick.AddListener(requestWebCamPermissionBTN_OnClick);

            mgrMicPermissionBTN.onClick.AddListener(() => StartCoroutine(coRequestMic()));
            mgrWebCamPermissionBTN.onClick.AddListener(() => StartCoroutine(coRequestCam()));
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coRequestMic()
        {
            using (LOG.Coroutine($"coRequestMic()", this))
            {
                var result = false;
                yield return DevicePermissionMGR.One.CheckPermissionAsync(DevicePermission.Microphone).ToCoroutine(ok => result = ok);

                LOG.Info($"Result : {result}", this);
            }
        }
        IEnumerator coRequestCam()
        {
            using (LOG.Coroutine($"coRequestCam()", this))
            {
                var result = false;
                yield return DevicePermissionMGR.One.CheckPermissionAsync(DevicePermission.WebCam).ToCoroutine(ok => result = ok);

                LOG.Info($"Result : {result}", this);
            }
        }
    }
}