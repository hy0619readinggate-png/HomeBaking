using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.EBook.Quiz
{
    public class T2Question : MonoBehaviour
    {
        // Methods
        public void Setup(string question, AudioClip clip)
        {
            LOG.Info($"Setup() | {question}", this);

            questionClip = clip;
            questionTXT.text = question.Replace("\\n", "\n"); ;
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            listenBTN.interactable = enable;
            
            if (!enable) 
                stopNarration();
        }
        public Coroutine PlayNarration()
        {
            return playNarration();
        }
        public void StopNarration()
        {
            stopNarration();
        }



        // Fields
        private AudioClip questionClip = null;
        private Coroutine crNarration = null;

        // Functions
        private Coroutine playNarration()
        {
            if (crNarration != null)
                this.StopCoroutineSafe(ref crNarration);

            crNarration = StartCoroutine(coNarration());
            return crNarration;
        }
        private void stopNarration()
        {
            if (crNarration != null)
                this.StopCoroutineSafe(ref crNarration);
            
            AudioMGR.One.StopNarration();
            listenAniGO.SetActive(false);
        }


        // Event Handlers
        private void listenBTN_OnClick()
        {
            LOG.Info($"listenBTN_OnClick()", this);

            playNarration();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI questionTXT = null;
        [SerializeField] private Button listenBTN = null;
        [SerializeField] private GameObject listenAniGO = null;

        // Unity Messages
        private void Awake()
        {
            listenAniGO.SetActive(false);

            listenBTN.interactable = false;
            listenBTN.onClick.AddListener(listenBTN_OnClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coNarration()
        {
            listenAniGO.SetActive(true);
            yield return AudioMGR.One.PlayNarrationAndWait(questionClip);

            listenAniGO.SetActive(false);
        }
    }
}