using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher
{
    public class LobbyPopSignIn : MonoBehaviour
    {
        // Protperties
        public string ID => idTMP.text;
        public string Password => passwordTMP.text;
        public bool AutoSignIn => autoSignInTG.isOn;

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Show()
        {
            idTMP.text = PlayerPrefs.GetString("CHILD_SIGN_ID", "");
            passwordTMP.text = PlayerPrefs.GetString("CHILD_SIGN_PW", "");
            passwordVisibleTG.isOn = true;
            autoSignInTG.SetIsOnWithoutNotify(PlayerPrefs.GetInt("CHILD_SIGN_AUTO", 0) != 0);

            Activate(true);
        }

        // Events
        public event Action OnSignIn;
        public event Action OnParents;



        // Event Handlers
        private void loginBT_OnClick()
        {
            PlayerPrefs.SetString("CHILD_SIGN_ID", ID);
            PlayerPrefs.SetString("CHILD_SIGN_PW", Password);
            PlayerPrefs.SetInt("CHILD_SIGN_AUTO", AutoSignIn ? 1 : 0);
            OnSignIn?.Invoke();
        }

        // Fields
        private Vector2 originPopupPosition;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_InputField idTMP = null;
        [SerializeField] private TMP_InputField passwordTMP = null;
        [SerializeField] private Toggle passwordVisibleTG = null;
        [SerializeField] private Toggle autoSignInTG = null;
        [SerializeField] private Button loginBT = null;
        [SerializeField] private Button parentsBT = null;
        [SerializeField] private RectTransform popupRT = null;

        // Unity Messages
        private void Awake()
        {
            loginBT.onClick.AddListener(() => loginBT_OnClick());
            parentsBT.onClick.AddListener(() => OnParents?.Invoke());
            idTMP.onValueChanged.AddListener((value) => idTMP.text = value.ToLower());
            passwordTMP.onValueChanged.AddListener((value) => passwordTMP.text = value.ToLower());
        }
        private void Start()
        {
            originPopupPosition = popupRT.anchoredPosition;
        }
        protected void OnEnable()
        {
            //autoSignInTG.isOn = UserData.One.Child.AutoSignIn;
        }
        protected void OnDisable()
        {
        }
        private void Update()
        {
            if (TouchScreenKeyboard.visible)
            {
                if (passwordTMP.isFocused)
                    popupRT.anchoredPosition = new Vector2(originPopupPosition.x, originPopupPosition.y + 255);
                else
                    popupRT.anchoredPosition = new Vector2(originPopupPosition.x, originPopupPosition.y + 155);
            }
            else
            {
                popupRT.anchoredPosition = originPopupPosition;
            }
        }
        private void OnDestroy()
        {
        }

    }
}