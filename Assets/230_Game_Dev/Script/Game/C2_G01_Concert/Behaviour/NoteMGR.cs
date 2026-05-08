using beyondi.Util;
using DG.Tweening;
using DoDoEng.Common;
using DoDoEng.Game.Framework;
using DoDoEng.Game.UI;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace DoDoEng.Game.C2_G01
{
    [Flags]
    public enum LR
    {
        L = 1,
        R = 1 << 1,

        Both = L | R
    }

    [RequireComponent(typeof(NoteBuilder))]
    [RequireComponent(typeof(NotePool))]
    public class NoteMGR : MonoBehaviour
    {
        // Methods
        public void Init(QuizBoard quizBoard)
        {
            LOG.Info($"{nameof(Init)}()", this);

            var spawnY = noteLSpawnTR.GetComponent<RectTransform>().anchoredPosition.y;
            var hitY = hitLTR.GetComponent<RectTransform>().anchoredPosition.y;
            noteDropHeight = spawnY - hitY;

            this.quizBoard = quizBoard;
        }
        public Coroutine StartMusic(RoundData roundData)
        {
            LOG.Info($"{nameof(StartMusic)}() | {roundData}", this);

            this.roundData = roundData;

            noteScroller.MusicForGizmos = roundData.Music;
            noteScroller.StartScroll(roundData.Music.Speed);

            problemSuccessCount = Enumerable.Repeat(0, roundData.Problems.Length).ToArray();

            crPlayMusic = StartCoroutine(coPlayMusic());
            return crPlayMusic;
        }
        public void StopMusic()
        {
            LOG.Info($"{nameof(StopMusic)}()", this);

            quizBoard.HideProblem();
            noteScroller.StopScroll();
            this.StopCoroutineSafe(ref crPlayMusic);

            audioSource.Stop();

            releaseAllNotes();
            AudioMGR.One.StopBGM();
        }

        // Methods
        public void DoFever()
        {
            LOG.Info($"{nameof(DoFever)}()", this);

            isFever = true;

            var notes = noteScroller.GetComponentsInChildren<Note>(false);
            notes.ForEach(n => n.ChangeToFever());
            audioSource.DOFade(feverVol, 1);
        }
        public void DoNormal()
        {
            LOG.Info($"{nameof(DoNormal)}()", this);

            isFever = false;

            var notes = noteScroller.GetComponentsInChildren<Note>(false);
            notes.ForEach(n => n.ChangeToMusic());
            audioSource.DOFade(normalVol, 1);
        }

        // Methods
        public bool checkProblemFail(Note note)
        {
            // 정답인 경우에만 호출됨
            if (note.IsLastSeq)
                return problemSuccessCount[note.PNO - 1] == 0;
            else return false;
        }



        // Fields : caching
        private NotePool pool_ = null;
        private NotePool pool => pool_ ??= GetComponent<NotePool>();
        private NoteBuilder builder_ = null;
        private NoteBuilder builder => builder_ ??= GetComponent<NoteBuilder>();
        private AudioSource audioSource_ = null;
        private AudioSource audioSource => audioSource_ ??= gameObject.AddComponent<AudioSource>();

        // Fields
        private RoundData roundData = null;
        private QuizBoard quizBoard = null;
        private Coroutine crPlayMusic = null;
        private float noteDropHeight;
        private bool isFever;
        private int[] problemSuccessCount = null;

        // Functions
        private void spawnNote(MusicNote musicNote)
        {
            switch (musicNote.Cmd)
            {
                case MusicNote.Command.Empty: break;
                case MusicNote.Command.Music:
                    if (musicNote.LR.HasFlag(LR.L))
                    {
                        var note = pool.Get();
                        note.gameObject.name = $"Note-{musicNote.NoteNO}";
                        note.InitForMusic(floorTR.position.y);
                        note.transform.position = noteLSpawnTR.position;
                    }
                    if (musicNote.LR.HasFlag(LR.R))
                    {
                        var note = pool.Get();
                        note.gameObject.name = $"Note-{musicNote.NoteNO}";
                        note.InitForMusic(floorTR.position.y);
                        note.transform.position = noteRSpawnTR.position;
                    }
                    break;
                case MusicNote.Command.Text:
                    {
                        var spawnTR = musicNote.LR.HasFlag(LR.L) ? noteLSpawnTR : noteRSpawnTR;
                        var note = pool.Get();
                        note.gameObject.name = $"Note-{musicNote.NoteNO}";
                        note.InitForText(floorTR.position.y,
                            musicNote.PNO,
                            musicNote.Phonics,
                            musicNote.PhonicsCLIP,
                            musicNote.IsAnswer,
                            musicNote.ProblemID,
                            musicNote.IsLastSeq);
                        note.transform.position = spawnTR.position;

                        if (musicNote.IsLast)
                        {
                            var duration = noteDropHeight / roundData.Music.Speed;
                            var delay = duration + roundData.Music.BeatInterval * 0.7f;
                            DOVirtual.DelayedCall(delay, () =>
                            {
                                quizBoard.HideProblem();
                                quizBoard.EnableInteraction(false);
                            }, false);
                        }
                    }
                    break;
                case MusicNote.Command.Problem:
                    {
                        LOG.Info($"----- Problem {musicNote.PNO} -----", this);

                        var duration = noteDropHeight / roundData.Music.Speed;

                        var pNO = musicNote.PNO - 1;
                        var phrases = roundData.Problems[pNO].Phrases;
                        var wordCLIP = roundData.Problems[pNO].WordCLIP;

                        //var isBlankQuiz = roundData.Round == 3;
                        // 멘티스 수정 #1419, 기록을 위해 남겨둠
                        var isBlankQuiz = false;

                        DOVirtual.DelayedCall(duration, () =>
                        {
                            quizBoard.ShowProblem(phrases, wordCLIP, isBlankQuiz);
                            quizBoard.EnableInteraction(true);
                            AudioMGR.One.PlayEffect(wordCLIP);
                        });
                    }
                    break;
            }
        }
        private void spawnFeverNote(int randomNum)
        {
            {
                var note = pool.Get();
                note.gameObject.name = $"Note-Fever";
                note.InitForFever(floorTR.position.y);
                note.transform.position = noteLSpawnTR.position;
            }
            {
                var note = pool.Get();
                note.gameObject.name = $"Note-Fever";
                note.InitForFever(floorTR.position.y);
                note.transform.position = noteRSpawnTR.position;
            }
        }
        private void releaseAllNotes()
        {
            var notes = noteScroller.GetComponentsInChildren<Note>(false);
            notes.ForEach(n => pool.Release(n));
        }
        private void playMusic(AudioClip clip)
        {
            audioSource.clip = clip;
            audioSource.volume = normalVol;
            audioSource.Play();
        }
        private bool isProblemCorrectAll(int pNO) => problemSuccessCount[pNO] >= 3;

        // Event Handlers
        private void note_OnCorrect(Note note)
        {
            LOG.Info($"{nameof(note_OnCorrect)}() | {note.PNO}", this);

            AudioMGR.One.PlayEffect(correctCLIP[note.ColorType - 1]);

            quizBoard.ShowAnswer(note.ProblemID != 3);

            var pNO = note.PNO - 1;
            if (problemSuccessCount[pNO] == 0)
            {
                UIGameCommon.One.StarGauge.Success();
                GameProgress.One.Correct();
            }
            problemSuccessCount[pNO]++;

            if (!isFever && isProblemCorrectAll(pNO))
            {
                var notes = noteScroller.GetComponentsInChildren<Note>(false);
                notes.Where(n => n.NoteType == NoteType.Text).ForEach(n => n.ChangeToMusic());
            }

        }
        private void note_OnWrong(Note note)
        {
            LOG.Info($"{nameof(note_OnWrong)}()", this);

            AudioMGR.One.PlayEffect(wrongCLIP);
        }
        private void note_OnHit(Note note)
        {
            LOG.Info($"{nameof(note_OnHit)}() | {note.IsFever}", this);

            if (note.IsFever)
                AudioMGR.One.PlayEffect(feverHitCLIP);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private NoteScroller noteScroller = null;
        [SerializeField] private Transform noteLSpawnTR = null;
        [SerializeField] private Transform noteRSpawnTR = null;
        [SerializeField] private Transform hitLTR = null;
        [SerializeField] private Transform floorTR = null;
        [SerializeField] private AudioMixerGroup bgmAudioMixerGroup = null;
        [Header("★ Audios")]
        [SerializeField] private AudioClip[] correctCLIP = null;
        [SerializeField] private AudioClip wrongCLIP = null;
        [SerializeField] private AudioClip feverHitCLIP = null;
        [Header("★ Config")]
        [SerializeField] private float normalVol = 0.4f;
        [SerializeField] private float feverVol = 0.8f;
        [SerializeField] private float musicPostDelay = 1f;
        [Header("★ DEV - music")]
        [SerializeField][ReadOnly] private Music currentMusic;
        [SerializeField][ReadOnly] private float elapsed;

        // Unity Messages
        private void Awake()
        {
            Util.RemoveAllChildren(noteScroller.transform);

            audioSource.playOnAwake = false;
            audioSource.outputAudioMixerGroup = bgmAudioMixerGroup;
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            EventBus.Subscribe<EventBus.NoteCorrect>(note_OnCorrect);
            EventBus.Subscribe<EventBus.NoteWrong>(note_OnWrong);
            EventBus.Subscribe<EventBus.NoteHit>(note_OnHit);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<EventBus.NoteCorrect>(note_OnCorrect);
            EventBus.Unsubscribe<EventBus.NoteHit>(note_OnHit);
        }

        // Unity Coroutine
        IEnumerator coPlayMusic()
        {
            using (LOG.Coroutine($"{nameof(coPlayMusic)}()", this))
            {
                var music = roundData.Music;
                var dropDuration = noteDropHeight / music.Speed;
                var musicSheet = builder.Build(roundData, dropDuration);
                yield return null;

                playMusic(music.MusicCLIP);
                yield return new WaitForSeconds(music.BeginningSkipTime - dropDuration);

                var notes = new Queue<MusicNote>(musicSheet.Notes);
                var noteCount = notes.Count;
                for (var i = 0; i < noteCount; i++)
                {
                    if (isFever)
                        spawnFeverNote(i);
                    else
                    {
                        var note = notes.Dequeue();
                        if (note.Cmd == MusicNote.Command.Text && isProblemCorrectAll(note.PNO - 1))
                            note = MusicNote.Music(note.NoteNO);
                        spawnNote(note);
                    }
                    yield return new WaitForSeconds(music.BeatInterval * music.BeatSetp);
                }

                yield return new WaitForSeconds(dropDuration);
                yield return new WaitForSeconds(musicPostDelay);

                audioSource.Stop();
                yield return null;
            }
        }
    }
}