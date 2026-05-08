using beyondi.Behaviour;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Game.C1_G03;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
    public class UILauncher : BYDSingleton<UILauncher>
    {
        // Properties
        public Button BackButton => backBTN;
        public CoinInfo CoinInfo => coinInfo;
        public CoursePopup CoursePU => coursePU;
        public MyInfoPopup MyInfoPU => myInfoPU;
        public ParentChildLMSPopup ParentChildLMSPU => parentChildLMSPU;
        public ParentSettingPopup ParentSettingPU => parentSettingPU;
        public ContinuousAttendPopup ContinuousAttendPU => continuousAttendPU;
        public ContentPopup ContentPU => contentPU;
        public StorePopup StorePU => storePU;
        public ParentsOnlyPopup ParentsOnlyPU => parentsOnlyPU;
        public Button GuideButton => guideBTN;
        public GuidePopup GuidePU => guidePU;
        public MailBoxPopup MailBoxPU => mailBoxPU;
        public FamilyPopup FamilyPU => familyPU;
        public FamilyAddPopup FamilyAddPU => familyAddPU;
        public bool IsNewMailBox => mailNewGO.activeSelf;

        // Properties
        public bool VisibleBackButton
        {
            get => backBTN.gameObject.activeSelf;
            set
            {
                backBTN.gameObject.SetActive(value);
            }
        }
        public bool VisibleCoinInfo
        {
            get => coinInfo.gameObject.activeSelf;
            set
            {
                coinInfo.gameObject.SetActive(value);
            }
        }
        public bool VisibleGuideButton
        {
            get => guideBTN.gameObject.activeSelf;
            set
            {
                guideBTN.gameObject.SetActive(value);
            }
        }
        public bool VisibleParentButton
        {
            get => parentBTN.gameObject.activeSelf;
            set
            {
                parentBTN.gameObject.SetActive(value);
            }
        }
        public bool VisibleMailButton
        {
            get => mailBTN.gameObject.activeSelf;
            set
            {
                mailBTN.gameObject.SetActive(value);
            }
        }

        // Methods
        public async UniTask LoadMails()
        {
            JObject mailData = new JObject();
            if (UserData.One.Parent.HasSignedIn)
            {
                mailData = await LMS.One.LoadMails(UserData.One.Parent.AccessToken);
                if (mailData == null) return;
            }
            else if (UserData.One.Child.HasSignedIn)
            {
                mailData = await LMS.One.LoadMails(UserData.One.Child.AccessToken);
                if (mailData == null) return;
            }
            else
            {
                mailData.Add("isNew", false);
                mailData.Add("list", new JArray());
            }
            var isNew = mailData.Value<bool>("isNew");
            var list = mailData.Value<JArray>("list");

            mailNewGO.SetActive(isNew);
            MailBoxPU.Init(list);

            OnLoadedMailBox?.Invoke(isNew);
        }
        public async UniTask ShowMailBox()
        {
            LOG.Function(this);

            await MailBoxPU.ShowPopup();
            string token = null;
            if (UserData.One.Parent.HasSignedIn)
                token = UserData.One.Parent.AccessToken;
            else if (UserData.One.Child.HasSignedIn)
                token = UserData.One.Child.AccessToken;
            MailBoxPU.ItemList.ForEach(async item =>
            {
                await LMS.One.LoadMail(token, item.SN);
            });
            await LoadMails();
        }

        // Events
        public event Action OnSignInParent;
        public event Action<bool> OnLoadedMailBox;



        // Event Listeners
        private void guideBT_OnClick()
        {
            AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
            guidePU.ShowPopup(false);
        }
        private async UniTask parentBT_OnClick()
        {
            AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
            if (await ParentsOnlyPU.ShowPopup() == SimplePopupResult.Yes)
                OnSignInParent?.Invoke();
        }
        private async UniTask mailBT_OnClick()
        {
            AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
            await ShowMailBox();
        }



        // Unity Inspectors
        [Header("Bindings")]
        [SerializeField] private Button backBTN = null;
        [SerializeField] private CoinInfo coinInfo = null;
        [SerializeField] private CoursePopup coursePU = null;
        [SerializeField] private MyInfoPopup myInfoPU = null;
        [SerializeField] private ParentChildLMSPopup parentChildLMSPU = null;
        [SerializeField] private ParentSettingPopup parentSettingPU = null;
        [SerializeField] private ContinuousAttendPopup continuousAttendPU = null;
        [SerializeField] private ContentPopup contentPU = null;
        [SerializeField] private StorePopup storePU = null;
        [SerializeField] private ParentsOnlyPopup parentsOnlyPU = null;
        [SerializeField] private Button guideBTN = null;
        [SerializeField] private GuidePopup guidePU = null;
        [SerializeField] private Button parentBTN = null;
        [SerializeField] private Button mailBTN = null;
        [SerializeField] private GameObject mailNewGO = null;
        [SerializeField] private MailBoxPopup mailBoxPU = null;
        [SerializeField] private FamilyPopup familyPU = null;
        [SerializeField] private FamilyAddPopup familyAddPU = null;

        // Unity Messages
        protected override void Awake()
        {
            base.Awake();

            VisibleBackButton = false;
            VisibleCoinInfo = false;

            coursePU.gameObject.SetActive(false);
            myInfoPU.gameObject.SetActive(false);
            parentChildLMSPU.gameObject.SetActive(false);
            parentSettingPU.gameObject.SetActive(false);
            continuousAttendPU.gameObject.SetActive(false);
            contentPU.gameObject.SetActive(false);
            storePU.gameObject.SetActive(false);
            parentsOnlyPU.gameObject.SetActive(false);
            guideBTN.gameObject.SetActive(false);
            guidePU.gameObject.SetActive(false);
            parentBTN.gameObject.SetActive(false);
            mailBTN.gameObject.SetActive(false);
            mailBoxPU.gameObject.SetActive(false);
            familyPU.gameObject.SetActive(false);
            familyAddPU.gameObject.SetActive(false);

            guideBTN.onClick.AddListener(() => guideBT_OnClick());
            parentBTN.onClick.AddListener(() => parentBT_OnClick().Forget());
            mailBTN.onClick.AddListener(() => mailBT_OnClick().Forget());
        }
        private void Start()
        {
        }
    }
}