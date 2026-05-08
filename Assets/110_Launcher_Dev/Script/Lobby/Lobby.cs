using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using DoDoEng.Launcher.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class Lobby : MonoBehaviour
    {
        // Definitions
        public enum Menu
        {
            Today,
            Activity,
            Movie,
            Book,
            Game,
            Studio,
            MyPet
        }

        // Properties
        public bool HasUser => popAddUser.HasUser;

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);

            cg.blocksRaycasts = active;
        }
        public void Show(bool immediately = false)
        {
            LOG.Function(this, $"{immediately}");

            Activate(true);

            if (immediately)
            {
                lobbyANI.SetTrigger("idle");
                lobbyInfo.gameObject.SetActive(true);
                Refresh();
            }
            else StartCoroutine(coIntro());
        }
        public void Refresh(bool hideAll = false)
        {
            popSignIn.Activate(false);
            popAddUser.Activate(false);

            lobbyInfo.Refresh();

            if (UserData.One.Child.HasSignedIn)
            {
                UILauncher.One.VisibleCoinInfo = true;
                UILauncher.One.VisibleGuideButton = true;
                UILauncher.One.VisibleParentButton = true;
                UILauncher.One.VisibleMailButton = true;
                if (UserData.One.Child.HasSignedIn && UserData.One.Child.IsGuideFirst)
                {
                    UILauncher.One.GuidePU.ShowPopup(true);
                }
            }
            else
            {
                UILauncher.One.VisibleCoinInfo = false;
                UILauncher.One.VisibleGuideButton = false;
                UILauncher.One.VisibleParentButton = false;
                UILauncher.One.VisibleMailButton = false;
                popAddUser.Activate(!hideAll && popAddUser.HasUser);
            }

        }
        public void SetInteract(bool interact)
        {
            cg.blocksRaycasts = interact;
        }
        public void ShowPopSignIn()
        {
            if (popAddUser.HasUser)
                popAddUser.Show();
            else
                popSignIn.Show();
        }
        public void AddUser(string id, string pw, string name)
        {
            popAddUser.AddUser(id, pw, name);
        }

        // Events
        public event Action<string, string, bool> OnSignInChild;
        public event Action OnSignOut;
        public event Action OnParents;
        public event Action OnUpdate;
        public event Action OnRestart;
        public event Action OnInit;
        public event Action OnAlarm;
        public event Action OnClickLogo;
        public event Action<Menu, LobbyMenuButton> OnClickMenu;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Event Handlers
        private void lobbyInfo_OnClick()
        {
            LOG.Info($"lobbyInfo_OnClick()", this);

            if (UserData.One.Child.HasSignedIn)
                UILauncher.One.MyInfoPU.ShowPopup().Forget();
            else
                ShowPopSignIn();
        }
        private void popAddUser_OnAdd()
        {
            popSignIn.Show();
        }
        private void popAddUser_OnSelect(string id, string pw)
        {
            OnSignInChild?.Invoke(id, pw, true);
        }
        private void popAddUser_OnParents()
        {
            LOG.Info($"popAddUser_OnParents()", this);

            OnParents?.Invoke();
        }
        private void popSignIn_OnSignIn()
        {
            LOG.Info($"popSignIn_OnSignIn()", this);

            if (string.IsNullOrEmpty(popSignIn.ID))
            {
                SystemUI.One.ShowPopupNoInputID();
                return;
            }

            OnSignInChild?.Invoke(popSignIn.ID, popSignIn.Password, popSignIn.AutoSignIn);
        }
        private void popSignIn_OnParents()
        {
            LOG.Info($"popSignIn_OnParents()", this);

            OnParents?.Invoke();
        }
        private void myInfoPU_OnUpdate()
        {
            LOG.Function(this);

            OnUpdate?.Invoke();
        }
        private void myInfoPU_OnSignOut()
        {
            LOG.Function(this);

            OnSignOut?.Invoke();

            UILauncher.One.MyInfoPU.CloseWithResult();

            Refresh();
        }
        private void myInfoPU_OnChangeSettings()
        {
            LOG.Function(this);

            LMS.One.SaveSettingsChild().Forget();
        }
        private void myInfoPU_OnRestart()
        {
            LOG.Function(this);

            OnRestart?.Invoke();
        }
        private void myInfoPU_OnInit()
        {
            LOG.Function(this);

            OnInit?.Invoke();
        }
        private void myInfoPU_OnAlarm()
        {
            LOG.Function(this);

            OnAlarm?.Invoke();
        }
        private void myInfoPU_OnChangePhoto()
        {
            lobbyInfo.Refresh();
        }
        private void lobbyMenuButton_OnClick(LobbyMenuButton button)
        {
            LOG.Function(this, $"| button={button}");

            AudioMGR.One.PlayEffect(SfxMoment.Common_Click);

            var menu = Menu.Today;
            if (button == dayStudyBT) menu = Menu.Today;
            else if (button == myRoomBT) menu = Menu.MyPet;
            else if (button == playgroundBT) menu = Menu.Game;
            else if (button == movieBT) menu = Menu.Movie;
            else if (button == eBookBT) menu = Menu.Book;
            else if (button == activityBT) menu = Menu.Activity;
            else if (button == aISpeakBT) menu = Menu.Studio;

            OnClickMenu?.Invoke(menu, button);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Animator lobbyANI = null;
        [SerializeField] private LobbyInfo lobbyInfo = null;
        [SerializeField] private LobbyMenuButton dayStudyBT = null;
        [SerializeField] private LobbyMenuButton myRoomBT = null;
        [SerializeField] private LobbyMenuButton playgroundBT = null;
        [SerializeField] private LobbyMenuButton movieBT = null;
        [SerializeField] private LobbyMenuButton eBookBT = null;
        [SerializeField] private LobbyMenuButton activityBT = null;
        [SerializeField] private LobbyMenuButton aISpeakBT = null;
        [SerializeField] private LobbyPopSignIn popSignIn = null;
        [SerializeField] private LobbyPopAddUser popAddUser = null;
        [SerializeField] private Button logoBT = null;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            lobbyInfo.gameObject.SetActive(false);
            logoBT.onClick.AddListener(() => OnClickLogo?.Invoke());
        }
        private void Start()
        {
            UILauncher.One.VisibleCoinInfo = false;

            popSignIn.Activate(false);
            popAddUser.Activate(false);
        }
        protected void OnEnable()
        {
            lobbyInfo.OnClick += lobbyInfo_OnClick;

            popAddUser.OnAdd += popAddUser_OnAdd;
            popAddUser.OnSelect += popAddUser_OnSelect;
            popAddUser.OnParent += popAddUser_OnParents;

            popSignIn.OnSignIn += popSignIn_OnSignIn;
            popSignIn.OnParents += popSignIn_OnParents;

            dayStudyBT.OnClick += lobbyMenuButton_OnClick;
            myRoomBT.OnClick += lobbyMenuButton_OnClick;
            playgroundBT.OnClick += lobbyMenuButton_OnClick;
            movieBT.OnClick += lobbyMenuButton_OnClick;
            eBookBT.OnClick += lobbyMenuButton_OnClick;
            activityBT.OnClick += lobbyMenuButton_OnClick;
            aISpeakBT.OnClick += lobbyMenuButton_OnClick;

            UILauncher.One.MyInfoPU.OnUpdate += myInfoPU_OnUpdate;
            UILauncher.One.MyInfoPU.OnSignOut += myInfoPU_OnSignOut;
            UILauncher.One.MyInfoPU.OnChangeSettings += myInfoPU_OnChangeSettings;
            UILauncher.One.MyInfoPU.OnRestart += myInfoPU_OnRestart;
            UILauncher.One.MyInfoPU.OnInit += myInfoPU_OnInit;
            UILauncher.One.MyInfoPU.OnAlarm += myInfoPU_OnAlarm;
            UILauncher.One.MyInfoPU.OnChangePhoto += myInfoPU_OnChangePhoto;
        }
        protected void OnDisable()
        {
            lobbyInfo.OnClick -= lobbyInfo_OnClick;

            popAddUser.OnAdd -= popAddUser_OnAdd;
            popAddUser.OnSelect -= popAddUser_OnSelect;
            popAddUser.OnParent -= popAddUser_OnParents;

            popSignIn.OnSignIn -= popSignIn_OnSignIn;
            popSignIn.OnParents -= popSignIn_OnParents;

            dayStudyBT.OnClick -= lobbyMenuButton_OnClick;
            myRoomBT.OnClick -= lobbyMenuButton_OnClick;
            playgroundBT.OnClick -= lobbyMenuButton_OnClick;
            movieBT.OnClick -= lobbyMenuButton_OnClick;
            eBookBT.OnClick -= lobbyMenuButton_OnClick;
            activityBT.OnClick -= lobbyMenuButton_OnClick;
            aISpeakBT.OnClick -= lobbyMenuButton_OnClick;

            if (UILauncher.One)
            {
                UILauncher.One.MyInfoPU.OnUpdate -= myInfoPU_OnUpdate;
                UILauncher.One.MyInfoPU.OnSignOut -= myInfoPU_OnSignOut;
                UILauncher.One.MyInfoPU.OnChangeSettings -= myInfoPU_OnChangeSettings;
                UILauncher.One.MyInfoPU.OnRestart -= myInfoPU_OnRestart;
                UILauncher.One.MyInfoPU.OnInit -= myInfoPU_OnInit;
                UILauncher.One.MyInfoPU.OnAlarm -= myInfoPU_OnAlarm;
                UILauncher.One.MyInfoPU.OnChangePhoto -= myInfoPU_OnChangePhoto;
            }
        }
        private void OnDestroy()
        {
        }

        // Unity Coroutine
        private IEnumerator coIntro()
        {
            //lobbyANI.SetTrigger("logo");
            //yield return new WaitForSeconds(2.0f);
            lobbyANI.SetTrigger("lobby");
            yield return new WaitForSeconds(2.0f);
            lobbyInfo.gameObject.SetActive(true);
            Refresh();
        }
    }
}