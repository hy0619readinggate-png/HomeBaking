using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Activity.C3_A06
{
    [RequireComponent(typeof(VoiceRecognizer))]
    public class RecordMGR : MonoBehaviour
    {
        // Properties
        public MyButton RecordBTN => recordBTN;
        public MyButton MyVoiceBTN => myVoiceBTN;
        public bool RecordedCLIP => recordedCLIP;
        public bool IsRecorded => recordedCLIP != null && recognizer.IsSuccess(recognizeCutOffResultScore);
        public int Score => (int)recognizer.ResultScore;

        // Methods
        public void Setup(ProblemData pData)
        {
            LOG.Info($"Setup()", this);

            this.pData = pData;

            clearData();
            enableButtons(false);
            updateState();
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            enableButtons(enable);
            updateState();
        }
        public Coroutine StartRecord()
        {
            LOG.Info($"StartRecord()", this);

            crRecord = StartCoroutine(coRecord());
            return crRecord;
        }
        public void FinishRecord()
        {
            LOG.Info($"FinishRecord()", this);

            this.StopCoroutineSafe(ref crRecord);

            recognizer.StopRecognize();
        }
        public Coroutine StartPlayMyVoice()
        {
            LOG.Info($"StartPlayMyVoice()", this);

            myVoiceAnim.SetTrigger("Play");

            crPlayMyVoice = StartCoroutine(coPlayMyVoice());
            return crPlayMyVoice;
        }
        public void FinishPlayMyVoice()
        {
            LOG.Info($"FinishPlayMyVoice()", this);

            myVoiceAnim.SetTrigger("Idle");

            this.StopCoroutineSafe(ref crPlayMyVoice);

            AudioMGR.One.StopNarration();
        }



        // Fields : caching
        private VoiceRecognizer recognizer_ = null;
        private VoiceRecognizer recognizer => recognizer_ ??= GetComponent<VoiceRecognizer>();



        // Fields
        private ProblemData pData = null;
        private Coroutine crRecord = null;
        private Coroutine crPlayMyVoice = null;
        private AudioClip recordedCLIP = null;

        // Functions
        private void enableButtons(bool enable)
        {
            recordBTN.EnableInteraction(enable);
            myVoiceBTN.EnableInteraction(enable && IsRecorded);
        }
        private void updateState()
        {
            myVoiceBTN.Activate(IsRecorded);
            recordBTNGlowGO.SetActive(recordBTN.Enabled && !IsRecorded);
        }
        private void clearData()
        {
            recordedCLIP = null;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private MyButton myVoiceBTN = null;
        [SerializeField] private Animator myVoiceAnim = null;
        [SerializeField] private MyButton recordBTN = null;
        [SerializeField] private GameObject recordBTNGlowGO = null;
        [SerializeField] private Animator recordAnim = null;
        [SerializeField] private GameObject feedbackVFX = null;
        [Header("★ Config")]
        [SerializeField] private float recordDelay = 1f;
        [SerializeField] private int recognizeCutOffResultScore = 30;

        // Unity Messages
        private void Awake()
        {
            RecordBTN.Activate(true);
            feedbackVFX.SetActive(false);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coRecord()
        {
            using (LOG.Coroutine($"coRecord()", this))
            {
                recognizer.Reset();
                recordedCLIP = null;
                yield return null;

                recordAnim.SetTrigger("Record");
                yield return null;

                var duration = (int)Math.Ceiling(pData.SentenceCLIP.length + recordDelay);

                yield return recognizer.StartRecognize(pData.SentenceCLIP, pData.Sentence, duration);

                if (recognizer.IsRecognized)
                {
                    recordedCLIP = recognizer.RecordedClip;
                    yield return null;
                }

                recordAnim.SetTrigger("Idle");
                yield return null;
            }
        }
        IEnumerator coPlayMyVoice()
        {
            using (LOG.Coroutine($"coPlayMyVoice()", this))
            {
                if (recordedCLIP != null)
                    yield return AudioMGR.One.PlayMyVocieAndWait(recordedCLIP);
                yield return null;
            }
        }
    }
}