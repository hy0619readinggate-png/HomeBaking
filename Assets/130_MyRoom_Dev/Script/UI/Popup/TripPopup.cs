using beyondi.Util;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.MyRoom.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.MyRoom.UI.Popup
{
    public class TripPopup : MonoBehaviour
    {
        // Definitions
        public const int Max_Count = 2;

        // Properties
        public int[] SelectedIndice
        {
            get
            {
                var list = new List<int>();
                foreach (var (slot, i) in slots.Select((v, i) => (v, i)))
                {
                    if (slot.Checked)
                        list.Add(i);
                }
                return list.ToArray();
            }
        }

        // Methods
        public void Activate(bool active)
        {
            LOG.Info($"Activate() | {active}", this);

            gameObject.SetActive(active);
        }
        public void Show(int idxPet = -1)
        {
            if (!gameObject.activeSelf)
            {
                Activate(true);

                foreach (var (slot, i) in slots.Select((v, i) => (v, i)))
                {
                    if (i < UserData.One.Pets.Count)
                    {
                        slot.Activate(true);
                        slot.Init(i, false, true);
                    }
                    else
                    {
                        slot.Activate(false);
                    }
                }

                tripBT.interactable = false;
            }


            if (idxPet != -1)
            {
                if (addPet(slots[idxPet]))
                {
                    float dest = idxPet / (float)(UserData.One.Pets.Count - 1);
                    //scrollRect.horizontalScrollbar.value = dest;
                    DOTween.To(() => scrollRect.horizontalScrollbar.value, v => scrollRect.horizontalScrollbar.value = v, dest, 0.5f);
                }
            }
            else
            {
                scrollRect.horizontalScrollbar.value = 0.0f;
            }
        }

        // Events
        public Action<CollectionSlot> OnChecked;
        public Action OnClose;
        public Action OnTrip;



        // Fields : caching
        // Fields

        // Functions
        private bool addPet(CollectionSlot slot)
        {
            bool result = false;
            int count = slots.Count(slot => slot.Checked);
            if (!slot.Checked && Max_Count <= count)
                AudioMGR.One.PlayEffect(failCLIP);
            else
            {
                slot.Check(!slot.Checked);
                OnChecked?.Invoke(slot);
                result = true;
            }

            tripBT.interactable = slots.Any(slot => slot.Checked);

            return result;
        }
        private async UniTask goTrip()
        {
            if (await SystemUI.One.ShowPopupMyRoomTrip1())
            {
                if (await SystemUI.One.ShowPopupMyRoomTrip2())
                {
                    Activate(false);
                    OnTrip?.Invoke();
                }
            }
        }

        // Event Handlers
        private void slot_onClick(CollectionSlot slot)
        {
            LOG.Info($"slot_onClick() | {slot}", this);

            addPet(slot);
        }

        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button closeBT = null;
        [SerializeField] private Button tripBT = null;
        [SerializeField] private CollectionSlot[] slots = null;
        [SerializeField] private ScrollRect scrollRect = null;
        [Header("★ Sound")]
        [SerializeField] private AudioClip failCLIP = null;

        // Unity Messages
        private void Awake()
		{
            closeBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
                Activate(false);
                OnClose?.Invoke();
            });
            tripBT.onClick.AddListener(() =>
            {
                AudioMGR.One.PlayEffect(SfxMoment.Common_Click);
                goTrip().Forget();
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