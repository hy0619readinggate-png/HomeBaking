using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Launcher.Framework;
using DoDoEng.Launcher.UI;
using Newtonsoft.Json.Linq;
using SRDebugger;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DoDoEng.Launcher
{
    [RequireComponent(typeof(LauncherRunner))]
    [RequireComponent(typeof(LauncherTester))]
    public class Launcher : MonoBehaviour
    {
        // https://hidodo.beany.co.kr/login/
        // https://dev.beyondiedu.com/DoDoEng/test-webview/

        // Definitions
        public enum DebugStep
        {
            Start,
            HasSignedIn,
            ChildManagement
        }

        // Properties
        public static Launcher One { get; private set; } = null;

        // Methods
        public async UniTask Prepare()
        {
            LOG.Function(this);

            canvasGroup.blocksRaycasts = false;

            webView.Close().Forget();
            lobby.Activate(true);
            childrenManagement.Activate(false);

            ProductVersion.One.LoadVersion().Forget();

            // 코스, 스테이지 정보 로드
            courseTableResult = await CourseTableLoader.One.LoadTable();
            if (courseTableResult == null)
                LOG.Warning("There is no Course Table.", this);
            await UILauncher.One.CoursePU.Init(CoursePopupType.New, courseTableResult);

            // 리워드 정보 로드
            rewardTableResult = await RewardTableLoader.One.LoadTable();
            if (rewardTableResult == null)
                LOG.Warning("There is no Reward Table.", this);
            UILauncher.One.MyInfoPU.Init(rewardTableResult);
            UILauncher.One.ContinuousAttendPU.Init(rewardTableResult);

            // 액티비티, 게임 정보 로드
            await LibraryTableLoader.One.LoadActivityTable();
            await LibraryTableLoader.One.LoadGameTable();

            if (UserData.One.Launched)
            {// 런치 이후 다시 씬이 시작되었을 경우
                await UILauncher.One.LoadMails();
                if (UserData.One.Parent.HasSignedIn)
                {
                    lobby.Activate(false);
                    if (UserData.One.IsReportContents)
                    {
                        await childrenManagement.Show(UserData.One.IsReportContents);
                        UserData.One.IsReportContents = false;
                    } else
                    {
                        await childrenManagement.Show();
                    }
                }
                else
                {
                    lobby.Refresh();
                    lobby.Show(true);
                    if (UserData.One.IsReportContents)
                    {
                        await UILauncher.One.MyInfoPU.ShowPopupJump();
                        UserData.One.IsReportContents = false;
                    }
                }
            }
            await webView.Init(API.One.ParentSignInHost);
        }
        public void StartLauncher()
        {
            LOG.Function(this);

            buildOption();

            startLauncher().Forget();
        }
        public void FinishLauncher()
        {
            LOG.Function(this);

            //UILauncher.One.MyInfoPU.StopLoading();

            removeOption();
        }



        // Fields
#if !DISABLE_SRDEBUGGER
        private DynamicOptionContainer srOptionContainer;
#endif
        private CourseTableLoaderResult courseTableResult;
        private RewardTableLoaderResult rewardTableResult;
        private int debugRewardPolicyCode = 10001;
        private bool quitReady = false;

        // Functions
        private void buildOption()
        {
#if !DISABLE_SRDEBUGGER
            srOptionContainer = new DynamicOptionContainer();

            var sort = 400;

            srOptionContainer.AddOption(OptionDefinition.Create("Reward Policy Code", () => debugRewardPolicyCode, (value) => debugRewardPolicyCode = value, "Launcher", ++sort));
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Force Reward", () =>
                {
                    forceReward().Forget();
                }, "Launcher", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test Attendance", () =>
                {
                    attendanceProccess(10).Forget();
                }, "Launcher", ++sort)
            );
            srOptionContainer.AddOption(
                OptionDefinition.FromMethod("Test WebView (Jobkorea)", () =>
                {
                    webView.Open("https://m.jobkorea.co.kr/Login/Login.asp");
                }, "Launcher", ++sort)
            );

            SRDebug.Instance.AddOptionContainer(srOptionContainer);
#endif
        }
        private void removeOption()
        {
#if !DISABLE_SRDEBUGGER
            if (SRDebug.Instance != null && srOptionContainer != null)
            {
                SRDebug.Instance.RemoveOptionContainer(srOptionContainer);
                srOptionContainer = null;
            }
#endif
        }
        private async UniTask startLauncher()
        {
            if (!UserData.One.Launched)
            {// 최초 런치
                if (debugStep == DebugStep.HasSignedIn)
                {
                    await signInChild(testID, testPW);
                    lobby.Show(true);
                }
                else if (debugStep == DebugStep.ChildManagement)
                {
                    signInParent(JToken.Parse($"{{ accessToken:'{accessToken}', loginId:'smmasaru@naver.com', nickName:'jaws', name:'jaws' }}")).Forget();
                }
                else
                {
                    lobby.Show();
                    await UniTask.Delay(1500);
                    if (UserData.One.LoadLocalData() && UserData.One.Child.AutoSignIn)
                    {
                        await signInChild(UserData.One.Child.ID, UserData.One.Child.Password, UserData.One.Child.AutoSignIn);
                    }
                    else
                    {
                        await downloadThumbnail();
                        await UserData.One.CheckEvents();
                    }
                }

                UserData.One.Launched = true;
            }
            canvasGroup.blocksRaycasts = true;
        }

        private async UniTask<bool> userSignInChild(string id, string pw, bool autoSignIn = false)
        {
            canvasGroup.blocksRaycasts = false;
            var result = await signInChild(id, pw, autoSignIn);

            if (result)
            {
                lobby.AddUser(id, pw, UserData.One.Child.NickName);
            }

            canvasGroup.blocksRaycasts = true;
            return result;
        }
        private async UniTask<bool> signInChild(string id, string pw, bool autoSignIn = false)
        {
            API.Result result = await LMS.One.SignInChild(id, pw, autoSignIn);

            if (result.Success)
            {
                await signInChild();
            }
            else
            {
                if (result.Data.Value<string>("code") == "AUTH_USER_NOT_FOUND")
                    SystemUI.One.ShowPopupWrongID();
                else if (result.Data.Value<string>("code") == "BAD_CREDENTIAL")
                    SystemUI.One.ShowPopupWrongPW();
                else if (result.Data.Value<string>("code") == "AUTH_USER_WITHDRAWAL")
                    SystemUI.One.ShowPopupUnregisteredID();
                else
                    SystemUI.One.ShowPopupProblem();
            }

            return result.Success;
        }
        private async UniTask signInChild()
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
                await LMS.One.SavePushTokenChild(FirebaseMGR.One.RegistrationToken, "ANDROID", "", "");
#elif UNITY_IOS
                await LMS.One.SavePushTokenChild(FirebaseMGR.One.RegistrationToken, "IOS", "", "");
#endif
#endif
            await LMS.One.LoadPhotoChild();
            var prevLocale = LocalizationMGR.One.Locale;
            await LMS.One.LoadSettingsChild();
            if (prevLocale != LocalizationMGR.One.Locale)
                reloadSystemUI();

            await LMS.One.LoadReward();
            await LMS.One.LoadCandy();

            if (debugStep != DebugStep.HasSignedIn)
            {
                if (await LMS.One.CheckAvaliable())
                {
                    await LMS.One.LoadDayProgress();
                    var courseLearningHistory = UserData.One.Child.DayProgress.Value<JArray>("courseLearningHistory");
                    bool allComplete = true;
                    foreach (var courseData in courseLearningHistory)
                    {
                        var courseId = courseData.Value<int>("courseId");
                        var courseComplete = courseData.Value<bool>("isComplete");
                        LOG.Info($"Course {courseId} Complete : {courseComplete}", this);
                        if (!courseComplete) allComplete = false;
                    }
                    if (!allComplete)
                    {
                        showTodayStudy().Forget();
                        return;
                    }
                }
                if (prevLocale != LocalizationMGR.One.Locale)
                {
                    restartScene();
                    return;
                }
            }

            await UILauncher.One.LoadMails();

            //var point = await LMS.One.DoAttendance();

            //if (UserData.One.Child.IsAttendanceFirst)
            //    await attendanceProccess(point);

            lobby.Show(true);
            childrenManagement.Activate(false);
        }
        private void signOutChild()
        {
            LOG.Function(this);

            LMS.One.SignOutChild().Forget();

            UserData.One.Child.AccessToken = "";
            UserData.One.Child.AutoSignIn = false;

            UserData.One.SaveLocalData();
        }
        private void signOutParent()
        {
            LMS.One.StopParentCheckAuthStatus();
            webView.Close(true, true).Forget();
            UserData.One.Parent.AccessToken = "";
            UserData.One.Child.AccessToken = "";
            lobby.Show(true);
            childrenManagement.Activate(false);
        }
        private async UniTask signInParentFromChild()
        {
            LOG.Function(this);

            canvasGroup.blocksRaycasts = false;
            var data = await LMS.One.SignInParentFromChild();
            if (data != null)
            {
                LMS.One.SignOutChild().Forget();

                UserData.One.Child.AccessToken = "";
                UserData.One.Child.AutoSignIn = false;

                UserData.One.SaveLocalData();
                await signInParent(data);
            }
            else
                await SystemUI.One.ErrorPU.ShowPopup("Failed to sign in parent.");
            canvasGroup.blocksRaycasts = true;
        }
        private async UniTask signInChildFromParent(string childId)
        {
            LOG.Function(this, $"| childId={childId}");

            canvasGroup.blocksRaycasts = false;
            var result = await LMS.One.SignInChildFromParent(childId);
            if (result.Success)
            {
                LMS.One.StopParentCheckAuthStatus();
                webView.Close(true, true).Forget();
                UserData.One.Parent.AccessToken = "";
                UserData.One.Child.AccessToken = result.Data.Value<string>("accessToken");
                await signInChild();
            }
            else
                await SystemUI.One.ErrorPU.ShowPopup("Failed to sign in child.");
            canvasGroup.blocksRaycasts = true;
        }
        private async UniTask attendanceProccess(int point)
        {
            await SystemUI.One.CalendarPU.ShowPopup(true);
            await UILauncher.One.ContinuousAttendPU.ShowPopup(UserData.One.Child.AttendanceContinuous, point);
        }
        private async UniTask signInParent(JToken data)
        {
            UserData.One.Parent.AccessToken = data.Value<string>("accessToken");
            UserData.One.Parent.Name = data.Value<string>("name");
            UserData.One.Parent.NickName = data.Value<string>("nickName");

            LOG.LMS($"signInParent() data: {data.ToString(Newtonsoft.Json.Formatting.None)}", this);

#if !UNITY_EDITOR
#if UNITY_ANDROID
            await LMS.One.SavePushTokenParent(FirebaseMGR.One.RegistrationToken, "ANDROID", "", "");
#elif UNITY_IOS
            await LMS.One.SavePushTokenParent(FirebaseMGR.One.RegistrationToken, "IOS", "", "");
#endif
#endif
            var prevLocale = LocalizationMGR.One.Locale;
            await LMS.One.LoadSettingsParent();
            await LMS.One.LoadParentProfile();
            await LMS.One.LoadFamily();
            await LMS.One.LoadUserTicket();

            LMS.One.StartParentCheckAuthStatus().Forget();

            if (prevLocale != LocalizationMGR.One.Locale)
            {
                reloadSystemUI();
                restartScene();
            }
            else
            {
                await UILauncher.One.LoadMails();
                webView.Close().Forget();
                lobby.Refresh(true);
                lobby.Activate(false);
                childrenManagement.Show().Forget();
            }
        }
        private async UniTask loadScene(string sceneName)
        {
            try
            {
                canvasGroup.blocksRaycasts = false;

                RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
                SceneLoader.One.LoadScene(sceneName);
                FinishLauncher();
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.Message}", this);
                LOG.Error($"Cannot run any more because of an error!!!!", this);

                await SystemUI.One.ErrorPU.ShowPopup(ex.Message);
            }
        }
        private async UniTask unregister()
        {
            await webView.Close(true, true);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken,
                $"/api/v1/user/mypage/withdrawal",
                API.Method.Put,
                JObject.Parse($"{{withdrawalReasonCode: 'APP', withdrawalReasonContent: '앱에서의 탈퇴 처리'}}"));

            if (result.Success)
            {
                SystemUI.One.ShowPopupUnregisteredParent().Forget();
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("회원 탈퇴에 문제가 발생하였습니다.").Forget();
            }

            lobby.Show(true);
            childrenManagement.Activate(false);
            UserData.One.Parent.AccessToken = "";
        }
        private async UniTask showTodayStudyInProgress()
        {
            await LMS.One.LoadDayProgress();
            await showTodayStudy();
        }
        private async UniTask showTodayStudy()
        {
            if (await downloadThumbnail())
            {
                RunnerParam.TodaysStage = UserData.One.Child.DayProgress.Value<int>("stageId");
                RunnerParam.TodaysDay = UserData.One.Child.DayProgress.Value<int>("day");
                RunnerParam.TodaysOrder = 0; // 0은 현재 진도로 자동 이동
                RunnerParam.TodaysCompleted = true;
                loadScene("TodaysStudy").Forget();
            }
        }
        private async UniTask showMovie()
        {
            if (UserData.One.Child.HasSignedIn)
            {
                if (await downloadThumbnail())
                {
                    RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
                    loadScene("LibraryMovie").Forget();
                }
            }
            else
            {
                await SystemUI.One.ShowPopupNoSignIn();
                lobby.ShowPopSignIn();
            }
        }
        private async UniTask showEBook()
        {
            if (UserData.One.Child.HasSignedIn)
            {
                if (await downloadThumbnail())
                {
                    RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
                    loadScene("LibraryEBook").Forget();
                }
            }
            else
            {
                await SystemUI.One.ShowPopupNoSignIn();
                lobby.ShowPopSignIn();
            }
        }
        private async UniTask showActivity()
        {
            if (UserData.One.Child.HasSignedIn)
            {
                if (await downloadThumbnail())
                {
                    RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
                    loadScene("LibraryActivity").Forget();
                }
            }
            else
            {
                await SystemUI.One.ShowPopupNoSignIn();
                lobby.ShowPopSignIn();
            }
        }
        private async UniTask showPlayground()
        {
            await LMS.One.LoadDayProgress();
            RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
            loadScene("Playground").Forget();
        }
        private async UniTask<bool> downloadThumbnail()
        {
            try
            {
                var addressList = new List<string>();
                addressList.Add("Thumbnail/DataDefinitionSO.asset");
                addressList.Add("AIStudio/DataDefinitionSO.asset");
                var size = await DataDownloader.One.GetDataDownloadSize(addressList);
                if (size > 0)
                {
                    var result = await SystemUI.One.DownloadConfirmPU.ShowPopup(size);
                    if (result != SimplePopupResult.Yes)
                        return false;

                    await DataDownloader.One.DownloadData(addressList, SystemUI.One.DownloadPU);
                }
                return true;
            }
            catch (Exception ex)
            {
                LOG.Error($"{ex.Message}", this);

                await SystemUI.One.ErrorPU.ShowPopup(ex.Message);
            }

            return false;
        }
        private async UniTask forceReward()
        {
            int point = await LMS.One.SaveReward(debugRewardPolicyCode);
            await LMS.One.LoadReward();
            LOG.Function(this, $"| point={point}");
        }
        private async UniTask askQuit()
        {
            quitReady = true;
            var wantQuit = await SystemUI.One.ShowQuit();
            if (wantQuit)
                Application.Quit();
            else
                quitReady = false;
        }
        private async UniTask goUpdate(bool isParent)
        {
            if (isParent || await UILauncher.One.ParentsOnlyPU.ShowPopup() == SimplePopupResult.Yes)
            {
                #if UNITY_ANDROID
                    string packageName = Application.identifier; // 현재 앱의 패키지명을 가져옴
                    string url = "https://play.google.com/store/apps/details?id=" + packageName;
                    Application.OpenURL(url);
                #elif UNITY_IOS
                    string appId = "6467715927"; // App Store에서 제공되는 앱의 ID
                    string url = "https://apps.apple.com/app/id" + appId;
                    Application.OpenURL(url);
                #endif
            }
        }
        private void reloadSystemUI()
        {
            DestroyImmediate(SystemUI.One.gameObject);
            var pb = Resources.Load<GameObject>("Singleton/SystemUI");
            var go = Instantiate(pb);
            DontDestroyOnLoad(go);
        }
        private void restartScene()
        {
            RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
            SceneLoader.One.LoadScene(SceneManager.GetActiveScene().name);
            FinishLauncher();
        }

        // Event Handlers
        private void webView_OnSignIn(JToken data)
        {
            LOG.Info($"webView_OnSignIn()", this);

            signInParent(data).Forget();
        }
        private void webView_OnSignUp(JToken data)
        {
            LOG.Info($"webView_OnSignUp()", this);

            signInParent(data).Forget();
        }
        private void lobby_OnSignInChild(string id, string pw, bool autoSignIn)
        {
            LOG.Function(this);

            userSignInChild(id, pw, autoSignIn).Forget();
        }
        private void Lobby_OnSignOut()
        {
            signOutChild();
        }
        private void lobby_OnParents()
        {
            LOG.Function(this);

            webView.Open();
        }
        private void lobby_onClickMenu(Lobby.Menu menu, LobbyMenuButton button)
        {
            LOG.Function(this, $"menu={menu} | button={button}");

            lobby_onClickMenu_async(menu, button).Forget();
        }
        private async UniTask lobby_onClickMenu_async(Lobby.Menu menu, LobbyMenuButton button)
        {
            if (UserData.One.Child.HasSignedIn)
            {
                canvasGroup.blocksRaycasts = false;
                if (await LMS.One.CheckAvaliable())
                {
                    button.PlayAnimation(() =>
                    {
                        if (menu == Lobby.Menu.Today)
                        {
                            showTodayStudyInProgress().Forget();
                        }
                        else if (menu == Lobby.Menu.Activity)
                        {
                            showActivity().Forget();
                        }
                        else if (menu == Lobby.Menu.Movie)
                        {
                            showMovie().Forget();
                        }
                        else if (menu == Lobby.Menu.Book)
                        {
                            showEBook().Forget();
                        }
                        else if (menu == Lobby.Menu.Game)
                        {
                            showPlayground().Forget();
                        }
                        else if (menu == Lobby.Menu.Studio)
                        {
                            RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
                            loadScene("AiStudio").Forget();
                        }
                        else if (menu == Lobby.Menu.MyPet)
                        {
                            RunnerParam.LaunchedFrom = RunnerParam.LaunchType.Others;
                            loadScene("MyRoom").Forget();
                        }
                    }).Forget();
                }
                else
                {
                    canvasGroup.blocksRaycasts = true;
                    SystemUI.One.ShowPopupNoAvailable().Forget();
                }
            }
            else
            {
                await SystemUI.One.ShowPopupNoSignIn();
                lobby.ShowPopSignIn();
            }
        }
        private void lobby_OnClickLogo()
        {
            LOG.Function(this);

            Application.OpenURL(infoURL);
        }
        private void lobby_onUpdate()
        {
            goUpdate(false).Forget();
        }
        private void settings_onAlarm()
        {
#if UNITY_ANDROID
        // Android 알림 설정으로 이동
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    int sdkVersion = new AndroidJavaClass("android.os.Build$VERSION").GetStatic<int>("SDK_INT");
                    AndroidJavaObject intent;

                    if (sdkVersion >= 26) // Android 8.0 (Oreo) 이상
                    {
                        intent = new AndroidJavaObject("android.content.Intent", "android.settings.APP_NOTIFICATION_SETTINGS");
                        intent.Call<AndroidJavaObject>("putExtra", "android.provider.extra.APP_PACKAGE", Application.identifier);
                    }
                    else if (sdkVersion >= 21) // Android 5.0 (Lollipop) 이상
                    {
                        intent = new AndroidJavaObject("android.content.Intent", "android.settings.APP_NOTIFICATION_SETTINGS");
                        intent.Call<AndroidJavaObject>("putExtra", "app_package", Application.identifier);
                        int appUid = currentActivity.Call<AndroidJavaObject>("getApplicationInfo").Get<int>("uid");
                        intent.Call<AndroidJavaObject>("putExtra", "app_uid", appUid);
                    }
                    else
                    {
                        // Android 5.0 미만에서는 일반 설정 화면으로 이동
                        intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS");
                        AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", "package:" + Application.identifier);
                        intent.Call<AndroidJavaObject>("setData", uri);
                    }

                    currentActivity.Call("startActivity", intent);
                }
            }
        }
        catch (Exception ex)
        {
            LOG.Error(ex.Message, this);
        }
#elif UNITY_IOS
        Application.OpenURL("app-settings:");
#endif
        }
        private void lobby_onRestart()
        {
            reloadSystemUI();
            restartScene();
        }
        private void lobby_onInit()
        {
            UserData.One.Child.AccessToken = "";
            UserData.One.Child.AutoSignIn = false;

            UserData.One.SaveLocalData();

            RunnerParam.ReturnScene = null;
            UserData.One.SetAppInitialization = true;
            webView.ClearAllData();
            SceneLoader.One.LoadScene("A10_Landing");
            FinishLauncher();
        }
        private void childrenManagement_OnInfoEdit()
        {
            LOG.Function(this);

            webView.Open(API.One.ParentInfoHost);
        }
        private void childrenManagement_OnSignOut()
        {
            LOG.Function(this);

            signOutParent();
        }
        private void childrenManagement_OnUnregister()
        {
            LOG.Info($"childrenManagement_OnUnregister()", this);

            unregister().Forget();
        }
        private void childrenManagement_onRestart()
        {
            reloadSystemUI();
            restartScene();
        }
        private void childrenManagement_onInit()
        {
            UserData.One.Parent.AccessToken = "";
            UserData.One.Child.AccessToken = "";

            RunnerParam.ReturnScene = null;
            UserData.One.SetAppInitialization = true;
            webView.ClearAllData();
            SceneLoader.One.LoadScene("A10_Landing");
            FinishLauncher();
        }
        private void childrenManagement_onUpdate()
        {
            goUpdate(true).Forget();
        }
        private void childrenManagement_OnSignInChild(string childId)
        {
            signInChildFromParent(childId).Forget();
        }
        private void UILauncher_OnSignInParent()
        {
            signInParentFromChild().Forget();
        }
        private void firebaseMGR_OnReceived()
        {
            UILauncher.One.LoadMails().Forget();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private CanvasGroup canvasGroup = null;
        [SerializeField] private WebView webView = null;
        [SerializeField] private Lobby lobby = null;
        [SerializeField] private ChildrenManagement childrenManagement = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip bgmCLIP = null;
        [Header("★ Config")]
        [SerializeField] private string infoURL = null;
        [SerializeField] private DebugStep debugStep = DebugStep.Start;
        [SerializeField] private string testID = null;
        [SerializeField] private string testPW = null;
        [SerializeField] private string accessToken = null;

        // Unity Messages
        protected void Awake()
        {
            UserData.One.LoadLanguage();
        }
        protected void Start()
        {
            AudioMGR.One.PlayBGM(bgmCLIP);

            if (FirebaseMGR.One)
                FirebaseMGR.One.OnReceived += firebaseMGR_OnReceived;
        }
        protected void OnEnable()
        {
            webView.OnSignIn += webView_OnSignIn;
            webView.OnSignUp += webView_OnSignUp;

            lobby.OnSignInChild += lobby_OnSignInChild;
            lobby.OnSignOut += Lobby_OnSignOut;
            lobby.OnParents += lobby_OnParents;
            lobby.OnClickMenu += lobby_onClickMenu;
            lobby.OnUpdate += lobby_onUpdate;
            lobby.OnAlarm += settings_onAlarm;
            lobby.OnRestart += lobby_onRestart;
            lobby.OnInit += lobby_onInit;
            lobby.OnClickLogo += lobby_OnClickLogo;

            childrenManagement.OnUpdate += childrenManagement_onUpdate;
            childrenManagement.OnInfoEdit += childrenManagement_OnInfoEdit;
            childrenManagement.OnSignOut += childrenManagement_OnSignOut;
            childrenManagement.OnUnregister += childrenManagement_OnUnregister;
            childrenManagement.OnRestart += childrenManagement_onRestart;
            childrenManagement.OnInit += childrenManagement_onInit;
            childrenManagement.OnAlarm += settings_onAlarm;
            childrenManagement.OnSignInChild += childrenManagement_OnSignInChild;

            UILauncher.One.OnSignInParent += UILauncher_OnSignInParent;
        }
        protected void OnDisable()
        {
            webView.OnSignIn -= webView_OnSignIn;
            webView.OnSignUp -= webView_OnSignUp;

            lobby.OnSignInChild -= lobby_OnSignInChild;
            lobby.OnSignOut -= Lobby_OnSignOut;
            lobby.OnParents -= lobby_OnParents;
            lobby.OnClickMenu -= lobby_onClickMenu;
            lobby.OnUpdate -= lobby_onUpdate;
            lobby.OnAlarm -= settings_onAlarm;
            lobby.OnRestart -= lobby_onRestart;
            lobby.OnInit -= lobby_onInit;
            lobby.OnClickLogo -= lobby_OnClickLogo;

            childrenManagement.OnUpdate -= childrenManagement_onUpdate;
            childrenManagement.OnInfoEdit -= childrenManagement_OnInfoEdit;
            childrenManagement.OnSignOut -= childrenManagement_OnSignOut;
            childrenManagement.OnUnregister -= childrenManagement_OnUnregister;
            childrenManagement.OnRestart -= childrenManagement_onRestart;
            childrenManagement.OnInit -= childrenManagement_onInit;
            childrenManagement.OnAlarm -= settings_onAlarm;
            childrenManagement.OnSignInChild -= childrenManagement_OnSignInChild;

            if (UILauncher.One)
                UILauncher.One.OnSignInParent -= UILauncher_OnSignInParent;
        }
        protected void Update()
        {
            if (!quitReady && Input.GetKeyDown(KeyCode.Escape))
            {
                askQuit().Forget();
            }
        }
        protected void OnDestroy()
        {
            if (FirebaseMGR.One)
                FirebaseMGR.One.OnReceived -= firebaseMGR_OnReceived;
        }
    }
}