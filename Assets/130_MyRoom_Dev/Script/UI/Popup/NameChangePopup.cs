using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.UI;

namespace DoDoEng.MyRoom.UI
{
    public class NameChangePopup : PopupBase<SimplePopupResult>
	{
        // Definitions
        // Properties
        public string Name => nameTMP.text;

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(string name)
        {
            LOG.Function(this, $"| name={name}");

            AudioMGR.One.PlayEffect(popupCLIP);

            nameTMP.text = name;

            return await showPopup();
        }

        // Events
        //[HideInInspector] public event Action OnSignOut;



        // Fields : caching
        // Fields
        // Functions

        // Event Handlers
        private async UniTask okBT_OnClick()
        {
            LOG.Function(this);
            if (await LMS.One.ValidatePetName(nameTMP.text))
                CloseWithResult(SimplePopupResult.Yes);
            else
            {
                await SystemUI.One.ShowPopupNoValidatedPetName();
                nameTMP.Select();
            }
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_InputField nameTMP = null;
        [SerializeField] private Button okBT = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;
        //[Header("★ Config")]
        //[SerializeField] private float buttonDelay = 0.5f;

        // Unity Messages
        private void Awake()
		{
            okBT.onClick.AddListener(() => okBT_OnClick().Forget());
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