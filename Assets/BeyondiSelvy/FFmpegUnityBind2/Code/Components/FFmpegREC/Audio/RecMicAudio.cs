using BeyondiSelvy;
using UnityEngine;

namespace FFmpegUnityBind2.Components
{
    class RecMicAudio : MonoBehaviour, IRecAudio
    {
        [SerializeField]
        int maxLength = 60;

        AudioClip buffer;

        public void StartRecording()
        {
#if !UNITY_WEBGL
            // 2024-05-23 송앤챈트 녹음 안되는 문제 수정 (조성민)
            //buffer = Microphone.Start(null, false, maxLength, AudioSettings.outputSampleRate);
            buffer = Microphone.Start(null, false, maxLength, selvy.GetMicrophoneFrequency(44100));
#endif
        }

        public void StopRecording(string savePath)
        {
#if !UNITY_WEBGL
            Microphone.End(null);
            WAV.Save(savePath, buffer);
#endif
        }
    }
}