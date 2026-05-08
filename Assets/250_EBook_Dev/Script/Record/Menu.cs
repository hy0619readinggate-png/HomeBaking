using DG.Tweening;
using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Record
{
    public class Menu : MonoBehaviour
    {
        // Definitions
        public enum Buttons { NativeButton, RecordButton, MyVoiceButton }

        // Properties
        public bool EnableMyVoice
        {
            get => myVoiceBTN.interactable;
            set => myVoiceBTN.interactable = value;
        }

        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void SetProgress(int cNO, int cTOTAL, bool ani = true)
        {
            LOG.Info($"SetProgress() | {cNO} {cTOTAL}", this);

            var duration = ani ? progressDuration : 0;
            var ratio = cNO / (float)cTOTAL;
            DOTween.To(
                () => progressSLD.value,
                r =>
                {
                    progressSLD.value = r;
                    progressPercentTXT.text = $"{Mathf.Round(r * 100)}%";
                },
                ratio, duration);
        }
        public void SetNativePlaying(bool playing)
        {
            LOG.Info($"SetNativePlaying() | {playing}", this);

            nativeBTN.gameObject.SetActive(!playing);
            nativePlayingGO.SetActive(playing);
        }
        public void SetMyVoicePlaying(bool playing)
        {
            LOG.Info($"SetMyVoicePlaying() | {playing}", this);

            myVoiceBTN.gameObject.SetActive(!playing);
            myVoicePlayingGO.SetActive(playing);
        }

        // Events
        public event Action<Buttons> OnButtonClick;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= gameObject.AddComponent<CanvasGroup>();



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button nativeBTN = null;
        [SerializeField] private Button recordBTN = null;
        [SerializeField] private Button myVoiceBTN = null;
        [SerializeField] private Slider progressSLD = null;
        [SerializeField] private GameObject nativePlayingGO = null;
        [SerializeField] private GameObject myVoicePlayingGO = null;
        [SerializeField] private TextMeshProUGUI progressPercentTXT = null;
        [Header("★ Config")]
        [SerializeField] private float progressDuration = 0.2f;

        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;
            myVoiceBTN.interactable = false;

            nativePlayingGO.SetActive(false);
            myVoicePlayingGO.SetActive(false);

            nativeBTN.onClick.AddListener(() => OnButtonClick?.Invoke(Buttons.NativeButton));
            recordBTN.onClick.AddListener(() => OnButtonClick?.Invoke(Buttons.RecordButton));
            myVoiceBTN.onClick.AddListener(() => OnButtonClick?.Invoke(Buttons.MyVoiceButton));
        }
        private void Start()
        {
        }
    }
}