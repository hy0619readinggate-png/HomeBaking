using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using DoDoEng.Behaviour;
using TMPro;
using DoDoEng.Common;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace DoDoEng.Library.UI
{
	public class LibraryEBookPlayerPopup : PopupBase<SimplePopupResult>
    {
		// Definitions

		// Properties
		public bool ActiveSelf => gameObject.activeSelf;
        public string[] SelectedCodes => slots.Where(slot => slot.Checked).Select(slot => slot.ContentIndex).ToArray();

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup(LibrarySlot[] parentSlots)
        {
            LOG.Function(this);

            AudioMGR.One.PlayEffect(popupCLIP);

            this.parentSlots = parentSlots;

            playBT.interactable = false;

            removeAllSlots();
            updateCount();
            for (int i = 0; i < parentSlots.Length; i++)
            {
                addSlot().InitEBookPlayer(parentSlots[i].EBookList, parentSlots[i].IsMyRecord);
            }

            return await showPopup();
        }



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();

        // Fields
        private LibrarySlot[] parentSlots;
        private List<LibrarySlot> slots = new List<LibrarySlot>();

        // Functions
        private void removeAllSlots()
        {
            slots.ForEach(slot => slot.OnClick -= slot_onClick);
            slots.Clear();
            foreach (var ch in slotsRT.GetChildren())
            {
                if (ch != slotPF.GetComponent<RectTransform>())
                    GameObject.Destroy(ch.gameObject);
            }
        }
        private LibrarySlot addSlot()
        {
            var slot = Instantiate(slotPF, slotsRT);
            slot.OnClick += slot_onClick;

            slots.Add(slot);

            return slot;
        }
        private void updateCount()
        {
            int count = slots.Count(slot => slot.Checked);
            playBT.interactable = count > 0;
            countTMP.text = LocalizationMGR.One.GetText("WORD_133", count);

            allTGL.SetIsOnWithoutNotify(slots.Count == count);
        }

        // Event Handlers
        private void slot_onClick(LibrarySlot slot)
        {
            LOG.Function(this, $"{slot}");

            slot.Checked = !slot.Checked;

            updateCount();
        }
        private void allTGL_onValueChanged(bool check)
        {
            LOG.Function(this, $"{check}");

            slots.ForEach(slot => slot.Checked = check);
            updateCount();
        }

        // Overrides
        protected override void onOpen()
        {
            base.onOpen();

            scrollBar.value = 1;
            allTGL.isOn = false;
        }
        protected override void onClose(SimplePopupResult result)
        {
            base.onClose(result);

            if (result != SimplePopupResult.Yes)
                gameObject.SetActive(false);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button playBT = null;
        [SerializeField] private TMP_Text countTMP = null;
        [SerializeField] private RectTransform slotsRT = null;
        [SerializeField] protected LibrarySlot slotPF = null;
        [SerializeField] private Scrollbar scrollBar = null;
        [SerializeField] private Toggle allTGL = null;

        [Header("★ Sound")]
        [SerializeField] private AudioClip popupCLIP = null;

        // Unity Messages
        private void Awake()
		{
            allTGL.onValueChanged.AddListener((check) => allTGL_onValueChanged(check));
        }
		private void Start()
		{
		}
        private void Update()
        {
            if (parentSlots  != null && parentSlots.Length == slots.Count)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].Thumbnail == null)
                        slots[i].Thumbnail = parentSlots[i].Thumbnail;
                }
            }
        }
        private void OnEnable()
        {
            cg.blocksRaycasts = true;
        }
        private void OnDisable()
        {
        }
    }
}