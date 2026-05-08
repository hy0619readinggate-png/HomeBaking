using beyondi.Behaviour;
using beyondi.Util;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace DoDoEng.Common
{
    public class SystemUI : BYDSingleton<SystemUI>
    {
        // Properties
        public SimpleFader Fader => fader;
        public DownloadConfirmPopup DownloadConfirmPU => downloadConfirmPU;
        public DownloadPopup DownloadPU => downloadPU;
        public ErrorPopup ErrorPU => errorPU;
        public MessagePopup MessagePU => messagePU;
        public StandbyPopup StandbyPU => standbyPU;
        public PushPopup PushPU => pushPU;
        public CoinHistoryPopup CoinHistoryPU => coinHistoryPU;
        public CalendarPopup CalendarPU => calendarPU;
        public EventPopup EventPU => eventPU;

        // Properteis
        public bool DeveloperModeMarker
        {
            get => developerModeGO.activeSelf;
            set => developerModeGO.SetActive(value);
        }
        public bool TestServerModeMarker
        {
            get => testServerModeGO.activeSelf;
            set => testServerModeGO.SetActive(value);
        }


        // Methods
        public void ShowToastMessage(string msg)
        {
            LOG.Info($"ShowMessage() | {msg}", this);

            toastMessage.Show(msg);
        }

        // Methods : Popups
        public async UniTask ShowPopupNoSignIn()
        {
            await MessagePU.ShowPopupOK("POPUP_6");
        }
        public void ShowPopupNoService()
        {
            MessagePU.ShowPopupOK("POPUP_13").Forget();
        }
        public void ShowPopupNoInputID()
        {
            MessagePU.ShowPopupOK("POPUP_14").Forget();
        }
        public void ShowPopupWrongID()
        {
            MessagePU.ShowPopupOK("POPUP_2").Forget();
        }
        public void ShowPopupWrongPW()
        {
            MessagePU.ShowPopupOK("POPUP_4").Forget();
        }
        public void ShowPopupUnregisteredID()
        {
            MessagePU.ShowPopupOK("POPUP_15").Forget();
        }
        public void ShowPopupProblem()
        {
            MessagePU.ShowPopupOK("POPUP_16").Forget();
        }
        public async UniTask<bool> ShowPopupDeleteChild()
        {
            return await MessagePU.ShowPopupYesNo("POPUP_8", "BUTTON_6") == SimplePopupResult.Yes;
        }
        public async UniTask<bool> ShowPopupUnregister()
        {
            return await MessagePU.ShowPopupYesNo("POPUP_17", "BUTTON_18") == SimplePopupResult.Yes;
        }
        public void ShowPopupEBookNoPlayable()
        {
            MessagePU.ShowPopupOK("EBOOK_3", "BUTTON_3", true).Forget();
        }
        public async UniTask<bool> ShowPopupEBookSave()
        {
            return await MessagePU.ShowPopupYesNo("EBOOK_5", "BUTTON_19", "BUTTON_12") == SimplePopupResult.Yes;
        }
        public async UniTask<bool> ShowPopupMyRoomTrip1()
        {
            return await MessagePU.ShowPopupYesNo("MYROOM_3", "BUTTON_20", "BUTTON_21") == SimplePopupResult.Yes;
        }
        public async UniTask<bool> ShowPopupMyRoomTrip2()
        {
            return await MessagePU.ShowPopupYesNo("MYROOM_2", "BUTTON_20", "BUTTON_21", true) == SimplePopupResult.Yes;
        }
        public async UniTask ShowPopupCameraDisconnected()
        {
            await MessagePU.ShowPopupOK("POPUP_18", "BUTTON_7", true, false);
        }
        public void ShowPopupPetLimit()
        {
            MessagePU.ShowPopupOK("MYROOM_4").Forget();
        }
        public async UniTask<bool> ShowPopupDeleteMovie()
        {
            return await MessagePU.ShowPopupYesNo("MOVIE_4", "BUTTON_6", "BUTTON_5") == SimplePopupResult.Yes;
        }
        public async UniTask<bool> ShowPopupDeleteEBook()
        {
            return await MessagePU.ShowPopupYesNo("EBOOK_4", "BUTTON_6", "BUTTON_5") == SimplePopupResult.Yes;
        }
        public void ShowPopupMyPetNeedCoin()
        {
            MessagePU.ShowPopupOK("MYROOM_5").Forget();
        }
        public async UniTask<bool> ShowPopupChangeLanguage()
        {
            return await MessagePU.ShowPopupYesNo("MESSAGE_73") == SimplePopupResult.Yes;
        }
        public async UniTask ShowPopupNoInternet()
        {
            await MessagePU.ShowPopupOK("MESSAGE_75");
        }
        public async UniTask ShowPopupNoAvailable()
        {
            await MessagePU.ShowPopupOK("POPUP_7");
        }
        public async UniTask<bool> ShowPopupInit()
        {
            return await MessagePU.ShowPopupYesNo("MESSAGE_42") == SimplePopupResult.Yes;
        }
        public async UniTask ShowPopupCannotStamp()
        {
            await MessagePU.ShowPopupOK("POPUP_25");
        }
        public async UniTask<bool> ShowQuit()
        {
            return await MessagePU.ShowPopupYesNo("POPUP_26", "BUTTON_1", "BUTTON_31", true) == SimplePopupResult.Yes;
        }
        public async UniTask<bool> ShowPopupDayComplete(bool isLast)
        {
            if (isLast)
                return await MessagePU.ShowPopupYesNo("POPUP_28", "BUTTON_1", "BUTTON_35", true) == SimplePopupResult.Yes;
            else
                return await MessagePU.ShowPopupYesNo("POPUP_27", "BUTTON_20", "BUTTON_21", true) == SimplePopupResult.Yes;
        }
        public async UniTask ShowPopupCannotPlayground()
        {
            await MessagePU.ShowPopupOK("GAME_2", "BUTTON_3", true);
        }
        public async UniTask ShowPopupEdutemAccessDenided()
        {
            await MessagePU.ShowPopupOK("POPUP_35", "BUTTON_7", true);
        }
        public async UniTask ShowPopupEdutemTimeout()
        {
            await MessagePU.ShowPopupOK("POPUP_36", "BUTTON_7", true);
        }
        public async UniTask ShowPopupEdutemWrongInput()
        {
            await MessagePU.ShowPopupOK("POPUP_37", "BUTTON_7", true);
        }
        public async UniTask ShowPopupEdutemError()
        {
            await MessagePU.ShowPopupOK("POPUP_38", "BUTTON_7", true);
        }
        public async UniTask ShowPopupNoRecordFiles()
        {
            await MessagePU.ShowPopupOK("POPUP_20");
        }
        // 중복 로그인
        public async UniTask ShowPopupDuplicatedSignIn()
        {
            await MessagePU.ShowPopupOK("POPUP_40", "BUTTON_3", true);
        }
        // 로비 계정 선택 화면에서 슬롯 삭제 여부 확인 팝업
        public async UniTask<bool> ShowPopupRemoveAccount()
        {
            return await MessagePU.ShowPopupYesNo("POPUP_29", "BUTTON_20", "BUTTON_21", true) == SimplePopupResult.Yes;
        }
        // 자녀 생성 시 안내 팝업
        public async UniTask ShowPopupAddUserGuide()
        {
            await MessagePU.ShowPopupOK("MESSAGE_77", "BUTTON_7", true);
        }
        // 탈퇴 완료 안내 팝업
        public async UniTask ShowPopupUnregisteredParent()
        {
            await MessagePU.ShowPopupOK("POPUP_33", "BUTTON_7", true);
        }
        // 자녀 생성 시 입력 오류 팝업
        public async UniTask ShowPopupAddUserWrongInput()
        {
            await MessagePU.ShowPopupOK("POPUP_41", "BUTTON_3", true);
        }
        // 마이크 권한 허용 요청 팝업
        public async UniTask<bool> ShowPopupMicrophonePermission()
        {
            var result = await MessagePU.ShowPopupYesNo("POPUP_34", "BUTTON_34", "BUTTON_33", true) == SimplePopupResult.Yes;
            if (result)
            {
#if !UNITY_EDITOR
#if UNITY_ANDROID
                try
                {
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                        {
                            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS");
                            AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", "package:" + Application.identifier);
                            intent.Call<AndroidJavaObject>("setData", uri);

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
                await UniTask.WaitForSeconds(1.0f);
#else
                return false;
#endif
            }
            return result;
        }
        // 마이크 권한 허용 요청 팝업
        public async UniTask<bool> ShowPopupFilePermission()
        {
            var result = await MessagePU.ShowPopupYesNo("POPUP_42", "BUTTON_34", "BUTTON_22", true) == SimplePopupResult.Yes;
            if (result)
            {
#if !UNITY_EDITOR
#if UNITY_ANDROID
                try
                {
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                        {
                            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS");
                            AndroidJavaObject uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("parse", "package:" + Application.identifier);
                            intent.Call<AndroidJavaObject>("setData", uri);

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
                await UniTask.WaitForSeconds(1.0f);
#else
                return false;
#endif
            }
            return result;
        }
        // 사용할 수 없는 펫 이름
        public async UniTask ShowPopupNoValidatedPetName()
        {
            await MessagePU.ShowPopupOK("MYROOM_24", "BUTTON_7", true);
        }
        // 우편 메시지 삭제
        public async UniTask<bool> ShowPopupRemoveMail()
        {
            return await MessagePU.ShowPopupYesNo("POPUP_31", "BUTTON_20", "BUTTON_21", true) == SimplePopupResult.Yes;
        }
        // 전체 우편 메시지 삭제
        public async UniTask<bool> ShowPopupRemoveAllMails()
        {
            return await MessagePU.ShowPopupYesNo("POPUP_32", "BUTTON_20", "BUTTON_21", true) == SimplePopupResult.Yes;
        }
        // 가족 삭제
        public async UniTask<bool> ShowPopupRemoveFamily()
        {
            return await MessagePU.ShowPopupYesNo("MESSAGE_90", "BUTTON_20", "BUTTON_21", true) == SimplePopupResult.Yes;
        }
        // 이미 등록되어있는 가족 등록 불가
        public void ShowPopupCannotAddFamily()
        {
            MessagePU.ShowPopupOK("MESSAGE_91", "BUTTON_7", true).Forget();
        }
        // 가족 등록 오류 팝업
        public void ShowErrorAddFamily()
        {
            ErrorPU.ShowPopup(LocalizationMGR.One.GetText("ERROR_5")).Forget();
        }
        // 가족 삭제 오류 팝업
        public void ShowErrorRemoveFamily()
        {
            ErrorPU.ShowPopup(LocalizationMGR.One.GetText("ERROR_4")).Forget();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SimpleFader fader;
        [SerializeField] private GameObject developerModeGO;
        [SerializeField] private GameObject testServerModeGO;
        [Header("★ Bindings - popup")]
        [SerializeField] private DownloadConfirmPopup downloadConfirmPU;
        [SerializeField] private DownloadPopup downloadPU;
        [SerializeField] private ErrorPopup errorPU;
        [SerializeField] private MessagePopup messagePU;
        [SerializeField] private StandbyPopup standbyPU;
        [SerializeField] private PushPopup pushPU;
        [SerializeField] private CoinHistoryPopup coinHistoryPU;
        [SerializeField] private CalendarPopup calendarPU;
        [SerializeField] private EventPopup eventPU;
        [Header("★ Bindings - toast")]
        [SerializeField] private ToastMessage toastMessage;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            fader.gameObject.SetActive(true);
            developerModeGO.SetActive(false);
            testServerModeGO.SetActive(false);

            downloadConfirmPU.gameObject.SetActive(false);
            downloadPU.gameObject.SetActive(false);
            errorPU.gameObject.SetActive(false);
            toastMessage.gameObject.SetActive(false);
            messagePU.gameObject.SetActive(false);
            standbyPU.gameObject.SetActive(false);
            pushPU.gameObject.SetActive(false);
            coinHistoryPU.gameObject.SetActive(false);
            calendarPU.gameObject.SetActive(false);
            eventPU.gameObject.SetActive(false);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && Input.GetKey(KeyCode.LeftControl))
            {
                Time.timeScale = 1;
                AudioMGR.One.Pitch = 1;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && Input.GetKey(KeyCode.LeftControl))
            {
                Time.timeScale = 2;
                AudioMGR.One.Pitch = 2;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) && Input.GetKey(KeyCode.LeftControl))
            {
                Time.timeScale = 3;
                AudioMGR.One.Pitch = 3;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) && Input.GetKey(KeyCode.LeftControl))
            {
                Time.timeScale = 4;
                AudioMGR.One.Pitch = 4;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) && Input.GetKey(KeyCode.LeftControl))
            {
                Time.timeScale = 5;
                AudioMGR.One.Pitch = 5;
            }
        }
    }
}