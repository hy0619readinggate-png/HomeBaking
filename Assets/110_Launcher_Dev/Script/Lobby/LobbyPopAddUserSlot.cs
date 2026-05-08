using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class LobbyPopAddUserSlot : MonoBehaviour
    {
        // Protperties

        // Methods
        public void Init(int idx, string name = "")
        {
            this.idx = idx;

            if (name != "")
            {
                addBT.gameObject.SetActive(false);
                userGO.SetActive(true);
                nameTMP.text = name;
            }
            else
            {
                addBT.gameObject.SetActive(true);
                userGO.SetActive(false);
            }
        }

        // Events
        public event Action<int> OnAdd;
        public event Action<int> OnSelect;
        public event Action<int> OnRemove;



        // Fields
        private int idx;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button addBT = null;
        [SerializeField] private GameObject userGO = null;
        [SerializeField] private Button userBT = null;
        [SerializeField] private TMP_Text nameTMP = null;
        [SerializeField] private Button deleteBT = null;

        // Unity Messages
        private void Awake()
        {
            addBT.onClick.AddListener(() => OnAdd?.Invoke(idx));
            userBT.onClick.AddListener(() => OnSelect?.Invoke(idx));
            deleteBT.onClick.AddListener(() => OnRemove?.Invoke(idx));
        }
        private void Start()
        {
        }
        protected void OnEnable()
        {
            //autoSignInTG.isOn = UserData.One.Child.AutoSignIn;
        }
        protected void OnDisable()
        {
        }
        private void OnDestroy()
        {
        }

    }
}