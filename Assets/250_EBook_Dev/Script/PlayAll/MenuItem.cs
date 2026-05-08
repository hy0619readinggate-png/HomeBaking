using beyondi.Util;
using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.PlayAll
{
    [RequireComponent(typeof(Button))]
    public class MenuItem : MonoBehaviour, IID
    {
        // Properties
        public int ID { get; set; }

        // Methods
        public void Setup(MenuItemData data)
        {
            LOG.Function(this);

            this.ID = data.ID;

            thumnailIMG.sprite = data.ThumnailSPR;

            playPauseTGL.gameObject.SetActive(false);
        }
        public void SetCurrent(bool isCurrent, bool isPlaying)
        {
            LOG.Function(this, $"{isCurrent}");

            playPauseTGL.gameObject.SetActive(isCurrent);
            playPauseTGL.IsOn = isPlaying;

            btn.interactable = !isCurrent;
        }

        // Events
        public event Action<bool> OnPlayPauseChanged;
        public event Action<int> OnSelected;



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Event Handlers
        private void playPauseTGL_OnValueChanged(bool isOn)
        {
            LOG.Info($"playPauseTGL_OnValueChanged() | {isOn}", this);

            OnPlayPauseChanged?.Invoke(isOn);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image thumnailIMG = null;
        [SerializeField] private ToggleButton playPauseTGL = null;

        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(() => { OnSelected?.Invoke(ID); });
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            playPauseTGL.OnValueChanged += playPauseTGL_OnValueChanged;
        }
        private void OnDisable()
        {
            playPauseTGL.OnValueChanged -= playPauseTGL_OnValueChanged;
        }
    }
}