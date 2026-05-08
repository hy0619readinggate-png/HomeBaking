using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DoDoEng.MyRoom.Behavior;
using Cysharp.Threading.Tasks;
using System.Reflection;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.MyRoom.UI.Popup
{
	public class CollectionPopup : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Show()
        {
            Activate(true);

            selectedNameTMP.text = string.Empty;

            foreach (var (slot, i) in slots.Select((v, i) => (v, i)))
            {
                slot.Init(i, false, true);
            }

            checkedSlot = null;
            selectedSlot.Init();
            nameEditBT.gameObject.SetActive(false);

            goBT.enabled = false;

            scrollbar.value = 1.0f;
        }

        // Events
        public Action<int> OnChangeName;
        public Action<int> OnGoToPet;
        public Action<int> OnTrip;



        // Fields : caching

        // Fields
        private CollectionSlot checkedSlot = null;

        // Functions
        private async UniTask changeName()
        {
            var data = selectedSlot.Pet.Data;
            if (data == null) return;
            if (await UIMyRoom.One.NameChangePU.ShowPopup(data.Name) == SimplePopupResult.Yes)
            {
                if (await LMS.One.ChangeMyPet(data.ID, UIMyRoom.One.NameChangePU.Name, data.Affection))
                {
                    data.Name = UIMyRoom.One.NameChangePU.Name;
                    checkedSlot.Init(selectedSlot.Index, false, true);
                    checkedSlot.Check(true);
                    selectedNameTMP.text = data.Name;
                }
                else
                    await SystemUI.One.ErrorPU.ShowPopup("Failed to save pet name.");
            }
        }

        // Event Handlers
        private void slot_onClick(CollectionSlot slot)
        {
            LOG.Function(this, $"{slot}");

            if (slot.Index < UserData.One.Pets.Count)
            {
                AudioMGR.One.PlayEffect(SfxMoment.Common_Click);

                slots.ForEach(s =>
                {
                    s.Check(s == slot);
                });

                checkedSlot = slot;
                selectedSlot.Init(slot.Index, true);
                if (slot.Index < UserData.One.Pets.Count)
                {
                    selectedNameTMP.text = UserData.One.Pets[slot.Index].Name;
                    goBT.enabled = true;
                    nameEditBT.gameObject.SetActive(true);
                }
                else
                {
                    selectedNameTMP.text = string.Empty;
                    goBT.enabled = false;
                    nameEditBT.gameObject.SetActive(false);
                }
            }
        }
        private void nameEditBT_onClick()
        {
            changeName().Forget();
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
		[SerializeField] private Button closeBT = null;
        [SerializeField] private CollectionSlot selectedSlot = null;
		[SerializeField] private TMP_Text selectedNameTMP = null;
		[SerializeField] private Button nameEditBT = null;
		[SerializeField] private Button goBT = null;
		[SerializeField] private Button tripBT = null;
        [SerializeField] private CollectionSlot[] slots = null;
        [SerializeField] private Scrollbar scrollbar = null;

        // Unity Messages
        private void Awake()
		{
            closeBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
                UserData.One.Pets.ForEach(data => data.New = false);
                Activate(false);
            });

            nameEditBT.onClick.AddListener(() => nameEditBT_onClick());

            goBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
                Activate(false);
                OnGoToPet?.Invoke(selectedSlot.Index);
            });
            tripBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
                Activate(false);
                OnTrip?.Invoke(selectedSlot.Index);
            });
        }
		private void Start()
		{
		}
        protected void OnEnable()
        {
            slots.ForEach(slot => slot.OnClick += slot_onClick);
        }
        protected void OnDisable()
        {
            slots.ForEach(slot => slot.OnClick -= slot_onClick);
        }

        // Unity Coroutine
	}
}