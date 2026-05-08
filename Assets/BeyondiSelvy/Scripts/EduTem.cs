using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
// using DoDoEng.AIStudio;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace DoDoEng.EduTem
{
    public class V2
    {
        const string pron_v2_url = "https://api.readinggate.elasolution.com/pron_v2/";  // Speak (ebook, AIStudio 의 문장),  최종점수 : total_score
        const string pron_v2_phoneme_url = "https://api.readinggate.elasolution.com/pron_v2/phoneme";  // Sight words (단어), 최종점수 : average_phoneme_score
        const string API_KEY = "e874641aac784ff6b9d62c3483f7aaaa"; //  (인증키)
        const int REQUEST_TIMEOUT = 7;

        public static async UniTask<_v2_result> Assessment(string word, string nativeFilePath, string studentFilePath, CancellationTokenSource cancel = null)
        {
            return await Assessment(word, new FileInfo(nativeFilePath), new FileInfo(studentFilePath), cancel);
        }
        public static async UniTask<_v2_result> Assessment(string word, FileInfo native, FileInfo student, CancellationTokenSource cancel = null)
        {
            List<IMultipartFormSection> formData = new()
            {
                new MultipartFormFileSection("native_audio", File.ReadAllBytes(native.FullName), native.Name, "audio/*"),
                new MultipartFormFileSection("student_audio", File.ReadAllBytes(student.FullName), student.Name, "audio/*"),
                new MultipartFormDataSection("text", word)
            };

            _v2_result rtn = null;
            using (UnityWebRequest www = UnityWebRequest.Post(pron_v2_url, formData))
            {
                www.timeout = REQUEST_TIMEOUT;
                www.SetRequestHeader("X-API-Key", API_KEY);
                try
                {
                    await www.SendWebRequest();
                    if (cancel != null && cancel.Token.IsCancellationRequested) return rtn;
                    if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
                    {
                        rtn = JsonConvert.DeserializeObject<_v2_result>(www.downloadHandler.text);
                    }
                    else
                    {
                        Debug.LogError($"Error: {www.responseCode}, {www.error}");
                        rtn = new();
                    }
                    rtn.status = www.responseCode;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    Debug.LogError(www.error);
                    rtn = new()
                    {
                        status = www.error.ToLower().Contains("timeout") ? 408 : www.responseCode
                    };
                }
            }
            // rtn.recClip = await Util.LoadAudioClip(student.FullName, AudioType.WAV);
            return rtn;
        }


        public static async UniTask<_v2_phoneme_result> PhonemeAssessment(string word, string audioPath, CancellationTokenSource cancel = null)
        {
            return await PhonemeAssessment(word, new FileInfo(audioPath), cancel);
        }
        public static async UniTask<_v2_phoneme_result> PhonemeAssessment(string word, FileInfo audio, CancellationTokenSource cancel = null)
        {
            List<IMultipartFormSection> formData = new()
            {
                new MultipartFormFileSection("audio", File.ReadAllBytes(audio.FullName), audio.Name, "audio/*"),
                new MultipartFormDataSection("text", word)
            };

            _v2_phoneme_result rtn = null;
            using (UnityWebRequest www = UnityWebRequest.Post(pron_v2_phoneme_url, formData))
            {
                www.SetRequestHeader("X-API-Key", API_KEY);
                try
                {
                    www.timeout = REQUEST_TIMEOUT;
                    await www.SendWebRequest();
                    if (cancel != null && cancel.Token.IsCancellationRequested) return rtn;
                    if (www.result == UnityWebRequest.Result.Success && www.responseCode == 200)
                    {
                        rtn = JsonConvert.DeserializeObject<_v2_phoneme_result>(www.downloadHandler.text);
                    }
                    else
                    {
                        Debug.LogError($"Error: {www.responseCode}, {www.error}");
                        rtn = new();
                    }
                    rtn.status = www.responseCode;
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    Debug.LogError(www.error);
                    rtn = new()
                    {
                        status = www.error.ToLower().Contains("timeout") ? 408 : www.responseCode
                    };
                }
            }
            // rtn.recClip = await Util.LoadAudioClip(audio.FullName, AudioType.WAV);
            return rtn;
        }


        const float volumeOffest = 1.0f;
        public static string MakeRecPath(string fname)
        {
            string DataDir = Path.Combine(Application.persistentDataPath, "Temp/EduTem/");
            string filePath = Path.Combine(DataDir, fname);
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(filePath)));
            return filePath;
        }

        public static async UniTask<_v2_result> GetV2Result(string word, AudioSource audioSource, string nativeFilePath, Action actionReadyMic, Action<float> actionWaitMic, Action actionTimeOut, Action actionReadyRun, Action<float[], float, float> actionRun, CancellationTokenSource cancel = null)
        {
            FileInfo fi = new(nativeFilePath);
            string orgPath = fi.FullName;
            string ext = fi.Extension.ToUpper();
            AudioType AT = ext.Equals(".MP3") ? AudioType.MPEG : AudioType.WAV;
            // AudioClip clipOrg = await Util.LoadAudioClip(orgPath, AT);
            // float orgMaxVolume = Util.GetMaxVoume(clipOrg) * 1.0f;
            // orgMaxVolume = orgMaxVolume > volumeOffest ? orgMaxVolume : volumeOffest;

            bool bEnd = false;
            _v2_result rtn = null;
            string recPath = MakeRecPath("edutemRec.wav");
            // await Util.Record(audioSource, clipOrg, actionReadyMic, actionWaitMic, actionTimeOut, actionReadyRun, actionRun, async (clip) =>
            // {
            //     string wavRecPath = Util.MakeClipToWav(clip, recPath, true, orgMaxVolume);
            //     rtn = await Assessment(word, orgPath, wavRecPath, cancel);
            //     bEnd = true;
            // });
            await UniTask.WaitUntil(() => bEnd);
            return rtn;
        }

        public static async UniTask<_v2_phoneme_result> GetV2PhonemeResult(string word, AudioSource audioSource, string nativeFilePath, Action actionReadyMic, Action<float> actionWaitMic, Action actionTimeOut, Action actionReadyRun, Action<float[], float, float> actionRun, CancellationTokenSource cancel = null)
        {
            FileInfo fi = new(nativeFilePath);
            string orgPath = fi.FullName;
            string ext = fi.Extension.ToUpper();
            AudioType AT = ext.Equals(".MP3") ? AudioType.MPEG : AudioType.WAV;
            // AudioClip clipOrg = await Util.LoadAudioClip(orgPath, AT);
            // float orgMaxVolume = Util.GetMaxVoume(clipOrg) * 1.0f;
            // orgMaxVolume = orgMaxVolume > volumeOffest ? orgMaxVolume : volumeOffest;

            bool bEnd = false;
            _v2_phoneme_result rtn = null;
            string recPath = MakeRecPath("edutemRec.wav");
            // await Util.Record(audioSource, clipOrg, actionReadyMic, actionWaitMic, actionTimeOut, actionReadyRun, actionRun, async (clip) =>
            // {
            //     string wavRecPath = Util.MakeClipToWav(clip, recPath, true, orgMaxVolume);
            //     rtn = await PhonemeAssessment(word, wavRecPath, cancel);
            //     bEnd = true;
            // });
            await UniTask.WaitUntil(() => bEnd);
            return rtn;
        }
    }

    [Serializable]
    public class _v2_phoneme_score
    {
        public string phoneme;
        public float score;
    }

    [Serializable]
    public class _v2_phoneme_word
    {
        public string word;
        public _v2_phoneme_score[] phonemes;
    }
    [Serializable]
    public class _v2_phoneme_result
    {
        public long status; // web response code
        public bool speech_detected;
        public float average_phoneme_score;
        public _v2_phoneme_word[] words;
        public AudioClip recClip;
        public override string ToString() { return JsonConvert.SerializeObject(this); }
    }


    [Serializable]
    public class _v2_data
    {
        public float accuracy;
        public float intonation;
        public float accent;
        public float speed;
        public float pause;
    }
    [Serializable]
    public class _v2_result
    {
        public long status; // web response code
        public string best_answer;
        public _v2_data score_weight;
        public _v2_data score;
        public _v2_data weighted_score;
        public float total_score;
        public bool speech_detected;
        public AudioClip recClip;
        public override string ToString() { return JsonConvert.SerializeObject(this); }
    }
}