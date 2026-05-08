using beyondi.Util;
using BeyondiSelvy;
using System.Collections;
using UnityEngine;

namespace DoDoEng.Common
{
    public class VoiceRecorder : MonoBehaviour
    {
        // Properties
        public static string MicrophoneName { get; set; } = null;
        public static int SampleFrequency { get; set; } = 44100;

        // Properties
        public bool CanRecord { get; private set; } = false;
        public AudioClip RecordedClip { get; private set; } = null;
        public bool IsRecording { get; private set; } = false;



        // Methods
        public void CheckMicrophone()
        {
            LOG.Info($"CheckMicrophone()", this);

            CanRecord = false;

#if !UNITY_WEBGL
            foreach (var d in Microphone.devices)
                LOG.Info($"[MIC] {d}", this);

            if (Microphone.devices.Length == 0)
                throw new System.Exception("No microphone is exists.");

            if (MicrophoneName != null &&
                !Microphone.devices.Contain(MicrophoneName))
                throw new System.Exception($"{MicrophoneName} is not connected");

            CanRecord = true;
#else
            LOG.Important($"Microphone is not supported on WebGL.", this);
#endif
        }
        public Coroutine StartRecord(int duration)
        {
            LOG.Info($"StartRecord() | {duration}", this);

            crRecord = StartCoroutine(coRecord(duration));
            return crRecord;
        }
        public void StopRecord()
        {
            LOG.Info($"StopRecord()", this);

            if (IsRecording)
            {
                IsRecording = false;

#if !UNITY_WEBGL
                Microphone.End(null);
#endif
                this.StopCoroutineSafe(ref crRecord);
            }
        }



        // Fields
        private Coroutine crRecord = null;



        // Unity Messages
        private void Awake()
        {
            try { CheckMicrophone(); }
            catch { }
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coRecord(int duration)
        {
            using (LOG.Coroutine($"coRecord() | {duration}", this))
            {
#if !UNITY_WEBGL
                if (Microphone.devices.Length == 0)
                {
                    LOG.Error($"Microphone is Not Connected!", this);
                    yield break;
                }

                IsRecording = true;
                //RecordedClip = Microphone.Start(null, false, duration, 44100);
                RecordedClip = Microphone.Start(null, false, duration, selvy.GetMicrophoneFrequency(44100)); // CHANGE: PARK JIN HYUNG, 2024-02-06
                yield return new WaitForSeconds(duration);

                Microphone.End(null);
                yield return null;

                IsRecording = false;
#else
                LOG.Important($"Microphone is not supported on WebGL.", this);
                yield break;
#endif
            }
        }
    }
}