using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
    public class FamilyAddPopup : PopupBase<SimplePopupResult>
	{
        // Definitions
        // Properties
        public string NickName => nickNameTMP.text;

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(popupCLIP);

            nickNameTMP.text = "";
            validateTMP.text = LocalizationMGR.One.GetText("MESSAGE_48");
            checkGO.SetActive(false);
            addBT.interactable = false;

            return await showPopup();
        }

        // Events
        //[HideInInspector] public event Action OnSignOut;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        // Functions
        // Event Handlers
        // Overrides


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_InputField nickNameTMP = null;
        [SerializeField] private TMP_Text validateTMP = null;
        [SerializeField] private GameObject checkGO = null;
        [SerializeField] private Button addBT = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        //[Header("★ Config")]
        //[SerializeField] private float buttonDelay = 0.5f;

        // Unity Messages
        private void Awake()
		{
            addBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
            });
            nickNameTMP.onValueChanged.AddListener(async (value) =>
            {
                checkGO.SetActive(false);
                addBT.interactable = false;
                if (value == "")
                {
                    validateTMP.text = LocalizationMGR.One.GetText("MESSAGE_48");
                }
                else
                {
                    var result = await LMS.One.ValidateFamilyNickName(value);
                    var valid = result["valid"] != null ? result.Value<bool>("valid") : false;
                    var message = result.Value<string>("message");
                    validateTMP.text = valid ? LocalizationMGR.One.GetText("MESSAGE_49") : $"<color=#EF504E>{message}</color>";
                    checkGO.SetActive(valid);
                    addBT.interactable = valid;
                }
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