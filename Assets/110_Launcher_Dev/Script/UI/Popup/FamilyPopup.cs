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
using System;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.Launcher.UI
{
    public class FamilyPopup : PopupBase<SimplePopupResult>
	{
        // Definitions
        // Properties

        // Methods
        public void Init(JArray list)
        {
        }
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(popupCLIP);

            myNameTMP.text = UserData.One.Parent.NickName;
            if (UserData.One.Parent.FamilyStatus == "WAITING")
            {
                familyNameWaitTMP.text = UserData.One.Parent.FamilyNickName;
                familyStateTMP.text = LocalizationMGR.One.GetText(UserData.One.Parent.FamilyManager ? "WORD_145" : "WORD_103");
                emptyGO.SetActive(false);
                waitGO.SetActive(true);
                familyGO.SetActive(false);
            }
            else if (UserData.One.Parent.FamilyStatus == "APPROVED")
            {
                familyNameTMP.text = UserData.One.Parent.FamilyNickName;
                emptyGO.SetActive(false);
                waitGO.SetActive(false);
                familyGO.SetActive(true);
            }
            else
            {
                emptyGO.SetActive(true);
                waitGO.SetActive(false);
                familyGO.SetActive(false);
            }

            return await showPopup();
        }

        // Events
        [HideInInspector] public event Action OnRemoveFamily;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text myNameTMP = null;
        [SerializeField] private TMP_Text familyNameWaitTMP = null;
        [SerializeField] private TMP_Text familyNameTMP = null;
        [SerializeField] private TMP_Text familyStateTMP = null;
        [SerializeField] private GameObject emptyGO = null;
        [SerializeField] private GameObject waitGO = null;
        [SerializeField] private GameObject familyGO = null;
        [SerializeField] private Button addBT = null;
        [SerializeField] private Button cancelBT = null;
        [SerializeField] private Button removeBT = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        //[Header("★ Config")]
        //[SerializeField] private float buttonDelay = 0.5f;

        // Unity Messages
        private void Awake()
		{
            addBT.onClick.AddListener(async () =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                if (await UILauncher.One.FamilyAddPU.ShowPopup() == SimplePopupResult.Okay)
                {
                    cg.interactable = false;
                    if (await LMS.One.SaveFamily(UILauncher.One.FamilyAddPU.NickName))
                    {
                        familyNameWaitTMP.text = UserData.One.Parent.FamilyNickName;
                        familyStateTMP.text = LocalizationMGR.One.GetText(UserData.One.Parent.FamilyManager ? "WORD_145" : "WORD_103");
                        emptyGO.SetActive(false);
                        waitGO.SetActive(true);
                    }
                    cg.interactable = true;
                }
            });
            cancelBT.onClick.AddListener(async () =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                cg.interactable = false;
                if (await LMS.One.RemoveFamily(UserData.One.Parent.FamilyNickName))
                {
                    myNameTMP.text = UserData.One.Parent.NickName;
                    emptyGO.SetActive(true);
                    waitGO.SetActive(false);
                }
                cg.interactable = true;
            });
            removeBT.onClick.AddListener(async () =>
            {
                AudioMGR.One.PlayEffectUI(SfxMoment.Common_Click);
                if (await SystemUI.One.ShowPopupRemoveFamily())
                {
                    cg.interactable = false;
                    if (await LMS.One.RemoveFamily(UserData.One.Parent.FamilyNickName))
                    {
                        myNameTMP.text = UserData.One.Parent.NickName;
                        emptyGO.SetActive(true);
                        familyGO.SetActive(false);
                        OnRemoveFamily?.Invoke();
                    }
                    cg.interactable = true;
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