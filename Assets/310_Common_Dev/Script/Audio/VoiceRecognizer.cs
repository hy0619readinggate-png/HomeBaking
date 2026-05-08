using beyondi.Util;
using BeyondiSelvy;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections;
using System.IO;
using UnityEngine;

namespace DoDoEng
{
    public enum RecognizeResult
    {
        Fail = -1,
        None = 0,
        Success = 1,
        Rejest = 2,
        EPDFail = 3,
        TimeOver = 4,
        FrameError = 5,
        DeviceError = 6
    }

    public class VoiceRecognizer : MonoBehaviour
    {
        // Properties
        public RecognizeResult RecognizeResult => recognizeResult;
        public bool IsRecognized => RecordedClip != null;
        public float ResultScore => resultScore;
        public AudioClip RecordedClip { get; private set; } = null;

        // Methods
        public void Reset()
        {
            LOG.Info($"Reset()", this);

            recognizeResult = RecognizeResult.None;
            resultScore = -1;
            RecordedClip = null;
        }
        public Coroutine StartRecognize(AudioClip nativeClip, string sentence, int duration = 10, int frequency = 44100)
        {
            LOG.Info($"StartRecognize() | {sentence} | {duration} | {frequency}", this);

            stopRecognize();

            crRecognize = StartCoroutine(coRecognize(nativeClip, sentence, duration, frequency));
            return crRecognize;
        }
        public void StopRecognize()
        {
            LOG.Info($"StopRecognize()", this);

            stopRecognize();
        }
        public bool IsSuccess(int cutOffResultSocre)
        {
            LOG.Function(this, $"{resultScore} / {cutOffResultSocre}");

            return recognizeResult == RecognizeResult.Success
                && resultScore >= cutOffResultSocre;
        }



        // Fields : caching
        private Dispatcher dispatcher_ = null;
        private Dispatcher dispatcher => dispatcher_ ??= gameObject.AddComponent<Dispatcher>();

        // Fields
        private RecognizeResult recognizeResult;
        private float resultScore = 0;
        private Coroutine crRecognize = null;

        // Functions
        private void stopRecognize()
        {
            this.StopCoroutineSafe(ref crRecognize);

#if !UNITY_WEBGL
            Microphone.End(null);
#endif
        }
        private void clear()
        {
            recognizeResult = RecognizeResult.None;
            resultScore = -1;
            RecordedClip = null;
        }



        // Unity Messages
        private void Awake()
        {
            if (dispatcher == null)
                LOG.Info($"{dispatcher}", this);
        }
        private void Start()
        {
        }

        // Unity Coroutine
        IEnumerator coRecognize(AudioClip nativeClip, string sentence, int duration = 10, int frequency = 44100)
        {
            using (LOG.Coroutine($"coRecognize() | {sentence} | {duration} sec | {frequency}", this))
            {
                clear();
                yield return coRecord(duration, frequency);
                yield return recognize(nativeClip, sentence).ToCoroutine();

                LOG.Info($"resultScore - {recognizeResult} , {resultScore}", this);
            }
        }
        private async UniTask recognize(AudioClip nativeClip, string sentence)
        {
            LOG.Info($"recognize()", this);

            var tempDir = Path.Combine(Application.persistentDataPath, "Temp");
            var nativeClipPath = UtilMedia.MakeClipToWav(nativeClip, Path.Combine(tempDir, "native.wav"));
            var recordedClipPath = UtilMedia.MakeClipToWav(RecordedClip, Path.Combine(tempDir, "rec.wav"));
            var result = await EduTem.V2.Assessment(sentence, nativeClipPath, recordedClipPath, null);

            LOG.Info($"recognize() result - {result}", this);
            resultScore = result.total_score;
            recognizeResult = result.speech_detected ? RecognizeResult.Success : RecognizeResult.Fail;

            switch (result.status)
            {
                case 200:
                    break;
                case 403: // Forbidden, 접근금지, POPUP_35
                    await SystemUI.One.ShowPopupEdutemAccessDenided();
                    break;
                case 408: // Timeout, 응답 대기 시간 초과, POPUP_36
                    await SystemUI.One.ShowPopupEdutemTimeout();
                    break;
                case 422: // Validation, 입력값 오류, POPUP_37
                    await SystemUI.One.ShowPopupEdutemWrongInput();
                    break;
                default: // 기타오류, POPUP_38
                    await SystemUI.One.ShowPopupEdutemError();
                    break;
            }
        }
        IEnumerator coRecord(int duration = 10, int frequency = 44100)
        {
            using (LOG.Coroutine($"coRecord()", this))
            {
#if !UNITY_WEBGL
                if (Microphone.devices.Length == 0)
                {
                    LOG.Important($"Microphone is Not Connected!", this);

                    recognizeResult = RecognizeResult.DeviceError;
                    yield break;
                }

                RecordedClip = Microphone.Start(null, false, duration, selvy.GetMicrophoneFrequency(frequency)); // CHANGE: PARK JIN HYUNG, 2024-02-06

                yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
                yield return new WaitUntil(() => Microphone.IsRecording(null));
                yield return new WaitUntil(() => !Microphone.IsRecording(null));

                Microphone.End(null);
                yield return null;
#else
                LOG.Important($"Microphone is not supported on WebGL.", this);
                recognizeResult = RecognizeResult.DeviceError;
                yield break;
#endif
            }
        }
    }
}