using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System;
#if !UNITY_WEBGL
using Firebase;
using Firebase.Crashlytics;
using Firebase.Extensions;
using Firebase.Messaging;
#endif

namespace DoDoEng
{
    public class FirebaseMGR : BYDSingleton<FirebaseMGR>
    {
        // Properties
        public string RegistrationToken => registrationToken;

        // Methods
        public void Initialize()
        {
#if !UNITY_WEBGL
            LOG.VeryImportant($"FirebaseApp.CheckAndFixDependenciesAsync Started!!", this);

            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    app = FirebaseApp.DefaultInstance;

                    LOG.Info($"FirebaseApp Name : {app.Name}", this);

                    FirebaseMessaging.TokenReceived += OnTokenReceived;
                    FirebaseMessaging.MessageReceived += OnMessageReceived;

                    Crashlytics.ReportUncaughtExceptionsAsFatal = true;
#if !UNITY_EDITOR && UNITY_ANDROID
                    FirebaseMessaging.GetTokenAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            var token = task.Result;
                            LOG.Info($"GetTokenAsync() : {token}", this);

                            this.registrationToken = token;
                        }
                        else LOG.Error($"Failed to GetTokenAsync() : {task.Exception}", this);
                    });
#endif
                }
                else LOG.Error($"Could not resolve all Firebase dependencies: {dependencyStatus}", this);
            });
#else
            LOG.Info($"FirebaseMGR.Initialize() skipped on WebGL", this);
#endif
        }

        // Actions
        public event Action OnReceived;



        // Fields
#if !UNITY_WEBGL
        private FirebaseApp app = null;
#endif
        private string registrationToken = string.Empty;

        // Event Handlers
#if !UNITY_WEBGL
        public void OnTokenReceived(object sender, TokenReceivedEventArgs e)
        {
            LOG.Info($"OnTokenReceived() | {e.Token}", this);

            this.registrationToken = e.Token;
        }
        public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            LOG.Info($"OnMessageReceived() | {e.Message}", this);

            if (e.Message == null)
                return;

            if (e.Message.Notification != null) // 포그라운드에서 수신
            {
                LOG.Info($"Notification={e.Message.Notification} | Title={e.Message.Notification.Title} | Body={e.Message.Notification.Body}", this);
                LOG.Info($"Data={e.Message.Data}", this);

                bool idMatch = false;
                if (e.Message.Data != null &&
                     e.Message.Data.Count > 0)
                {
                    e.Message.Data.TryGetValue("loginId", out var loginId);
                    LOG.Info($"loginId={loginId}", this);

                    if (loginId == null)
                        idMatch = true;
                    else if (UserData.One.Parent.HasSignedIn)
                    {
                        if (loginId == UserData.One.Parent.LoginID)
                            idMatch = true;
                    }
                    else if (UserData.One.Child.HasSignedIn && loginId == UserData.One.Child.ID)
                        idMatch = true;
                }
                else
                {
                    idMatch = true;
                }

                var title = e.Message.Notification.Title;
                var message = e.Message.Notification.Body;
                if (idMatch)
                    SystemUI.One.PushPU.ShowPopup(title, message).Forget();
            }
            else if (e.Message.Data != null &&
                     e.Message.Data.Count > 0) // 백그라운드에서 수신
            {
                e.Message.Data.TryGetValue("Title", out var title);
                e.Message.Data.TryGetValue("Body", out var message);
                e.Message.Data.TryGetValue("loginId", out var loginId);

                LOG.Info($"Data={e.Message.Data} | loginId={loginId} | Title={title} | Body={message}", this);

                bool idMatch = false;
                if (loginId == null)
                    idMatch = true;
                else if (UserData.One.Parent.HasSignedIn)
                {
                    if (loginId == UserData.One.Parent.LoginID)
                        idMatch = true;
                }
                else if (UserData.One.Child.HasSignedIn && loginId == UserData.One.Child.ID)
                    idMatch = true;
                if (idMatch && title != null && message != null)
                    SystemUI.One.PushPU.ShowPopup(title, message).Forget();
            }

            OnReceived?.Invoke();
        }
#endif



        // Unity Messages
        protected override void Awake()
        {
            base.Awake();
        }
        private void Start()
        {

        }
    }
}
