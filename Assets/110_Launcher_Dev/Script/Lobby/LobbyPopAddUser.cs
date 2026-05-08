using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class LobbyPopAddUser : MonoBehaviour
    {
        // Definition
        public const string PP_ACCOUNT = "CHILD_ACCOUNTS";
        private struct Account
        {
            public string ID;
            public string PassWord;
            public string NickName;
        }

        // Protperties
        public bool HasUser => PlayerPrefs.GetString(PP_ACCOUNT, "") != "";

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Show()
        {
            Activate(true);
        }
        public void AddUser(string id, string pw, string name)
        {
            loadAccounts();
            var foundIdx = accounts.FindIndex((account) => account.ID == id);
            if (foundIdx != -1)
            {
                var account = accounts[foundIdx];
                account.PassWord = pw;
                account.NickName = name;
            }
            else
            {
                var account = new Account();
                account.ID = id;
                account.PassWord = pw;
                account.NickName = name;
                accounts.Add(account);
                if (accounts.Count > 2)
                {
                    accounts.RemoveAt(0);
                }
            }
            saveAccounts();
        }

        // Events
        public event Action OnAdd;
        public event Action<string, string> OnSelect;
        public event Action OnParent;



        // Event Handlers
        private void slot_OnAdd(int idx)
        {
            OnAdd?.Invoke();
        }
        private void slot_OnSelect(int idx)
        {
            OnSelect?.Invoke(accounts[idx].ID, accounts[idx].PassWord);
        }
        private void slot_OnRemove(int idx)
        {
            removeSlot(idx).Forget();
        }

        // Functions
        private void loadAccounts()
        {
            if (accounts == null)
            {
                accounts = new List<Account>();
                var accountsStr = PlayerPrefs.GetString(PP_ACCOUNT, "");
                var accountsStrList = accountsStr.Split(';');
                foreach (var accountStr in accountsStrList)
                {
                    if (accountStr == "")
                        continue;
                    var accountStrData = accountStr.Split(',');
                    var account = new Account();
                    account.ID = accountStrData[0];
                    account.PassWord = accountStrData[1];
                    account.NickName = accountStrData[2];
                    accounts.Add(account);
                }
            }
        }
        private void saveAccounts()
        {
            string[] dataList = new string[accounts.Count];
            for (int i = 0; i < accounts.Count; i++)
            {
                dataList[i] = $"{accounts[i].ID},{accounts[i].PassWord},{accounts[i].NickName}";
            }
            PlayerPrefs.SetString(PP_ACCOUNT, String.Join(";", dataList));
        }
        private void refresh()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (i < accounts.Count)
                    slot.Init(i, accounts[i].NickName);
                else
                    slot.Init(i);
            }
        }
        private async UniTask removeSlot(int idx)
        {
            if (await SystemUI.One.ShowPopupRemoveAccount())
            {
                accounts.RemoveAt(idx);
                saveAccounts();
                refresh();
            }
        }
        
        // Fields
        private List<Account> accounts = null;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private LobbyPopAddUserSlot[] slots = null;
        [SerializeField] private Button backBT = null;
        [SerializeField] private Button parentBT = null;

        // Unity Messages
        private void Awake()
        {
            backBT.onClick.AddListener(() => Activate(false));
            parentBT.onClick.AddListener(() => OnParent?.Invoke());
        }
        private void Start()
        {
        }
        protected void OnEnable()
        {
            loadAccounts();
            refresh();

            slots.ForEach(slot =>
            {
                slot.OnAdd += slot_OnAdd;
                slot.OnSelect += slot_OnSelect;
                slot.OnRemove += slot_OnRemove;
            });
        }
        protected void OnDisable()
        {
            slots.ForEach(slot =>
            {
                slot.OnAdd -= slot_OnAdd;
                slot.OnSelect -= slot_OnSelect;
                slot.OnRemove -= slot_OnRemove;
            });
        }
        private void OnDestroy()
        {
        }

    }
}