using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using Newtonsoft.Json.Linq;

namespace DoDoEng.Launcher.UI
{
    public class ParentChildLMSPopup : PopupBase<SimplePopupResult>
	{
        // Definitions
        // Properties
        public UserDataChild ChildData => childData;

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(ChildSlot childSlot, bool jump = false)
        {
            LOG.Function(this, $"| childSlot={childSlot} | jump={jump}");

            AudioMGR.One.PlayEffect(popupCLIP);

            this.childSlot = childSlot;
            childData = childSlot.Data;
            photo.SetPhoto(childData.Photo);
            photo.VisibleButton = childData.ParentID == UserData.One.Parent.LoginID;

            bool successLoad = true;
            API.Result result = await API.One.Call(UserData.One.Parent.AccessToken, $"/api/v1/user/children/{childData.ID}");
            if (result.Success)
            {
                childData.DayProgress = result.Data.Value<JObject>("dayProgress");
                UserData.One.Child.ID = childData.ID;
                UserData.One.Child.NickName = childData.NickName;
                UserData.One.Child.AccessToken = result.Data.Value<string>("accessToken");
                if (childData.DayProgress == null)
                {
                    SystemUI.One.ErrorPU.ShowPopup("Failed to load child's progress.").Forget();
                    successLoad = false;
                }
            }
            else
            {
                SystemUI.One.ErrorPU.ShowPopup("Failed to load child's info.").Forget();
                successLoad = false;
            }

            if (successLoad)
            {
                nickTMP.text = childData.NickName;
                if (jump)
                {
                    tabControl.Select(1);
                    dayPage.Jump(UserData.One.LastReportPage, UserData.One.LastReportDate);
                    showPopup().Forget();
                    return SimplePopupResult.NA;
                }
                else
                {
                    tabControl.Select(0);
                    return await showPopup();
                }
            }
            else
                return SimplePopupResult.NA;
        }

        // Events
        //[HideInInspector] public event Action OnSignOut;



        // Fields : caching

        // Fields
        private ChildSlot childSlot;
        private UserDataChild childData;

        // Functions

        // Event Handlers
        private void photo_OnChangePhoto(Texture2D texture)
        {
            LMS.One.SavePhotoParent(childData, texture).Forget();
            childSlot.Refresh();
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();
        }
        protected override void onClose(SimplePopupResult result)
        {
            base.onClose(result);

            UserData.One.Child.AccessToken = "";
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text nickTMP = null;
        [SerializeField] private TabControl tabControl = null;
        [SerializeField] private ParentChildLMSPopupPageDay dayPage = null;
        [SerializeField] private ProfilePhoto photo = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;

        // Unity Messages
        private void Awake()
		{
        }
		private void Start()
		{
		}
        protected void OnEnable()
        {
            photo.OnChangePhoto += photo_OnChangePhoto;
        }
        protected void OnDisable()
        {
            photo.OnChangePhoto -= photo_OnChangePhoto;
        }

        // Unity Coroutine
    }
}