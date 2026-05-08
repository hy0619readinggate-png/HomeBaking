using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
    public class MailBoxPopup : PopupBase<SimplePopupResult>
	{
        // Definitions

        // Properties
        public List<MailBoxItem> ItemList => itemList;

        // Methods
        public void Init(JArray list)
        {
            removeAllItems();

            foreach (var data in list)
            {
                var item = Instantiate(itemPF, itemsRT);
                item.Init(data);
                item.OnDelete += item_OnDelete;
                itemList.Add(item);
            }

            refreshUI();
        }
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(popupCLIP);

            return await showPopup();
        }

        // Events
        //[HideInInspector] public event Action OnSignOut;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private List<MailBoxItem> itemList = new List<MailBoxItem>();

        // Functions
        private void removeAllItems()
        {
            itemList.ForEach(thumbnail => thumbnail.OnDelete -= item_OnDelete);
            itemList.Clear();

            foreach (var ch in itemsRT.GetChildren())
            {
                if (ch.gameObject != noItemsGO)
                    Destroy(ch.gameObject);
            }

            refreshUI();
        }
        private async UniTask removeMail(MailBoxItem item)
        {
            AudioMGR.One.PlayEffectUI(SfxMoment.System_Back);
            cg.blocksRaycasts = false;
            if (await SystemUI.One.ShowPopupRemoveMail())
            {
                string token = null;
                if (UserData.One.Parent.HasSignedIn)
                    token = UserData.One.Parent.AccessToken;
                else if (UserData.One.Child.HasSignedIn)
                    token = UserData.One.Child.AccessToken;

                if (await LMS.One.RemoveMail(token, item.SN))
                {
                    itemList.Remove(item);
                    Destroy(item.gameObject);
                }
            }
            refreshUI();

            cg.blocksRaycasts = true;
        }
        private async UniTask removeAllMails()
        {
            cg.blocksRaycasts = false;
            if (await SystemUI.One.ShowPopupRemoveAllMails())
            {
                string token = null;
                if (UserData.One.Parent.HasSignedIn)
                    token = UserData.One.Parent.AccessToken;
                else if (UserData.One.Child.HasSignedIn)
                    token = UserData.One.Child.AccessToken;

                if (await LMS.One.RemoveAllMails(token))
                {
                    removeAllItems();
                }
            }
            cg.blocksRaycasts = true;
        }
        private void refreshUI()
        {
            noItemsGO.SetActive(itemList.Count == 0);
            removeAllBT.interactable = itemList.Count > 0;
        }

        // Event Handlers
        private void item_OnDelete(MailBoxItem item)
        {
            removeMail(item).Forget();
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private RectTransform itemsRT = null;
        [SerializeField] private MailBoxItem itemPF = null;
        [SerializeField] private GameObject noItemsGO = null;
        [SerializeField] private Button removeAllBT = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        //[Header("★ Config")]
        //[SerializeField] private float buttonDelay = 0.5f;

        // Unity Messages
        private void Awake()
		{
            removeAllBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                removeAllMails().Forget();
            });
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

        // Unity Coroutine
    }
}