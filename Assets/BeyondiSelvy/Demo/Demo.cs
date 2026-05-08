using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeyondiSelvyDemo
{
    using UnityEngine.UI;
    using UnityEngine.Video;
    using System.IO;
    using System;
    using BeyondiSelvy;
    using UnityEngine.Networking;

    public class Demo : MonoBehaviour
    {
        [SerializeField] VideoPlayer video;
        [SerializeField] Button btnPlay;
        [SerializeField] RawImage micIcon, playIcon, recIcon;
        [SerializeField] Text textField, sentenceField;
        [SerializeField] AudioSource audioSource;

        string TempDir;
        string orgPath;

        List<string> sentences = new List<string>() { "bat", "book", "dog", "doll", "gate", "goat" };
        string sentence;
        AudioSource _audioSource;

        private void Start()
        {
            TempDir = Path.Combine(Application.persistentDataPath, "Temp");
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.volume = 1;
            _audioSource.loop = false;
            Init();
        }

        int step = 0;
        void Init()
        {
            step = 0;
            _audioSource.volume = 1;
            sentence = sentences[0];
            sentences.Add(sentences[0]);
            sentences.RemoveAt(0);
            sentenceField.text = sentence;
            string orgMp3Path = $@"https://dev.beyondiedu.com/DoDoEng/ai_speak/data/{sentence}.mp3";
            StartCoroutine(IEUrlDownLoad(orgMp3Path, Path.Combine(TempDir, "org.mp3"), (path) =>
            {
                Debug.Log(path);
                orgPath = path;
                btnPlay.gameObject.SetActive(false);
                recIcon.gameObject.SetActive(false);
                StartCoroutine(IELoadAudioClip(orgPath));
            }));
        }

        IEnumerator IEUrlDownLoad(string url, string path, Action<string> cb)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                    yield break;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
                File.WriteAllBytes(path, www.downloadHandler.data);
                cb(path);
            }
        }

        IEnumerator IELoadAudioClip(string path)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($@"file://{path}", AudioType.MPEG))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                    yield break;
                }

                _audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                showPlayButton();
            }
        }

        IEnumerator IEVideoPrepare()
        {
            video.Prepare();
            yield return new WaitUntil(() => video.isPrepared);
            step = 0;
            video.frame = 1;
            showPlayButton();
        }

        void showPlayButton()
        {
            recIcon.gameObject.SetActive(false);
            btnPlay.gameObject.SetActive(true);
            switch (step)
            {
                case 0:
                    micIcon.gameObject.SetActive(false);
                    playIcon.gameObject.SetActive(true);
                    break;
                case 1:
                    micIcon.gameObject.SetActive(true);
                    playIcon.gameObject.SetActive(false);
                    break;
            }
        }

        bool bPlay = false;
        public void videoPlay()
        {
            bPlay = true;
            btnPlay.gameObject.SetActive(false);
            _audioSource.Play();
            if (step == 1)
            {
                Record();
            }
        }

        void Update()
        {
            if (!bPlay) return;
            if (_audioSource.clip != null)
            {
                string time = _audioSource.time.ToString("F3");
                string length = _audioSource.clip.length.ToString("F3");
                if (_audioSource.isPlaying)
                {
                    textField.text = $"{time} / {length}";
                }
                else if (bPlay)
                {
                    bPlay = false;
                    _audioSource.Stop();
                    _audioSource.volume = 0;
                    textField.text = "---";
                    step = ++step % 2;
                    if (step == 1) showPlayButton();
                }
            }
        }

        public void Record()
        {
            recIcon.gameObject.SetActive(true);
            StartCoroutine(IERecord());
        }

        IEnumerator IERecord()
        {
#if !UNITY_WEBGL
            int duration = Mathf.CeilToInt((float)_audioSource.clip.length);
            audioSource.clip = Microphone.Start(null, false, duration, selvy.GetMicrophoneFrequency());

            yield return new WaitUntil(() => Microphone.GetPosition(null) > 0);
            yield return new WaitUntil(() => Microphone.IsRecording(null));
            yield return new WaitUntil(() => !Microphone.IsRecording(null));

            audioSource.Stop();
            Microphone.End(null);

            textField.text = "selvy 분석중...";

            selvy.Assessment(_audioSource.clip, audioSource.clip, sentence, (rd, orgWav, recWav) =>
            {
                showPlayButton();
                Debug.Log($"UNITY ResultData: {JsonUtility.ToJson(rd)}");

                string scores = $"prosody: {rd.prosody_score}, pronunciation: {rd.pronunciation_score}, timing: {rd.timing_score}, intonation: {rd.intonation_score}, loudness: {rd.loudness_score}";
                if (rd.success)
                {
                    textField.text = $"성공\n점수: {rd.score} >= {selvy.passScore} → {rd.score >= selvy.passScore}\n취약: {rd.weak}({rd.weak_score}), {rd.weak_grade}\n{rd.message}\n{scores}";
                }
                else
                {
                    if (rd.score < 0)
                    {
                        textField.text = $"오류({rd.score}): {rd.message}";
                    }
                    else
                    {
                        textField.text = $"실패\n점수: {rd.score} >= {selvy.passScore} → {rd.score >= selvy.passScore}\n취약: {rd.weak}({rd.weak_score}), {rd.weak_grade}\n{rd.message}\n{scores}";
                    }
                }
                Init();

                // 녹음 노말라이징, 원음의 볼륨에 맞춰 변형한다.
                selvy.Normalize(orgWav, recWav, (type, path) =>
                {
                    if (type == 2) Debug.Log($"selvy.Normalize SUCCESS: {path}");
                });
            });
#else
            Debug.LogWarning("Microphone is not supported on WebGL.");
            yield break;
#endif
        }
    }
}
