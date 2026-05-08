using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
// using Vuplex.WebView;

namespace DoDoEng.Launcher
{
    public class WebView : MonoBehaviour
    {
        // Methods
        public async UniTask Init(string url)
        {
            LOG.Function(this, $"| url={url}");

            initURL = url;

            var t = GetComponent<RectTransform>();
            t.anchoredPosition = new Vector2(0, t.rect.height * 2);

            gameObject.SetActive(true);
            // await canvasWebViewPrefab.WaitUntilInitialized();
            // canvasWebViewPrefab.KeyboardEnabled = false;

            //canvasWebViewPrefab.WebView.ExecuteJavaScript(@"
            //    Object.defineProperty(navigator, 'platform', {
            //        get: function() { return 'Linux armv8l'; }
            //    });
            //");
            // canvasWebViewPrefab.WebView.LoadUrl(url);
            // await canvasWebViewPrefab.WebView.WaitForNextPageLoadToFinish();
            gameObject.SetActive(false);

            t.anchoredPosition = Vector2.zero;
        }
        public async void Open()
        {
            LOG.Function(this);

            // await canvasWebViewPrefab.WaitUntilInitialized();

            // gameObject.SetActive(true);
            // canvasWebViewPrefab.WebView.SetFocused(true);
            // canvasWebViewPrefab.KeyboardEnabled = true;

            // await canvasWebViewPrefab.WebView.ExecuteJavaScript($"window.postMessage({{type: 'locale', message: '{LocalizationMGR.One.Locale}'}})");
        }
        public async void Open(string url)
        {
            LOG.Function(this);

            // await canvasWebViewPrefab.WaitUntilInitialized();
            //if (Web.CookieManager == null)
            //{
            //    LOG.Warning($"Web.CookieManager isn't supported on this platform.", this);
            //}
            //else
            //{
            //    //var succeeded = await Web.CookieManager.DeleteCookies("https://membership.gohidodo.com", "token");
            //    //Debug.Log("Delete Cookie: " + succeeded);
            //    var success = await Web.CookieManager.SetCookie(new Cookie
            //    {
            //        Domain = "membership.dev.gohidodo.com",
            //        Path = "/",
            //        Name = "token",
            //        Value = UserData.One.Parent.AccessToken,
            //        ExpirationDate = (int)DateTimeOffset.Now.ToUnixTimeSeconds() + 60 * 60 * 24 * 30,
            //        Secure = false,
            //        HttpOnly = false
            //    });
            //    LOG.Info($"SetCookie: {success}", this);
            //    var success2 = await Web.CookieManager.SetCookie(new Cookie
            //    {
            //        Domain = "membership.dev.gohidodo.com",
            //        Path = "/",
            //        Name = "rememberMe",
            //        Value = "N",
            //        ExpirationDate = (int)DateTimeOffset.Now.ToUnixTimeSeconds() + 60 * 60 * 24 * 30,
            //        Secure = false,
            //        HttpOnly = false
            //    });
            //    LOG.Info($"SetCookie: {success2}", this);
            //}
            // await canvasWebViewPrefab.WebView.ExecuteJavaScript($"window.postMessage({{type: 'signin', message: '{UserData.One.Parent.AccessToken}'}})");
            // canvasWebViewPrefab.WebView.LoadUrl(url);
            // await canvasWebViewPrefab.WebView.WaitForNextPageLoadToFinish();

            // gameObject.SetActive(true);
            // canvasWebViewPrefab.WebView.SetFocused(true);
            // canvasWebViewPrefab.KeyboardEnabled = true;

            // await canvasWebViewPrefab.WebView.ExecuteJavaScript($"window.postMessage({{type: 'locale', message: '{LocalizationMGR.One.Locale}'}})");
        }
        public async UniTask Close(bool reset = false, bool signout = false)
        {
            LOG.Info($"Close() : reset={reset}, signout={signout}", this);

//             if (canvasWebViewPrefab?.WebView != null)
//             {
//                 var t = GetComponent<RectTransform>();
//                 t.anchoredPosition = new Vector2(0, t.rect.height * 2);

//                 canvasWebViewPrefab.WebView.SetFocused(false);
//                 canvasWebViewPrefab.KeyboardEnabled = false;

//                 if (signout)
//                 {
//                     canvasWebViewPrefab.WebView.PostMessage("{\"type\": \"signout\", \"message\": \"logout\"}");

//                     if (Web.CookieManager == null)
//                     {
//                         LOG.Warning($"Web.CookieManager isn't supported on this platform.", this);
//                     }
//                     else
//                     {
//                         var succeeded = await Web.CookieManager.DeleteCookies("membership.gohidodo.com", "token");
//                         Debug.Log("Delete Cookie: " + succeeded);
//                         var success = await Web.CookieManager.SetCookie(new Cookie
//                         {
//                            Domain = "membership.gohidodo.com",
//                            Path = "/",
//                            Name = "token",
//                            Value = "",
//                            Secure = true,
//                            ExpirationDate = (int)DateTimeOffset.Now.ToUnixTimeSeconds() + 60 * 60 * 24 * 30
//                         });
//                         LOG.Info($"SetCookie: {success}", this);
//                     }

//                     await canvasWebViewPrefab.WebView.ExecuteJavaScript("window.postMessage({type: 'signout', message: 'logout'})");
//                     flush();
//                     //await UniTask.Delay(2000);
//                 }

//                 if (reset)
//                 {
// #if !UNITY_STANDALONE && !UNITY_EDITOR
//                    Web.ClearAllData();
// #endif
//                     var canGobBack = await canvasWebViewPrefab.WebView.CanGoBack();
//                     if (canGobBack)
//                        await canvasWebViewPrefab.WebView.ExecuteJavaScript("window.history.go(1-window.history.length);");
//                     LOG.Info($"URL back: {canvasWebViewPrefab.WebView.Url}", this);
//                     try
//                     {
//                         canvasWebViewPrefab.WebView.Reload();
//                         await canvasWebViewPrefab.WebView.ExecuteJavaScript("window.history.go();");
//                         LOG.Info($"URL reload: {canvasWebViewPrefab.WebView.Url}", this);
//                         canvasWebViewPrefab.WebView.LoadUrl(initURL);
//                         await canvasWebViewPrefab.WebView.WaitForNextPageLoadToFinish();
//                     }
//                     catch (Exception e)
//                     {
//                         LOG.Warning(e.Message, this);
//                     }
//                 }
//                 // LOG.Info($"URL current: {canvasWebViewPrefab.WebView.Url}", this);

//                 t.anchoredPosition = Vector2.zero;
//             }
            

            gameObject.SetActive(false);
        }
        public void ClearAllData()
        {
            // Web.ClearAllData();
        }

        // Events
        public event Action<JToken> OnSignIn;
        public event Action<JToken> OnSignUp;



        // Funcitons
        private void flush()
        {
#if UNITY_ANDROID
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaClass cookieManager = new AndroidJavaClass("android.webkit.CookieManager"))
            {
                AndroidJavaObject cookieManagerInstance = cookieManager.CallStatic<AndroidJavaObject>("getInstance");

                // 쿠키 동기화
                cookieManagerInstance.Call("flush");
            }
#endif
        }

        // Fields
        private string initURL;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button backBT = null;
        [SerializeField] private Button closeBT = null;
        // [SerializeField] private CanvasWebViewPrefab canvasWebViewPrefab = null;

        // Unity Messages
        private void Awake()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            LOG.Important($"LocalizationMGR.One.Locale: {LocalizationMGR.One.Locale}", this);
            // StandaloneWebView.SetCommandLineArguments($"--lang={LocalizationMGR.One.Locale}");
#endif
            //Web.ClearAllData();
            var userAgent = $"HiDODO/{Application.version};{SystemInfo.operatingSystem};{SystemInfo.deviceModel};hidodoapp";
            //var userAgent = $"HiDODO/{Application.version};Android OS 12 / API-31 (SP1A.210812.016/X200XXU1BVL2);samsung SM-X200;hidodoapp";
            //var userAgent = $"HiDODO/0.5.30;Android OS 11 / API-30 (RP1A.200720.012/T515NKSU8CVG4);samsung SM-T515N;hidodoapp";
            LOG.Important($"userAgent: {userAgent}", this);
            // Web.SetUserAgent(userAgent);

            // backBT.onClick.AddListener(() => {
            //     if (canvasWebViewPrefab.WebView.Url == initURL)
            //     {
            //         Close(true).Forget();
            //     }
            //     else
            //     {
            //         canvasWebViewPrefab.WebView.GoBack();
            //         canvasWebViewPrefab.WebView.SetFocused(true);
            //     }
            // });
            closeBT.onClick.AddListener(() => Close(true).Forget());
        }
        private async void Start()
        {
//             await canvasWebViewPrefab.WaitUntilInitialized();

//             canvasWebViewPrefab.WebView.UrlChanged += async (sender, eventArgs) => {
//                 LOG.Function(canvasWebViewPrefab, "| URL changed: " + eventArgs.Url);
//                 if (Web.CookieManager == null)
//                 {
//                     LOG.Warning($"Web.CookieManager isn't supported on this platform.", this);
//                 }
//                 else
//                 {
//                     var cookies = await Web.CookieManager.GetCookies("https://membership.gohidodo.com");
//                     if (cookies.Length > 0)
//                     {
//                         cookies.ForEach(c => LOG.Info("Cookie: " + c, this));
//                     }
//                     var cookies2 = await Web.CookieManager.GetCookies("https://www.google.com");
//                     if (cookies2.Length > 0)
//                     {
//                         cookies2.ForEach(c => LOG.Info("Cookie: " + c, this));
//                     }
//                 }
//                 flush();

//                 string url = eventArgs.Url;
//                 if (url.StartsWith("nidlogin://"))
//                 {
//                     LOG.Info("Naver Login: " + url, this);
//                     Application.OpenURL(url);
//                 }
//                 else if (url.StartsWith("intent:"))
//                 {
//                     LOG.Info("Kakao Login: " + url, this);

// #if UNITY_ANDROID && !UNITY_EDITOR
//         try
//         {
//                         AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
//                         AndroidJavaObject intent = intentClass.CallStatic<AndroidJavaObject>("parseUri", url, intentClass.GetStatic<int>("URI_INTENT_SCHEME"));
//                         AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//                         AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

//                         //AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.VIEW");

//                         //AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
//                         //AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse", url);

//                         //intent.Call<AndroidJavaObject>("setData", uriObject);
//                         //intent.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK

//                         LOG.Info($"action: {intent.Call<string>("getAction")}", this);

//                         currentActivity.Call("startActivity", intent);
//         }
//         catch (System.Exception e)
//         {
//             LOG.Warning("Failed to open Intent URI: " + e.Message, this);
//         }
// #else
//                     Application.OpenURL(url);
// #endif

//                 }
//             };
//             canvasWebViewPrefab.WebView.MessageEmitted += (sender, eventArgs) => {
//                 LOG.Function(canvasWebViewPrefab, $"| MessageEmitted: ${eventArgs.Value}");

//                 JObject json = JObject.Parse(eventArgs.Value);
//                 if (json["type"].ToString() == "signin_success")
//                 {
//                     LOG.Info($"signin_success, data: {json["data"].ToString(Newtonsoft.Json.Formatting.None)}", canvasWebViewPrefab);
//                     OnSignIn?.Invoke(json["data"]);
//                 } else if (json["type"].ToString() == "signup_success")
//                 {
//                     LOG.Info($"signup_success, data: {json["data"].ToString(Newtonsoft.Json.Formatting.None)}", canvasWebViewPrefab);
//                     OnSignUp?.Invoke(json["data"]);
//                 }
//             };
        }
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }
        private void OnDestroy()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            // StandaloneWebView.TerminateBrowserProcess();
#endif
        }

    }
}