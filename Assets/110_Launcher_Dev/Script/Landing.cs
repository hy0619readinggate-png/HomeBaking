using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Udar.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class Landing : MonoBehaviour
    {
        // Functions
        private async UniTask start()
        {
            LOG.Function(this);

            //player.Play();

            if (UserData.One.SetAppInitialization)
            {
                AddressableMGR.One.ClearCache();
                UserData.One.SetAppInitialization = false;
            }

            UserData.One.LoadLanguage();

            //AddressableMGR.One.SwitchTo(HostServerType.Test, false);

            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                // 인터넷 연결이 안되었을 때
                await SystemUI.One.ShowPopupNoInternet();
            }

            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                // 데이터로 연결이 되었을 때
            }
            else
            {
                // 와이파이로 연결이 되었을 때
            }


            // 다운로드를 중단할 수 있도록 수정(테스트 씬으로 이동등의 목적으로) by veramocor
            while (!await downloadThumbnail())
            {
                var result = await SystemUI.One.MessagePU.ShowPopupYesNo("POPUP_19", "BUTTON_20", "BUTTON_21");
                if (result == SimplePopupResult.No)
                    return;
            }

            RunnerParam.ReturnScene = SceneManager.GetActiveScene().name;
            if (UserData.One.LoadLocalData() && UserData.One.Child.AutoSignIn)
            {
                API.Result result = await LMS.One.SignInChild(UserData.One.Child.ID, UserData.One.Child.Password, UserData.One.Child.AutoSignIn);
                if (result.Success)
                {
                    UserData.One.Launched = true;
#if !UNITY_EDITOR
#if UNITY_ANDROID
                    await LMS.One.SavePushTokenChild(FirebaseMGR.One.RegistrationToken, "ANDROID", "", "");
#elif UNITY_IOS
                    await LMS.One.SavePushTokenChild(FirebaseMGR.One.RegistrationToken, "IOS", "", "");
#endif
#endif
                    await LMS.One.LoadPhotoChild();
                    await LMS.One.LoadSettingsChild();
                    await LMS.One.LoadReward();
                    await LMS.One.LoadCandy();

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
                            RunnerParam.TodaysStage = UserData.One.Child.DayProgress.Value<int>("stageId");
                            RunnerParam.TodaysDay = UserData.One.Child.DayProgress.Value<int>("day");
                            RunnerParam.TodaysOrder = 1;
                            RunnerParam.TodaysCompleted = true;
                            StartCoroutine(load(todayScene.Name));
                        }
                        else
                            StartCoroutine(load(launcherScene.Name));
                    }
                    else
                        StartCoroutine(load(launcherScene.Name));
                }
                else
                {
                    StartCoroutine(load(launcherScene.Name));
                }
            }
            else
                StartCoroutine(load(launcherScene.Name));
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
        private IEnumerator load(string sceneName)
        {
            LOG.Function(this);

            // 씬 전환시 사라지지 않게 설정 by veramocor
            DontDestroyOnLoad(gameObject);

            progressSLD.value = 0;
            loadingGO.SetActive(true);

            var op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            var time = 0f;
            while (op.progress < 0.9f)
            {
                yield return null;
                time += Time.deltaTime;

                progressSLD.value = Mathf.Lerp(progressSLD.value, op.progress, time);
                if (progressSLD.value >= op.progress)
                    time = 0f;

                LOG.Info($"progressSLD.value | {progressSLD.value}", this);
            }

            // 씬 로딩 완료
            op.allowSceneActivation = true;
            yield return op;

            audioListener.enabled = false;
            var runner = FindObjectOfType<SceneRunnerBase>();
            if (runner == null)
            {
                LOG.Important($"No SceneRunnerBase instance. retry after 0.5s", this);
                yield return new WaitForSeconds(0.5f);

                runner = FindObjectOfType<SceneRunnerBase>();
                if (runner == null)
                    LOG.Important($"No SceneRunnerBase instance. SceneRunner is not running.", this);
            }

            if (runner != null)
            {
                Exception exception = null;
                yield return runner.Prepare().ToCoroutine((ex) => exception = ex);
                if (exception != null)
                {
                    LOG.Error($"{exception.Message}", this);
                    LOG.Error($"Cannot run any more because of an error!!!!", this);

                    yield return SystemUI.One.ErrorPU.ShowPopup(exception.Message).ToCoroutine();
                    yield break;
                }
            }

            // 씬 전환후 사라지도록 설정 by veramocor
            Destroy(gameObject);

            // 모듈 실행
            if (runner != null)
                runner.Run();
        }

        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SceneField launcherScene = null;
        [SerializeField] private SceneField todayScene = null;
        //[SerializeField] private VideoPlayer player = null;
        [SerializeField] private AudioListener audioListener = null;
        [SerializeField] private GameObject languageGO = null;
        [SerializeField] private Toggle korTGL = null;
        [SerializeField] private Toggle engTGL = null;
        [SerializeField] private Toggle vieTGL = null;
        [SerializeField] private Button languageBT = null;
        [SerializeField] private Slider progressSLD = null;
        [SerializeField] private TMP_Text progressTMP = null;
        [SerializeField] private GameObject loadingGO = null;
        [Header("★ Config")]
        [SerializeField] private float delay = 4f;

        // Unity Messages
        private void Awake()
        {
            LOG.Function(this);

            languageGO.SetActive(false);
            loadingGO.SetActive(false);

            languageBT.onClick.AddListener(() =>
            {
                languageGO.SetActive(false);
                if (korTGL.isOn)
                    UserData.One.SaveLanguage(LocalizationLocale.ko);
                else if (engTGL.isOn)
                    UserData.One.SaveLanguage(LocalizationLocale.en);
                else if (vieTGL.isOn)
                    UserData.One.SaveLanguage(LocalizationLocale.vi);

                start().Forget();
            });
        }
        private IEnumerator Start()
        {
            LOG.Function(this);

            yield return new WaitForSeconds(delay);
            yield return null;

#if !UNITY_STANDALONE && !UNITY_EDITOR
            // 알림 권한 요청
            var result = false;
            yield return DevicePermissionMGR.One.RequestNotificationPermission().ToCoroutine(ok => result = ok);
            LOG.Info($"Request Permission [DevicePermission.Notifications] Result : {result}", this);

            // FCM 초기화
            // FCM을 초기화하면 iOS에서 알림 권한 요청을 해버려서, 위에서 먼저 권한 요청이후, FCM을 초기화 하도록 수정함
            FirebaseMGR.One.Initialize();
#endif

            // 언어 선택
            if (UserData.One.HasLanguage())
                start().Forget();
            else languageGO.SetActive(true);
        }
        private void Update()
        {
            if (progressSLD.gameObject.activeSelf)
            {
                progressTMP.text = $"{Math.Floor(progressSLD.value * 100)}%";
            }
        }
    }
}