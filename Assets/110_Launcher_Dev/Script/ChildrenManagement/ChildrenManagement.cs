using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Launcher.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class ChildrenManagement : MonoBehaviour, IPointerDownHandler
    {
        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public async UniTask Show(bool jump = false)
        {
            Activate(true);

            popChildInfo.Activate(false);
            await Refresh(jump);
        }
        public async UniTask Refresh(bool jump = false)
        {
            nickTMP.text = UserData.One.Parent.NickName;
            if (UserData.One.Parent.VoucherAvailable)
            {
                ticketTMP.text = LocalizationMGR.One.GetText("WORD_98") + LocalizationMGR.One.GetText("WORD_99", UserData.One.Parent.VoucherEndDate.ToString("yyyy.MM.dd"));
            }
            else
            {
                ticketTMP.text = LocalizationMGR.One.GetText("WORD_100");
            }

            childSlots.ForEach(slot => {
                slot.OnClick -= slot_OnClick;
                slot.OnAdd -= slot_OnAdd;
                slot.OnLMS -= slot_OnLMS;
                slot.OnEdit -= slot_OnEdit;
                slot.OnRemove -= slot_OnRemove;
                slot.OnChangePhoto -= slot_OnChangePhoto;
                slot.OnSignIn -= slot_OnSignIn;
            });
            childSlots.Clear();
            foreach (var ch in slotsRT.GetChildren())
            {
                Destroy(ch.gameObject);
            }

            API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, "/api/v1/user/children");

            if (result.Success)
            {
                UserData.One.Parent.ChildrenSlotLimitCount = result.Data.Value<int>("childrenSlotLimitCount");
                UserData.One.Parent.InitChildren(result.Data.Value<JArray>("children"));

                //int slotCount = Math.Max(UserData.One.Parent.ChildrenSlotLimitCount, UserData.One.Parent.ChildDatas.Count);
                int slotCount = UserData.One.Parent.ChildDatas.Count;
                int myChildCount = UserData.One.Parent.ChildDatas.Sum(data => data.ParentID == UserData.One.Parent.LoginID ? 1 : 0);
                if (myChildCount < UserData.One.Parent.ChildrenSlotLimitCount)
                    slotCount += UserData.One.Parent.ChildrenSlotLimitCount - myChildCount;
                for (int i = 0; i < slotCount; i++)
                {
                    var slot = Instantiate(childSlotPF, slotsRT);
                    slot.OnClick += slot_OnClick;
                    slot.OnAdd += slot_OnAdd;
                    slot.OnLMS += slot_OnLMS;
                    slot.OnEdit += slot_OnEdit;
                    slot.OnRemove += slot_OnRemove;
                    slot.OnChangePhoto += slot_OnChangePhoto;
                    slot.OnSignIn += slot_OnSignIn;
                    if (i < UserData.One.Parent.ChildDatas.Count)
                    {
                        if (UserData.One.Parent.ChildDatas[i].ParentID == UserData.One.Parent.LoginID)
                            slot.Init(ChildSlot.SlotState.Opened, UserData.One.Parent.ChildDatas[i]);
                        else
                            slot.Init(ChildSlot.SlotState.OpenedFromFamily, UserData.One.Parent.ChildDatas[i]);
                        await LMS.One.LoadPhotoParent(slot.Data);
                    }
                    else
                    {
                        slot.Init(ChildSlot.SlotState.CanAdd);
                    }
                    childSlots.Add(slot);

                    slot.Refresh();
                }

                if (jump)
                {
                    await UILauncher.One.ParentChildLMSPU.ShowPopup(childSlots[UserData.One.LastReportChild], jump);
                }
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("자녀 조회에 실패하였습니다.").Forget();
            }
        }

        // Events
        public event Action OnUpdate;
        public event Action OnInfoEdit;
        public event Action OnSignOut;
        public event Action OnUnregister;
        public event Action OnRestart;
        public event Action OnInit;
        public event Action OnAlarm;
        public event Action<string> OnSignInChild;



        // Event Handlers
        private void popSettings_OnUpdate()
        {
            LOG.Function(this);

            OnUpdate?.Invoke();
        }
        private void popSettings_OnInfoEdit()
        {
            LOG.Function(this);

            UILauncher.One.ParentSettingPU.CloseWithResult();

            OnInfoEdit?.Invoke();
        }
        private void popSettings_OnSignOut()
        {
            LOG.Function(this);

            UILauncher.One.ParentSettingPU.CloseWithResult();

            OnSignOut?.Invoke();
        }
        private void popSettings_OnUnregister()
        {
            LOG.Function(this);

            UILauncher.One.ParentSettingPU.CloseWithResult();

            OnUnregister?.Invoke();
        }
        private void popSettings_OnRestart()
        {
            LOG.Function(this);

            OnRestart?.Invoke();
        }
        private void popSettings_OnInit()
        {
            LOG.Function(this);

            OnInit?.Invoke();
        }
        private void popSettings_OnAlarm()
        {
            LOG.Function(this);

            OnAlarm?.Invoke();
        }
        private void popSettings_OnChangeSettings()
        {
            LOG.Function(this);

            LMS.One.SaveSettingsParent().Forget();
        }
        private void slot_OnClick()
        {
            LOG.Function(this);

            childSlots.ForEach(slot => slot.HideMenu());
        }
        private void slot_OnAdd()
        {
            LOG.Function(this);

            popChildInfo.Show();
        }
        private void slot_OnLMS(ChildSlot slot)
        {
            LOG.Function(this, $"| slot={slot}");

            UserData.One.LastReportChild = UserData.One.Parent.ChildDatas.IndexOf(slot.Data);
            UILauncher.One.ParentChildLMSPU.ShowPopup(slot).Forget();
        }
        private void slot_OnEdit(UserDataChild data)
        {
            LOG.Function(this, $"| data={data}");

            popChildInfo.Show(data);
        }
        private void slot_OnSignIn(ChildSlot slot)
        {
            LOG.Function(this, $"| slot={slot}");

            OnSignInChild?.Invoke(slot.Data.ID);
        }
        private async void slot_OnChangePhoto(UserDataChild childData, Texture2D texture)
        {
            await LMS.One.SavePhotoParent(childData, texture);
        }
        private async void slot_OnRemove(UserDataChild data)
        {
            LOG.Function(this, $"| data={data}");

            if (await SystemUI.One.ShowPopupDeleteChild())
            {
                API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/children/{data.ID}", API.Method.Delete);

                if (result.Success)
                {
                    Refresh().Forget();
                }
            }
        }
        private async void popChildInfo_OnAdd()
        {
            LOG.Function(this);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken, 
                "/api/v1/user/children", 
                API.Method.Post, 
                JObject.Parse($"{{loginId: '{popChildInfo.ID}', password: '{popChildInfo.Password}', nickName: '{popChildInfo.NickName}', yearOfBirth: {popChildInfo.BirthYear}, courseId: {popChildInfo.Course}}}"));

            if (result.Success)
            {
                popChildInfo.Activate(false);
                Refresh().Forget();
                SystemUI.One.ShowPopupAddUserGuide().Forget();
            }
            else
            {
                if (result.Data.Value<string>("code") == "USER_REGISTERED")
                    SystemUI.One.ErrorPU.ShowPopup(result.Data.Value<string>("message")).Forget();
                else
                    SystemUI.One.ErrorPU.ShowPopup("Failed to add child.").Forget();
            }
        }
        private async void popChildInfo_OnEdit(UserDataChild childData)
        {
            LOG.Function(this, $"| id={childData.ID}");

            var json = JObject.Parse($"{{nickName: '{popChildInfo.NickName}', yearOfBirth: {popChildInfo.BirthYear}, learningCourse: {popChildInfo.Course}}}");
            if (popChildInfo.HasChangedPassword)
                json.Add("password", popChildInfo.Password);

            API.Result result = await API.One.Call(
                UserData.One.Parent.AccessToken, 
                $"/api/v1/user/children/{childData.ID}", 
                API.Method.Put, 
                json);

            if (result.Success)
            {
                if (popChildInfo.HasChangedPhoto)
                {
                    await LMS.One.SavePhotoParent(childData, popChildInfo.Photo);
                }
                popChildInfo.Activate(false);
                Refresh().Forget();
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Failed to edit child.").Forget();
            }
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            LOG.Function(this);

            childSlots.ForEach(slot => slot.HideMenu());
        }
        public void UILauncher_OnLoadedMailBox(bool isNew)
        {
            LOG.Function(this);

            mailBoxNewGO.SetActive(isNew);
        }
        private void familyPU_OnRemoveFamily()
        {
            Refresh().Forget();
        }

        // Fields
        private List<ChildSlot> childSlots = new List<ChildSlot>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text nickTMP = null;
        [SerializeField] private TMP_Text ticketTMP = null;
        [SerializeField] private Button familyBT = null;
        [SerializeField] private Button mailBoxBT = null;
        [SerializeField] private GameObject mailBoxNewGO = null;
        [SerializeField] private Button ticketBT = null;
        [SerializeField] private Button settingsBT = null;
        [SerializeField] private Button signOutBT = null;
        [SerializeField] private CMPopChildInfo popChildInfo = null;
        [SerializeField] private RectTransform slotsRT = null;
        [SerializeField] private ChildSlot childSlotPF = null;

        // Unity Messages
        private void Awake()
        {
            popChildInfo.Activate(false);

            familyBT.onClick.AddListener(async () =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                UILauncher.One.FamilyPU.ShowPopup().Forget();
            });
            mailBoxBT.onClick.AddListener(async () => {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                await UILauncher.One.ShowMailBox();
            });
            ticketBT.onClick.AddListener(async () =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                if (await UILauncher.One.ParentsOnlyPU.ShowPopup() == SimplePopupResult.Yes)
                {
                    if (await UILauncher.One.StorePU.ShowPopup() == SimplePopupResult.Okay)
                    {
                        await Refresh();
                    }
                }
            });
            settingsBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                UILauncher.One.ParentSettingPU.ShowPopup().Forget();
            });
            signOutBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                OnSignOut?.Invoke();
            });
        }
        private void Start()
        {
        }
        protected void OnEnable()
        {
            popChildInfo.OnAdd += popChildInfo_OnAdd;
            popChildInfo.OnEdit += popChildInfo_OnEdit;
            UILauncher.One.ParentSettingPU.OnUpdate += popSettings_OnUpdate;
            UILauncher.One.ParentSettingPU.OnInfoEdit += popSettings_OnInfoEdit;
            UILauncher.One.ParentSettingPU.OnSignOut += popSettings_OnSignOut;
            UILauncher.One.ParentSettingPU.OnUnregister += popSettings_OnUnregister;
            UILauncher.One.ParentSettingPU.OnRestart += popSettings_OnRestart;
            UILauncher.One.ParentSettingPU.OnInit += popSettings_OnInit;
            UILauncher.One.ParentSettingPU.OnAlarm += popSettings_OnAlarm;
            UILauncher.One.ParentSettingPU.OnChangeSettings += popSettings_OnChangeSettings;
            UILauncher.One.OnLoadedMailBox += UILauncher_OnLoadedMailBox;
            UILauncher.One.FamilyPU.OnRemoveFamily += familyPU_OnRemoveFamily;
            mailBoxNewGO.SetActive(UILauncher.One.IsNewMailBox);
        }
        protected void OnDisable()
        {
            popChildInfo.OnAdd -= popChildInfo_OnAdd;
            popChildInfo.OnEdit -= popChildInfo_OnEdit;
            if (UILauncher.One)
            {
                UILauncher.One.ParentSettingPU.OnUpdate -= popSettings_OnUpdate;
                UILauncher.One.ParentSettingPU.OnInfoEdit -= popSettings_OnInfoEdit;
                UILauncher.One.ParentSettingPU.OnSignOut -= popSettings_OnSignOut;
                UILauncher.One.ParentSettingPU.OnUnregister -= popSettings_OnUnregister;
                UILauncher.One.ParentSettingPU.OnRestart -= popSettings_OnRestart;
                UILauncher.One.ParentSettingPU.OnInit -= popSettings_OnInit;
                UILauncher.One.ParentSettingPU.OnAlarm -= popSettings_OnAlarm;
                UILauncher.One.ParentSettingPU.OnChangeSettings -= popSettings_OnChangeSettings;
                UILauncher.One.OnLoadedMailBox -= UILauncher_OnLoadedMailBox;
                UILauncher.One.FamilyPU.OnRemoveFamily -= familyPU_OnRemoveFamily;
            }
        }
        private void OnDestroy()
        {
            childSlots.ForEach(slot => {
                slot.OnClick -= slot_OnClick;
                slot.OnAdd -= slot_OnAdd;
                slot.OnLMS -= slot_OnLMS;
                slot.OnEdit -= slot_OnEdit;
                slot.OnRemove -= slot_OnRemove;
                slot.OnChangePhoto -= slot_OnChangePhoto;
                slot.OnSignIn -= slot_OnSignIn;
            });
            childSlots.Clear();
            foreach (var ch in slotsRT.GetChildren())
            {
                Destroy(ch.gameObject);
            }
        }
    }
}