using beyondi.Util;
using DoDoEng.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng
{
    [RequireComponent(typeof(VoiceRecognizer))]
    public class RecordTester : MonoBehaviour
    {
        // Fields : caching
        private VoiceRecognizer recognizer_ = null;
        private VoiceRecognizer recognizer => recognizer_ ??= GetComponent<VoiceRecognizer>();

        // Fields
        private Coroutine crRecognize = null;

        // Functions
        private void stopRecognize()
        {
            recognizer.StopRecognize();
            this.StopCoroutineSafe(ref crRecognize);
        }
        // Event Handlers
        private void startRecognizebtn_onClick()
        {
            LOG.Info($"startRecognizebtn_onClick()", this);

            stopRecognize();
            crRecognize = StartCoroutine(coRecognize());
        }
        private void stopRecognizebtn_onClick()
        {
            LOG.Info($"stopRecognizebtn_onClick()", this);

            stopRecognize();
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Button startRecognizeBTN = null;
        [SerializeField] private Button stopRecognizeBTN = null;
        [Header("★ Config")]
        [SerializeField] private string sentence = "hello, I am dodo.";
        [SerializeField] private int cutOff = 50;
        [SerializeField] private AudioClip sampleCLIP = null;
        [SerializeField] private float recordDelay = 2;

        // Unity Messages
        private void Awake()
        {
            startRecognizeBTN.onClick.AddListener(startRecognizebtn_onClick);
            stopRecognizeBTN.onClick.AddListener(stopRecognizebtn_onClick);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coRecognize()
        {
            var duration = (int)Math.Ceiling(sampleCLIP.length + recordDelay);

            yield return recognizer.StartRecognize(sampleCLIP, sentence, duration);

            LOG.Info($"{recognizer.RecognizeResult} | {recognizer.IsSuccess(cutOff)}", this);
        }
    }
}