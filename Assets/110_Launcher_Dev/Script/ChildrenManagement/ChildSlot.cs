using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class ChildSlot : MonoBehaviour
    {
        // Enum
        public enum SlotState
        {
            Locked,
            CanAdd,
            Opened,
            OpenedFromFamily,
            New,
            Edit
        }

        // Properties
        public SlotState State => state;
        public UserDataChild Data => childData;

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Init(SlotState state, UserDataChild childData = null)
        {
            this.state = state;
            this.childData = childData;

            if (state == SlotState.Locked)
            {
                nameTMP.text = "";
                photo.Clear();
                moreGO.SetActive(false);
                addGO.SetActive(false);
                lockedGO.SetActive(true);
            }
            else if (state == SlotState.CanAdd)
            {
                nameTMP.text = LocalizationMGR.One.GetText("WORD_26");
                photo.Clear();
                moreGO.SetActive(false);
                addGO.SetActive(true);
                lockedGO.SetActive(false);
            }
            else if (state == SlotState.Opened)
            {
                nameTMP.text = childData.NickName;
                photo.Clear();
                moreGO.SetActive(true);
                childMoreGO.SetActive(false);
                addGO.SetActive(false);
                lockedGO.SetActive(false);
            }
            else if (state == SlotState.OpenedFromFamily)
            {
                nameTMP.text = childData.NickName;
                photo.Clear();
                moreGO.SetActive(false);
                childMoreGO.SetActive(false);
                addGO.SetActive(false);
                lockedGO.SetActive(false);
            }
            else if (state == SlotState.New)
            {
                nameTMP.text = "";
                photo.Clear();
                moreGO.SetActive(true);
                childMoreGO.SetActive(false);
                addGO.SetActive(false);
                lockedGO.SetActive(false);
            }
            else if (state == SlotState.Edit)
            {
                nameTMP.text = childData.NickName;
                photo.SetPhoto(childData.Photo);
                moreGO.SetActive(true);
                childMoreGO.SetActive(false);
                addGO.SetActive(false);
                lockedGO.SetActive(false);
            }
        }
        public void Refresh()
        {
            if (childData != null)
            {
                photo.SetPhoto(childData.Photo);
            }
        }
        public void HideMenu()
        {
            childMoreGO.SetActive(false);
        }

        // Events
        public event Action OnClick;
        public event Action OnAdd;
        public event Action<ChildSlot> OnLMS;
        public event Action<UserDataChild> OnEdit;
        public event Action<UserDataChild> OnRemove;
        public event Action<UserDataChild, Texture2D> OnChangePhoto;
        public event Action<ChildSlot> OnSignIn;



        // Fields
        private SlotState state;
        private UserDataChild childData;

        // Event Handlers
        private async UniTask photoBT_OnClick()
        {
            LOG.Function(this);

            OnClick?.Invoke();
            if (await photo.ChangePhoto())
                OnChangePhoto?.Invoke(Data, photo.Photo);
        }
        private async UniTask moreBT_OnClick()
        {
            LOG.Info($"moreBT_OnClick()", this);

            if (state == SlotState.New || state == SlotState.Edit)
            {
                if (await photo.ChangePhoto())
                    OnChangePhoto?.Invoke(Data, photo.Photo);
            }
            else
            {
                OnClick?.Invoke();
                childMoreGO.SetActive(!childMoreGO.activeSelf);
            }
        }
        private void editBT_OnClick()
        {
            LOG.Info($"editBT_OnClick()", this);

            OnClick?.Invoke();
            OnEdit?.Invoke(Data);
        }
        private void removeBT_OnClick()
        {
            LOG.Info($"removeBT_OnClick()", this);

            OnClick?.Invoke();
            OnRemove?.Invoke(Data);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text nameTMP = null;
        [SerializeField] private ProfilePhoto photo = null;
        [SerializeField] private GameObject moreGO = null;
        [SerializeField] private Button moreBT = null;
        [SerializeField] private GameObject childMoreGO = null;
        [SerializeField] private GameObject lockedGO = null;
        [SerializeField] private GameObject addGO = null;
        [SerializeField] private Button photoBT = null;
        [SerializeField] private Button editBT = null;
        [SerializeField] private Button removeBT = null;
        [SerializeField] private Button signInBT = null;

        // Unity Messages
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => {
                OnClick?.Invoke();
                if (state == SlotState.CanAdd)
                    OnAdd?.Invoke();
                else if (state == SlotState.Opened || state == SlotState.OpenedFromFamily)
                    OnLMS?.Invoke(this);
            });
            photoBT.onClick.AddListener(() => photoBT_OnClick().Forget());
            moreBT.onClick.AddListener(() => moreBT_OnClick().Forget());
            editBT?.onClick.AddListener(() => editBT_OnClick());
            removeBT?.onClick.AddListener(() => removeBT_OnClick());
            signInBT?.onClick.AddListener(() => OnSignIn?.Invoke(this));

            Init(SlotState.Locked);
        }
        private void Start()
        {
        }
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }
        private void OnDestroy()
        {
        }
    }
}