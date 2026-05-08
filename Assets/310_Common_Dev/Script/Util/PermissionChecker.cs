using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using UnityEngine;


#if !UNITY_EDITOR
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
using System.Collections.Generic;
using UnityEngine.Android;
#elif UNITY_IOS
using System.Collections.Generic;
#endif
#endif


namespace DoDoEng
{
    public enum Authorization { WebCam, Microphone };
    public enum PermissionState
    {
        Unchecked, Granted, Denied, DeniedAndDontAskAgain
    }

    public class PermissionChecker : MonoBehaviour
    {
        // Properties
        public PermissionState ResultState { get; private set; }
        public bool IsCheckComplete => isCheckComplete;

        // Methods
        public async UniTask<bool> CheckMicrophonePermissionForced()
        {
            while (true)
            {
                CheckPermission(Authorization.Microphone);
                await new WaitUntil(() => IsCheckComplete);
                if (ResultState == PermissionState.Granted)
                    return true;
                else
                {
                    var result = await SystemUI.One.ShowPopupMicrophonePermission();
                    if (result)
                        continue;
                    else
                        return false;
                }
            }
        }
        public Coroutine CheckPermission(Authorization auth)
        {
            LOG.Function(this, $"{auth}");
#if !UNITY_EDITOR
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
            return StartCoroutine(coCheckPersmission(dicAndroid[auth]));
#elif UNITY_IOS
            return StartCoroutine(coCheckPersmission(dicIOS[auth]));
#else
            return StartCoroutine(coEmpty());
#endif
#else
            return StartCoroutine(coEmpty());
#endif
        }



        // Ctor.
        static PermissionChecker()
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
            dicAndroid = new Dictionary<Authorization, string>();
            dicAndroid[Authorization.WebCam] = Permission.Camera;
            dicAndroid[Authorization.Microphone] = Permission.Microphone;
#elif UNITY_IOS
            dicIOS = new Dictionary<Authorization, UserAuthorization>();
            dicIOS[Authorization.WebCam] = UserAuthorization.WebCam;
            dicIOS[Authorization.Microphone] = UserAuthorization.Microphone;
#endif
#endif
        }

        // Fields
#if !UNITY_EDITOR
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        private static Dictionary<Authorization, string> dicAndroid;
#elif UNITY_IOS
        private static Dictionary<Authorization, UserAuthorization> dicIOS;
#endif
#endif
        private bool isCheckComplete = false;

        // Unity Coroutine
#if !UNITY_EDITOR
#if UNITY_ANDROID || UNITY_STANDALONE_WIN
        IEnumerator coCheckPersmission(string permission)
        {
            using(LOG.Coroutine($"coCheckPersmission() | {permission}", this))
            {
                isCheckComplete = false;
                ResultState = PermissionState.Unchecked;

                var authorized = Permission.HasUserAuthorizedPermission(permission);
                LOG.Info($"Authorized[{permission}] : {authorized}", this);

                if (!authorized)
                {
                    // 권한 요청 응답에 따른 동작 콜백
                    var cb = new PermissionCallbacks();
                    cb.PermissionGranted += _ => ResultState = PermissionState.Granted;
                    cb.PermissionDenied += _ => ResultState = PermissionState.Denied;
                    cb.PermissionDeniedAndDontAskAgain += _ => ResultState = PermissionState.DeniedAndDontAskAgain;

                    // 권한 요청
                    Permission.RequestUserPermission(permission, cb);
                }
                else ResultState = PermissionState.Granted;

                // 대기
                yield return new WaitWhile(() => ResultState == PermissionState.Unchecked);

                isCheckComplete = true;
                yield return null;

                Debug.Log($"ResultState : {ResultState}");
            }
        }
#elif UNITY_IOS
        IEnumerator coCheckPersmission(UserAuthorization userAuth)
        {
            using(LOG.Coroutine($"coCheckPersmission() | {userAuth}", this))
            {
                isCheckComplete = false;
                ResultState = PermissionState.Unchecked;

                var authorized = Application.HasUserAuthorization(userAuth);
                LOG.Info($"Authorized[{userAuth}] : {authorized}", this);

                if (!authorized)
                {
                    // 권한 요청
                    yield return Application.RequestUserAuthorization(userAuth);

                    authorized = Application.HasUserAuthorization(userAuth);
                    LOG.Info($"Authorized[{userAuth}] : {authorized}", this);

                    ResultState = authorized ? PermissionState.Granted : PermissionState.Denied;
                }
                else ResultState = PermissionState.Granted;

                // 대기
                yield return new WaitWhile(() => ResultState == PermissionState.Unchecked);

                // 대기
                isCheckComplete = true;
                yield return null;

                Debug.Log($"ResultState : {ResultState}");
            }
        }
#endif
#endif
        IEnumerator coEmpty()
        {
            using (LOG.Coroutine("coEmpty()", this))
            {
                isCheckComplete = false;
                ResultState = PermissionState.Granted;
                yield return null;

                isCheckComplete = true;
                yield return null;
            }
        }
    }
    
}