using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json;
using FFmpegUnityBind2;

namespace BeyondiSelvy
{
    public class selvy
    {
#if UNITY_IOS
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string iOS_Recognition(string sentence, string recPath, string dbPath);
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern string iOS_Assessment(string sentence, string orgPath, string recPath, string dbPath);
#endif

        public static IFFmpegCallbacksHandler[] Handler(Action<int, long, string> cb)
        {
            return new IFFmpegCallbacksHandler[] { new selvyHandler(cb) };
        }

        public static int GetMicrophoneFrequency(int frequency = 44100, string name = null)
        {
#if UNITY_WEBGL
            return frequency;
#else
            int freq;
            Microphone.GetDeviceCaps(name, out int minFreq, out int maxFreq);
            if (minFreq == 0 && maxFreq == 0)
                freq = frequency;
            else
            if (frequency >= minFreq && frequency <= maxFreq)
                freq = frequency;
            else
                freq = maxFreq;
            Debug.Log($"Microphone.frequency: min:{minFreq} - max:{maxFreq}, set:{frequency} => {freq}");
            return freq;
#endif
        }

        public static FFmpegProcess Execute(string command, params IFFmpegCallbacksHandler[] callbacksHandlers)
        {
            return FFmpeg.Execute(command, callbacksHandlers);
        }


        static bool IsAudioFile(string src)
        {
            string exe = Path.GetExtension(src).ToUpper();
            return exe.Equals(".MP3") || exe.Equals(".WAV");
        }
        static bool IsPcmFile(string src)
        {
            return Path.GetExtension(src).ToUpper().Equals(".PCM");
        }
        static bool IsVideoFile(string src)
        {
            return Path.GetExtension(src).ToUpper().Equals(".MP4");
        }

        // mp3, wav -> pcm: 16k, mono, 16bit
        public static void makePCM(string src, string pcmDis, params IFFmpegCallbacksHandler[] callbacksHandlers)
        {
            if (!IsAudioFile(src))
            {
                Debug.LogError($"selvy.makePCM: 소스파일({src})이 Audio 파일이 아닙니다.");
                return;
            }
            if (!IsPcmFile(pcmDis))
            {
                Debug.LogError($"selvy.makePCM: 타겟파일({pcmDis})이 pcm 파일이 아닙니다.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(pcmDis)));
            string command = $"-loglevel error -nostats -y -channel_layout mono -i {src} -ar 16000 -ac 1 -f s16le -acodec pcm_s16le {pcmDis}";
            Execute(command, callbacksHandlers);
        }

        // mp4 -> mp3
        public static void extractionAudio(string src, string dis, params IFFmpegCallbacksHandler[] callbacksHandlers)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(dis)));
            //string command = $"-loglevel error -nostats -y -i {src} -vn -acodec libmp3lame {dis}";
            string command = $"-loglevel error -nostats -y -i {src} {dis}";
            Execute(command, callbacksHandlers);
        }

        // mp4 -> mp4: audio 없는
        public static void removeAudio(string mp4Src, string mp4Dis, params IFFmpegCallbacksHandler[] callbacksHandlers)
        {
            if (!IsVideoFile(mp4Src))
            {
                Debug.LogError($"selvy.extractionAudio: 소스파일({mp4Src})이 mp4 파일이 아닙니다.");
                return;
            }
            if (!IsVideoFile(mp4Dis))
            {
                Debug.LogError($"selvy.extractionAudio: 타겟파일({mp4Dis})이 mp4 파일이 아닙니다.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(mp4Dis)));
            string command = $"-loglevel error -nostats -y -i {mp4Src} -an {mp4Dis}";
            Execute(command, callbacksHandlers);
        }

        // mp3 -> mp3: ss(hh:mm:ss.nnn) 에서 to(hh:mm:ss.nnn) 까지 자른
        public static void cropAudio(string mp3Src, string mp3Dis, string ss, string to, params IFFmpegCallbacksHandler[] callbacksHandlers)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(mp3Dis)));
            string command = $"-loglevel error -nostats -y -i {mp3Src} -ss {ss} -to {to} {mp3Dis}";
            Execute(command, callbacksHandlers);
        }

        // mp3 -> mp3: ss(hh:mm:ss.nnn) 부터 끝까지 자르거나 처음부터 ss 까지 자르거나
        public static void cropAudio(string mp3Src, string mp3Dis, string ss, params IFFmpegCallbacksHandler[] callbacksHandlers)
        {
            if (!IsAudioFile(mp3Src))
            {
                Debug.LogError($"selvy.extractionAudio: 소스파일({mp3Src})이 Audio 파일이 아닙니다.");
                return;
            }
            if (!IsAudioFile(mp3Dis))
            {
                Debug.LogError($"selvy.extractionAudio: 타겟파일({mp3Dis})이 Audio 파일이 아닙니다.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(mp3Dis)));
            string command = $"-loglevel error -nostats -y -i {mp3Src} -ss {ss} {mp3Dis}";
            if (ss[0] == '-')
            {
                string to = ss.Substring(1);
                command = $"-loglevel error -nostats -y -i {mp3Src} -to {to} {mp3Dis}";
            }
            Execute(command, callbacksHandlers);
        }

        // mp3 + mp4 -> mp4: mp4 에 오디오를 믹스합니다. - BGM 있는 비디오에 voice 파일을 넣을 때 사용
        public static void audioComplexVideo(string mp3Src, string mp4Src, string mp4Dis, params IFFmpegCallbacksHandler[] callbacksHandlers)
        {
            if (!IsAudioFile(mp3Src))
            {
                Debug.LogError($"selvy.extractionAudio: 소스파일({mp3Src})이 Audio 파일이 아닙니다.");
                return;
            }
            if (!IsVideoFile(mp4Src))
            {
                Debug.LogError($"selvy.extractionAudio: 소스파일({mp4Src})이 mp4 파일이 아닙니다.");
                return;
            }
            if (!IsVideoFile(mp4Dis))
            {
                Debug.LogError($"selvy.extractionAudio: 타겟파일({mp4Dis})이 mp4 파일이 아닙니다.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(mp4Dis)));
            string command = $"-loglevel error -nostats -y -i {mp3Src} -i {mp4Src} -filter_complex amerge {mp4Dis}";
            Execute(command, callbacksHandlers);
        }

        // mp3 + mp4 -> mp4: mp4 오디오를 갈아 끼웁니다.
        // ffmpeg -i v.mp4 -i a.wav -c:v copy -map 0:v:0 -map 1:a:0 new.mp4
        // ffmpeg -i v.mp4 -i a.wav -acodec copy -vcodec copy -map 0:v:0 -map 1:a:0 new.mp4
        public static void audioChangeVideo(string mp4Src, string mp3Src, string mp4Dis, params IFFmpegCallbacksHandler[] callbacksHandlers)
        {
            if (!IsVideoFile(mp4Src))
            {
                Debug.LogError($"selvy.extractionAudio: 소스파일({mp4Src})이 mp4 파일이 아닙니다.");
                return;
            }
            if (!IsAudioFile(mp3Src))
            {
                Debug.LogError($"selvy.extractionAudio: 소스파일({mp3Src})이 AUDIO 파일이 아닙니다.");
                return;
            }
            if (!IsVideoFile(mp4Dis))
            {
                Debug.LogError($"selvy.extractionAudio: 타겟파일({mp4Dis})이 mp4 파일이 아닙니다.");
                return;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(mp4Dis)));
            string command = $"-loglevel error -nostats -y -i {mp4Src} -i {mp3Src} -c:v copy -map 0:v:0 -map 1:a:0 {mp4Dis}";
            Execute(command, callbacksHandlers);
        }

        static void getMaxVolume(string src, Action<float> cb)
        {
#if UNITY_EDITOR
            string command = $"-nostats -i {src} -af \"volumedetect\" -vn -sn -dn -f null NUL";
#else
            string command = $"-nostats -i {src} -af \"volumedetect\" -vn -sn -dn -f null /dev/null";
#endif
            float maxVolume = 0;
            Execute(command, selvy.Handler((type, id, message) =>
            {
                switch (type)
                {
                    case 2: // SUCCESS
                    case 3: // CANCEL
                    case 4: // FAIL
                        Debug.Log($"selvy.Normalize: {maxVolume}");
                        cb(maxVolume);
                        break;
                    case 5: // LOG
                        {
                            int n = message.IndexOf("max_volume:");
                            if (n < 0) return;
                            string volume = message.Substring(n);
                            volume = Regex.Replace(volume, @"[^0-9.-]", "");
                            maxVolume = float.Parse(volume);
                        }
                        break;
                }
            }));
        }

        // 오디오 정규화 및 노이즈 제거
        // 필터 패스 지정하는 방법을 정확히 찾지 못해서 노이즈 제거는 일단 보류, 노이즈 필터를 사용하면 변환속도는 느려지나 확실히 잡음이 줄어든 깨끗한 음성을 얻을 수 있음.
        public static void Normalize(string recWavPath, Action<int, string> cb)
        {
            Debug.Log($"Normalize: {recWavPath}");

            string dir = Path.GetDirectoryName(Path.GetFullPath(recWavPath));
            string fname = Path.GetFileNameWithoutExtension(recWavPath);
            string fext = Path.GetExtension(recWavPath);
            fname = $"{fname}-nor{fext}";
            string output = Path.Combine(dir, fname);

            /*
            Debug.Log($"FILTER PATH: A");
            string filterAbsolutePath = $"{Application.streamingAssetsPath}/Selvy/filter/sh.rnnn";
            string currPath = Path.GetFullPath("./");
            Debug.Log($"FILTER PATH: B, {filterAbsolutePath}, {currPath}");
            string filterRelativePath = filterAbsolutePath.Substring(currPath.Length);
            Debug.Log($"FILTER PATH: {filterAbsolutePath}, {filterRelativePath}");
            */

            getMaxVolume(recWavPath, (dB) =>
            {
                dB *= -1;
                //string option = $"-loglevel error -nostats -y -i {src} -af \"volume={dB}dB, arnndn=m={filterAbsolutePath}\" {output}"; // 노이즈 필터의 위치 지정 방식이 시스템별로 다른데 안드로이드일 경우 전체가 jar 파일로 묶여있는 형태여서 상대적 위치로 지정하거나 절대경로로 지정하는 방법을 찾지 못하였음.
                string option = $"-loglevel error -nostats -y -i {recWavPath} -af \"volume={dB}dB\" {output}";
                //string option = $"-loglevel error -nostats -y -i {src} -af \"loudnorm\" {output}"; // 특별히 더 나은 결과를 얻지 못함!.. 추후 R&D 가능성 있음.
                Execute(option, selvy.Handler((type, id, message) =>
                {
                    switch (type)
                    {
                        case 2:
                            cb(type, output);
                            break;
                        case 3:
                        case 4:
                            cb(type, message);
                            break;
                    }
                }));
            });
        }

        // 원음의 볼륨에 맞추어서 녹음 볼륨을 조정
        public static void Normalize(string orgWavPath, string recWavPath, Action<int, string> cb)
        {
            Debug.Log($"Normalize: {orgWavPath}, {recWavPath}");

            string dir = Path.GetDirectoryName(Path.GetFullPath(recWavPath));
            string fname = Path.GetFileNameWithoutExtension(recWavPath);
            string fext = Path.GetExtension(recWavPath);
            fname = $"{fname}-normal{fext}";
            string output = Path.Combine(dir, fname);

            getMaxVolume(orgWavPath, (orgVolime) =>
            {
                getMaxVolume(recWavPath, (recVolime) =>
                {
                    float dB = (recVolime - orgVolime) * -1;
                    string option = $"-loglevel error -nostats -y -i {recWavPath} -af \"volume={dB}dB\" {output}";
                    Execute(option, selvy.Handler((type, id, message) =>
                    {
                        switch (type)
                        {
                            case 2:
                                cb(type, output);
                                break;
                            case 3:
                            case 4:
                                cb(type, message);
                                break;
                        }
                    }));
                });
            });
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        static string MakeClipToWav(AudioClip clip, string filePath)
        {
            float[] floats = new float[clip.samples * clip.channels];
            clip.GetData(floats, 0);
            byte[] bytes = new byte[floats.Length * 2];
            for (int ii = 0; ii < floats.Length; ii++)
            {
                short uint16 = (short)(floats[ii] * short.MaxValue);
                byte[] vs = BitConverter.GetBytes(uint16);
                bytes[ii * 2] = vs[0];
                bytes[ii * 2 + 1] = vs[1];
            }

            byte[] wav = new byte[44 + bytes.Length];
            byte[] header = {
                0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00,
                0x57, 0x41, 0x56, 0x45, 0x66, 0x6D, 0x74, 0x20,
                0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61
            };

            Buffer.BlockCopy(header, 0, wav, 0, header.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(36 + bytes.Length), 0, wav, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.channels), 0, wav, 22, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency), 0, wav, 24, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.frequency * clip.channels * 2), 0, wav, 28, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(clip.channels * 2), 0, wav, 32, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, wav, 40, 4);
            Buffer.BlockCopy(bytes, 0, wav, 44, bytes.Length);

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)));
            File.WriteAllBytes(filePath, wav);

            return filePath;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        public const int passScore = 50; // 맨티스 0003213 에 따라 기존 70 => 50 으로 변경;
        static AssessmentResult GetAssessmentResult(Assessment A)
        {
            int min = Math.Min(Math.Min(Math.Min(A.pronunciation_score, A.timing_score), A.intonation_score), A.loudness_score);

            AssessmentResult ass = new AssessmentResult
            {
                success = A.overall >= passScore,
                score = A.overall,
                weak_score = min,
                prosody_score = A.prosody_score,
                pronunciation_score = A.pronunciation_score,
                timing_score = A.timing_score,
                intonation_score = A.intonation_score,
                loudness_score = A.loudness_score,
                sentence = A.sentence
            };

            if (A.overall >= 90)
            {
                ass.weak_grade = (min >= 74) ? "A" : "B";
                if (min > 90)
                {
                    ass.weak = "none";
                    ass.weak_grade = "S";
                    ass.message = "너무 훌륭한 스피킹 실력이네요!";
                }
                else if (min == A.pronunciation_score)
                {
                    ass.weak = "pronunciation";
                    ass.message = (min >= 74) ? "발음을 잘 따라 했어요!" : "전반적으로 좋은데, 발음이 살짝 아쉬워요. 원음의 발음에 집중해서 연습하면 더 훌륭하겠어요!";
                }
                else if (min == A.intonation_score)
                {
                    ass.weak = "intonation";
                    ass.message = (min >= 74) ? "억양을 잘 따라 했어요!" : "전반적으로 좋은데, 억양 살짝 아쉬워요. 원음의 억양에 집중해서 연습하면 더 훌륭하겠어요!";
                }
                else if (min == A.timing_score)
                {
                    ass.weak = "timing";
                    ass.message = (min >= 74) ? "속도를 잘 따라 했어요!" : "전반적으로 좋은데, 속도가 살짝 아쉬워요. 원음의 속도에 집중해서 연습하면 더 훌륭하겠어요!";
                }
                else
                {
                    ass.weak = "loudness";
                    ass.message = (min >= 74) ? "강세를 잘 따라 했어요!" : "전반적으로 좋은데, 강세가 살짝 아쉬워요. 원음의 강세에 집중해서 연습하면 더 훌륭하겠어요!";
                }
            }
            else
            {
                ass.weak_grade = (min >= 90) ? "C" : (min >= 74) ? "D" : (min >= 20) ? "E" : "F";
                if (min == A.pronunciation_score)
                {
                    ass.weak = "pronunciation";
                    ass.message = (min >= 90) ? "발음을 대부분 잘 따라 했어요!" : (min >= 74) ? "발음이 원음과 유사합니다." : (min >= 20) ? "발음이 원음과 달라요!" : "발음을 평가할 수 없습니다.";
                }
                else if (min == A.intonation_score)
                {
                    ass.weak = "intonation";
                    ass.message = (min >= 90) ? "억양을 대부분 잘 따라 했어요!" : (min >= 74) ? "억양이 원음과 유사합니다." : (min >= 20) ? "억양이 원음과 달라요!" : "억양을 평가할 수 없습니다.";
                }
                else if (min == A.timing_score)
                {
                    ass.weak = "timing";
                    ass.message = (min >= 90) ? "속도를 대부분 잘 따라 했어요!" : (min >= 74) ? "속도가 원음과 유사합니다." : (min >= 20) ? "속도가 원음과 달라요!" : "속도를 평가할 수 없습니다.";
                }
                else
                {
                    ass.weak = "loudness";
                    ass.message = (min >= 90) ? "강세를 대부분 잘 따라 했어요!" : (min >= 74) ? "강세가 원음과 유사합니다." : (min >= 20) ? "강세가 원음과 달라요!" : "강세를 평가할 수 없습니다.";
                }
            }
            return ass;
        }

        static RecognitionResult GetRecognitionResult(ResultData rd)
        {
            RecognitionResult rr = new RecognitionResult
            {
                success = rd.data.total_score >= passScore,
                score = rd.data.total_score,
                words = rd.data.words
            };
            rr.message = rr.success ? "SUCCESS" : "FAIL";

            if (rd.code != 1)
            {
                rr.success = false;
                switch (rd.code)
                {
                    default:
                    case 0: // FAIL
                        rr.score = -30;
                        rr.message = "녹음: FAIL";
                        break;
                    case -1: // ERROR_LICENCE
                    case -2: // ERROR_INIT
                        rr.score = -31;
                        rr.message = "ERROR_LICENSE OR ERROR_INIT";
                        break;
                    case -3: // ERROR_SETTEXT
                        rr.score = -33;
                        rr.message = "녹음: ERROR_SETTEXT";
                        break;
                    case -4: // ERROR_LIMITS
                        rr.score = -34;
                        rr.message = "녹음 파일: ERROR_LIMITS";
                        break;
                    case -5: // ERROR_TEXT
                        rr.score = -35;
                        rr.message = "녹음: ERROR_TEXT";
                        break;
                    case -6: // FAIL_ASSESSMENT
                        rr.score = -36;
                        rr.message = "녹음: FAIL_ASSESSMENT";
                        break;
                    case -7: // FAIL_GRADE_INIT
                        rr.score = -37;
                        rr.message = "녹음: FAIL_GRADE_INIT";
                        break;
                    case -8: // FAIL_INPUT_ERROR
                        rr.score = -38;
                        rr.message = "녹음: FAIL_INPUT_ERROR";
                        break;
                }
            }
            else
            if (rd.what != 1)
            {
                rr.success = false;
                switch (rd.what)
                {
                    case -1: // FAIL
                        rr.score = -40;
                        rr.message = "녹음: FAIL";
                        break;
                    case 0: // FRAME_RECORDING
                        rr.score = -41;
                        rr.message = "녹음: FRAME_RECORDING";
                        break;
                    case 2: // RECOG_REJECT
                        rr.score = -42;
                        rr.message = "녹음: RECOG_REJECT";
                        break;
                    case 3: // EPD_FAIL
                        rr.score = -43;
                        rr.message = "녹음: EPD_FAIL";
                        break;
                    case 4: // TIME_OVER
                        rr.score = -44;
                        rr.message = "녹음: TIME_OVER";
                        break;
                    case 5: // FRAME_ERROR
                        rr.score = -45;
                        rr.message = "녹음: FRAME_ERROR";
                        break;
                }
            }

            return rr;
        }

        static void makeS16LeMonoPCMFile(string orgMediaFilePath, Action<string> cb)
        {
            //System.Diagnostics.Stopwatch watch = new();
            //watch.Start();
            string pcmName = $"{Path.GetFileNameWithoutExtension(orgMediaFilePath)}.pcm";
            string pcmDis = Path.Combine(Path.GetDirectoryName(orgMediaFilePath), pcmName);
            selvy.makePCM(orgMediaFilePath, pcmDis, selvy.Handler((type, id, message) =>
            {
                switch (type)
                {
                    case 2: // SUCCESS
                        //watch.Stop();
                        //Debug.Log($"오디오 PCM(B) 변환 시간: {watch.ElapsedMilliseconds}ms");
                        cb(pcmDis);
                        break;
                    case 3: // CANCEL
                    case 4: // FAIL
                        cb(null);
                        break;
                }
            }));
        }

        static AssessmentResult SetAssessmentResult(ResultData rd, AssessmentResult assRes)
        {
            if (rd.codeOrg != 1)
            {
                assRes.success = false;
                switch (rd.codeOrg)
                {
                    default:
                    case 0: // FAIL
                        assRes.score = -10;
                        assRes.message = "원음: FAIL";
                        break;
                    case -1: // ERROR_LICENSE
                    case -2: // ERROR_INIT
                        assRes.score = -11;
                        assRes.message = "ERROR_LICENSE OR ERROR_INIT";
                        break;
                    case -3: // ERROR_SETTEXT
                        assRes.score = -13;
                        assRes.message = "원음: ERROR_SETTEXT";
                        break;
                    case -4: // ERROR_LIMITS
                        assRes.score = -14;
                        assRes.message = "원음: ERROR_LIMITS";
                        break;
                    case -5: // ERROR_TEXT
                        assRes.score = -15;
                        assRes.message = "원음: ERROR_TEXT";
                        break;
                    case -6: // FAIL_ASSESSMENT
                        assRes.score = -16;
                        assRes.message = "원음: FAIL_ASSESSMENT";
                        break;
                    case -7: // FAIL_GRADE_INIT
                        assRes.score = -17;
                        assRes.message = "원음: FAIL_GRADE_INIT";
                        break;
                    case -8: // FAIL_INPUT_ERROR
                        assRes.score = -18;
                        assRes.message = "원음: FAIL_INPUT_ERROR";
                        break;
                }
            }
            else
            if (rd.whatOrg != 1)
            {
                assRes.success = false;
                switch (rd.whatOrg)
                {
                    default:
                    case -1: // FAIL
                        assRes.score = -20;
                        assRes.message = "원음: FAIL";
                        break;
                    case 0: // FRAME_RECORDING
                        assRes.score = -21;
                        assRes.message = "원음: FRAME_RECORDING";
                        break;
                    case 2: // RECOG_REJECT
                        assRes.score = -22;
                        assRes.message = "원음: RECOG_REJECT";
                        break;
                    case 3: // EPD_FAIL
                        assRes.score = -23;
                        assRes.message = "원음: EPD_FAIL";
                        break;
                    case 4: // TIME_OVER
                        assRes.score = -24;
                        assRes.message = "원음: TIME_OVER";
                        break;
                    case 5: // FRAME_ERROR
                        assRes.score = -25;
                        assRes.message = "원음: FRAME_ERROR";
                        break;
                }
            }
            else
            if (rd.code != 1)
            {
                assRes.success = false;
                switch (rd.code)
                {
                    default:
                    case 0: // FAIL
                        assRes.score = -30;
                        assRes.message = "녹음: FAIL";
                        break;
                    case -1: // ERROR_LICENCE
                    case -2: // ERROR_INIT
                        assRes.score = -31;
                        assRes.message = "ERROR_LICENSE OR ERROR_INIT";
                        break;
                    case -3: // ERROR_SETTEXT
                        assRes.score = -33;
                        assRes.message = "녹음: ERROR_SETTEXT";
                        break;
                    case -4: // ERROR_LIMITS
                        assRes.score = -34;
                        assRes.message = "녹음 파일: ERROR_LIMITS";
                        break;
                    case -5: // ERROR_TEXT
                        assRes.score = -35;
                        assRes.message = "녹음: ERROR_TEXT";
                        break;
                    case -6: // FAIL_ASSESSMENT
                        assRes.score = -36;
                        assRes.message = "녹음: FAIL_ASSESSMENT";
                        break;
                    case -7: // FAIL_GRADE_INIT
                        assRes.score = -37;
                        assRes.message = "녹음: FAIL_GRADE_INIT";
                        break;
                    case -8: // FAIL_INPUT_ERROR
                        assRes.score = -38;
                        assRes.message = "녹음: FAIL_INPUT_ERROR";
                        break;
                }
            }
            else
            if (rd.what != 1)
            {
                assRes.success = false;
                switch (rd.what)
                {
                    case -1: // FAIL
                        assRes.score = -40;
                        assRes.message = "녹음: FAIL";
                        break;
                    case 0: // FRAME_RECORDING
                        assRes.score = -41;
                        assRes.message = "녹음: FRAME_RECORDING";
                        break;
                    case 2: // RECOG_REJECT
                        assRes.score = -42;
                        assRes.message = "녹음: RECOG_REJECT";
                        break;
                    case 3: // EPD_FAIL
                        assRes.score = -43;
                        assRes.message = "녹음: EPD_FAIL";
                        break;
                    case 4: // TIME_OVER
                        assRes.score = -44;
                        assRes.message = "녹음: TIME_OVER";
                        break;
                    case 5: // FRAME_ERROR
                        assRes.score = -45;
                        assRes.message = "녹음: FRAME_ERROR";
                        break;
                }
            }
            else
            if (rd.assess != 1)
            {
                assRes.success = false;
                switch (rd.assess)
                {
                    default:
                    case 0: // FAIL
                        assRes.score = -50;
                        assRes.message = "평가: FAIL";
                        break;
                    case -1: // ERROR_LICENSE
                    case -2: // ERROR_INIT
                        assRes.score = -51;
                        assRes.message = "ERROR_LICENSE OR ERROR_INIT";
                        break;
                    case -3: // ERROR_SETTEXT
                        assRes.score = -53;
                        assRes.message = "평가: ERROR_SETTEXT";
                        break;
                    case -4: // ERROR_LIMITS
                        assRes.score = -54;
                        assRes.message = "평가: ERROR_LIMITS";
                        break;
                    case -5: // ERROR_TEXT
                        assRes.score = -55;
                        assRes.message = "평가: ERROR_TEXT";
                        break;
                    case -6: // FAIL_ASSESSMENT
                        assRes.score = -56;
                        assRes.message = "평가: FAIL_ASSESSMENT";
                        break;
                    case -7: // FAIL_GRADE_INIT
                        assRes.score = -57;
                        assRes.message = "평가: FAIL_GRADE_INIT";
                        break;
                    case -8: // FAIL_INPUT_ERROR
                        assRes.score = -58;
                        assRes.message = "평가: FAIL_INPUT_ERROR";
                        break;
                }
            }

            return assRes;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        static string TempDir = Path.Combine(Application.persistentDataPath, "Temp");
        public static string GetTempFilePath(string fname)
        {
            string fPath = Path.Combine(TempDir, fname);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(fPath)));
            return fPath;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        public static void Recognition(AudioClip recClip, string sentence, Action<RecognitionResult> cb)
        {
            string recPath = MakeClipToWav(recClip, GetTempFilePath("rec.wav"));
            Recognition(recPath, sentence, cb);
        }
        public static void Recognition(AudioClip recClip, string sentence, Action<RecognitionResult, string> cb)
        {
            string recPath = MakeClipToWav(recClip, GetTempFilePath("rec.wav"));
            Recognition(recPath, sentence, cb);
        }
        public static void Recognition(string recPath, string sentence, Action<RecognitionResult> cb)
        {
            Recognition(recPath, sentence, (rr, path) =>
            {
                cb(rr);
            });
        }

        /// </summary>
        /// <param name="recPath">녹음된 mp3, wav 또는 녹화된 mp4 파일 등등</param>
        /// <param name="sentence"></param>
        /// <param name="cb">결과 + 녹음파일 패스</param>
        public static void Recognition(string recPath, string sentence, Action<RecognitionResult, string> cb)
        {
            Debug.Log("selvy: Recognition -1");

            string recPcmPath = null;

            Action callback = () =>
            {
                Debug.Log($"selvy: Recognition -4. {recPcmPath}");
                if (recPcmPath == null) return;

                ResultData rd = new ResultData();
                rd.data = new Data();
                rd.assessment = new Assessment();
                rd.codeOrg = 1;
                rd.whatOrg = 1;
                rd.code = 1;
                rd.what = 1;
                rd.assess = 1;
                rd.data.result = 100;
                rd.data.total_score = 100;

#if UNITY_EDITOR
                {
                    Debug.Log("EDITOR");
                    cb(GetRecognitionResult(rd), recPath);
                }
#elif UNITY_ANDROID
                {
                    Debug.Log("ANDROID");
                    AndroidJavaClass ajc = new AndroidJavaClass("com.beyondisoft.selvyengine.selvyEngine");
                    AndroidJavaObject ajo = ajc.CallStatic<AndroidJavaObject>("instance");
                    if (ajo == null) {
                        rd.codeOrg = -1; // ERROR_LICENSE
                        cb(GetRecognitionResult(rd), recPath);
                    }
                    else
                    {
                        string res = ajo.Call<string>("Recognition", sentence, recPcmPath);
                        Debug.Log($"ANDROID: {res}");
                        rd = JsonConvert.DeserializeObject<ResultData>(res);
                        cb(GetRecognitionResult(rd), recPath);
                    }
                }
#elif UNITY_IOS
                {
                    Debug.Log("selvy: iOS");
                    string dbPath = Path.Combine(Application.streamingAssetsPath, @"Selvy/iOS/db/"); // DB경로를 여기서 지정한다.
                    string res = iOS_Recognition(sentence, recPcmPath, dbPath);
                    Debug.Log($"selvy: iOS: {res}");
                    rd = JsonConvert.DeserializeObject<ResultData>(res);
                    cb(GetRecognitionResult(rd), recPath);
                }
#else
                {
                    Debug.Log("OTHER");
                    cb(GetRecognitionResult(rd), recPath);
                }
#endif
            };

            makeS16LeMonoPCMFile(recPath, (recDis) =>
            {
                Debug.Log($"selvy: Recognition -2");

                if (recDis == null)
                {
                    Debug.Log($"selvy: Recognition -3");

                    RecognitionResult recRes = new RecognitionResult
                    {
                        success = false,
                        score = -2,
                        message = "녹음 파일에 문제가 있습니다."
                    };
                    cb(recRes, recPath);

                    return;
                }
                recPcmPath = recDis;
                callback();
            });
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        public static void Assessment(AudioClip orgClip, AudioClip recClip, string sentence, Action<AssessmentResult> cb)
        {
            string orgPath = MakeClipToWav(orgClip, GetTempFilePath("org.wav"));
            string recPath = MakeClipToWav(recClip, GetTempFilePath("rec.wav"));
            Assessment(orgPath, recPath, sentence, cb);
        }
        public static void Assessment(AudioClip orgClip, AudioClip recClip, string sentence, Action<AssessmentResult, string> cb)
        {
            string orgPath = MakeClipToWav(orgClip, GetTempFilePath("org.wav"));
            string recPath = MakeClipToWav(recClip, GetTempFilePath("rec.wav"));
            Assessment(orgPath, recPath, sentence, cb);
        }
        public static void Assessment(AudioClip orgClip, AudioClip recClip, string sentence, Action<AssessmentResult, string, string> cb)
        {
            string orgPath = MakeClipToWav(orgClip, GetTempFilePath("org.wav"));
            string recPath = MakeClipToWav(recClip, GetTempFilePath("rec.wav"));
            Assessment(orgPath, recPath, sentence, cb);
        }

        public static void Assessment(string orgPath, AudioClip recClip, string sentence, Action<AssessmentResult> cb)
        {
            string recPath = MakeClipToWav(recClip, GetTempFilePath("rec.wav"));
            Assessment(orgPath, recPath, sentence, cb);
        }
        public static void Assessment(string orgPath, AudioClip recClip, string sentence, Action<AssessmentResult, string> cb)
        {
            string recPath = MakeClipToWav(recClip, GetTempFilePath("rec.wav"));
            Assessment(orgPath, recPath, sentence, cb);

        }
        public static void Assessment(string orgPath, AudioClip recClip, string sentence, Action<AssessmentResult, string, string> cb)
        {
            string recPath = MakeClipToWav(recClip, GetTempFilePath("rec.wav"));
            Assessment(orgPath, recPath, sentence, cb);
        }

        public static void Assessment(AudioClip orgClip, string recPath, string sentence, Action<AssessmentResult> cb)
        {
            string orgPath = MakeClipToWav(orgClip, GetTempFilePath("org.wav"));
            Assessment(orgPath, recPath, sentence, cb);
        }
        public static void Assessment(AudioClip orgClip, string recPath, string sentence, Action<AssessmentResult, string> cb)
        {
            string orgPath = MakeClipToWav(orgClip, GetTempFilePath("org.wav"));
            Assessment(orgPath, recPath, sentence, cb);

        }
        public static void Assessment(AudioClip orgClip, string recPath, string sentence, Action<AssessmentResult, string, string> cb)
        {
            string orgPath = MakeClipToWav(orgClip, GetTempFilePath("org.wav"));
            Assessment(orgPath, recPath, sentence, cb);
        }

        public static void Assessment(string orgPath, string recPath, string sentence, Action<AssessmentResult> cb)
        {
            Assessment(orgPath, recPath, sentence, (res, orgPath, recPath) =>
            {
                cb(res);
            });
        }
        public static void Assessment(string orgPath, string recPath, string sentence, Action<AssessmentResult, string> cb)
        {
            Assessment(orgPath, recPath, sentence, (res, orgPath, recPath) =>
            {
                cb(res, recPath);
            });
        }

        /// <summary>
        /// 원음과 녹음된 파일을 비교하여 평과결과 및 원음파일(wav) 녹음파일(wav)의 위치를 리턴한다.
        /// </summary>
        /// <param name="orgPath">원음파일: wav</param>
        /// <param name="recPath">녹음파일: wav</param>
        /// <param name="sentence">판단할 문장 또는 단어</param>
        /// <param name="cb">AssessmentResult: 정리된 결과값, string: 원음파일(wav), string: 녹음파일(wav)</param>
        public static void Assessment(string orgPath, string recPath, string sentence, Action<AssessmentResult, string, string> cb)
        {
            Debug.Log("selvy: Assessment -1");

            string orgPcmPath = null, recPcmPath = null;

            Action callback = () =>
            {
                Debug.Log($"selvy: Assessment -6, {orgPcmPath}, {recPcmPath}");
                if (orgPcmPath == null) return;
                if (recPcmPath == null) return;

                ResultData rd = new()
                {
                    data = new()
                    {
                        result = 100,
                        total_score = 100
                    },
                    assessment = new()
                    {
                        overall = 100,
                        prosody_score = 100,
                        pronunciation_score = 100,
                        timing_score = 100,
                        intonation_score = 100,
                        loudness_score = 100,
                        pronunciation = new int[] { 100 },
                        timing = new int[] { 100 },
                        intonation = new int[] { 100 },
                        loudness = new int[] { 100 },
                        sentence = sentence
                    },
                    codeOrg = 1,
                    whatOrg = 1,
                    code = 1,
                    what = 1,
                    assess = 1
                };

#if UNITY_EDITOR
                {
                    Debug.Log("EDITOR");
                    rd.data.result = rd.data.total_score = rd.assessment.overall = UnityEngine.Random.Range(50, 100);
                    rd.assessment.prosody_score = UnityEngine.Random.Range(50, 100);
                    rd.assessment.pronunciation_score = UnityEngine.Random.Range(50, 100);
                    rd.assessment.timing_score = UnityEngine.Random.Range(50, 100);
                    rd.assessment.intonation_score = UnityEngine.Random.Range(50, 100);
                    rd.assessment.loudness_score = UnityEngine.Random.Range(50, 100);
                    AssessmentResult assRes = GetAssessmentResult(rd.assessment);
                    cb(SetAssessmentResult(rd, assRes), orgPath, recPath);
                }
#elif UNITY_ANDROID
                {
                    Debug.Log("ANDROID");
                    AndroidJavaClass ajc = new AndroidJavaClass("com.beyondisoft.selvyengine.selvyEngine");
                    AndroidJavaObject ajo = ajc.CallStatic<AndroidJavaObject>("instance");
                    if (ajo == null) {
                        rd.codeOrg = -1; // ERROR_LICENSE
                        AssessmentResult assRes = GetAssessmentResult(rd.assessment);
                        cb(SetAssessmentResult(rd, assRes), orgPath, recPath);
                    }
                    else
                    {
                        string res = ajo.Call<string>("Assessment", sentence, orgPcmPath, recPcmPath);
                        Debug.Log($"ANDROID: {res}");
                        rd = JsonConvert.DeserializeObject<ResultData>(res);
                        rd.assessment.sentence = sentence;
                        AssessmentResult assRes = GetAssessmentResult(rd.assessment);
                        cb(SetAssessmentResult(rd, assRes), orgPath, recPath);
                    }
                }
#elif UNITY_IOS
                {
                    Debug.Log("selvy: iOS");
                    string dbPath = Path.Combine(Application.streamingAssetsPath, @"Selvy/iOS/db/"); // DB경로를 여기서 지정한다.
                    string res = iOS_Assessment(sentence, orgPcmPath, recPcmPath, dbPath);
                    Debug.Log($"selvy: iOS: {res}");
                    rd = JsonConvert.DeserializeObject<ResultData>(res);
                    rd.data ??= new()
                    {
                        result = 0,
                        total_score = 0
                    };
                    rd.assessment ??= new()
                    {
                        overall = 0,
                        prosody_score = 0,
                        pronunciation_score = 0,
                        timing_score = 0,
                        intonation_score = 0,
                        loudness_score = 0,
                        pronunciation = new int[] { 0 },
                        timing = new int[] { 0 },
                        intonation = new int[] { 0 },
                        loudness = new int[] { 0 },
                        sentence = sentence
                    };
                    rd.assessment.sentence = sentence;
                    AssessmentResult assRes = GetAssessmentResult(rd.assessment);
                    cb(SetAssessmentResult(rd, assRes), orgPath, recPath);
                }
#else
                {
                    Debug.Log("OTHER");
                    AssessmentResult assRes = GetAssessmentResult(rd.assessment);
                    cb(SetAssessmentResult(rd, assRes), orgPath, recPath);
                }
#endif
            };

            makeS16LeMonoPCMFile(orgPath, (orgDis) =>
            {
                Debug.Log($"selvy: Assessment -2");
                if (orgDis == null)
                {
                    Debug.Log($"selvy: Assessment -3");
                    AssessmentResult assRes = new AssessmentResult
                    {
                        success = false,
                        score = -1,
                        message = "원음 파일에 문제가 있습니다."
                    };
                    cb(assRes, orgPath, recPath);
                    return;
                }
                orgPcmPath = orgDis;

                makeS16LeMonoPCMFile(recPath, (recDis) =>
                {
                    Debug.Log($"selvy: Assessment -4");
                    if (recDis == null)
                    {
                        Debug.Log($"selvy: Assessment -5");
                        AssessmentResult assRes = new AssessmentResult
                        {
                            success = false,
                            score = -2,
                            message = "녹음 파일에 문제가 있습니다."
                        };
                        cb(assRes, orgPath, recPath);

                        return;
                    }
                    recPcmPath = recDis;

                    callback();
                });
            });
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    [Serializable]
    public class Word
    {
        public string word;
        public int score;
        public int stress;
        public string position;
        public string phoneme;
        public int phoneme_cnt;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public class Data
    {
        public string sentence;
        public int result;
        public int total_score;
        public Word[] words;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public class Assessment
    {
        public int overall;
        public int prosody_score;
        public int pronunciation_score;
        public int timing_score;
        public int intonation_score;
        public int loudness_score;
        public int[] pronunciation;
        public int[] timing;
        public int[] intonation;
        public int[] loudness;
        public string sentence;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public class ResultData
    {
        public int codeOrg;
        public int whatOrg;
        public int code;
        public int what;
        public int assess;
        public Data data;
        public Assessment assessment;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public class AssessmentResult
    {
        public bool success;
        public int score;
        public string weak; // 취약점
        public string weak_grade; // 취약점 등급(S,A,B,C,D,E,F)
        public int weak_score; // 취약점 점수
        public string message;

        public int prosody_score;
        public int pronunciation_score;
        public int timing_score;
        public int intonation_score;
        public int loudness_score;
        public string sentence;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    [Serializable]
    public class RecognitionResult
    {
        public bool success;
        public int score;
        public Word[] words;
        public string message;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
