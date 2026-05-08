using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DoDoEng.Game.C2_G01
{
    public class NoteBuilder : MonoBehaviour
    {
        // Methods
        public MusicSheet Build(RoundData roundData, float dropDuration)
        {
            LOG.Info($"{nameof(Build)}() | {roundData}", this);

            var duration = roundData.Music.Duration;
            var beginningSkipTime = roundData.Music.BeginningSkipTime;
            var endingSkipTime = roundData.Music.EndingSkipTime;
            var interval = roundData.Music.BeatInterval;
            var beatStep = roundData.Music.BeatSetp;
            var readyNoteCount = roundData.Music.ReadyBeatCount;
            var wrongTextNoteCount = roundData.Music.WrongTextBeatCount;
            var feverTime = roundData.Music.FeverTime;

            var problemCount = roundData.Problems.Length;
            var beatCount = Mathf.FloorToInt((duration - beginningSkipTime - endingSkipTime) / interval / beatStep);
            var beatProblem = 1 + readyNoteCount + 3 + wrongTextNoteCount;
            var beatFever = Mathf.CeilToInt(feverTime / interval);
            var beatRemain = beatCount - musicBeatCountForOpening - problemCount * beatProblem - beatFever;

            var musinInProblemCount = Mathf.Max(0, Mathf.Min(3, beatRemain / 3));
            beatProblem += musinInProblemCount;
            beatRemain -= musinInProblemCount * 3;
            var beatInterval = beatRemain / problemCount;

            LOG.VeryImportant($"beatCount {beatCount}", this);
            LOG.VeryImportant($"musicBeatCountForOpening {musicBeatCountForOpening}", this);
            LOG.VeryImportant($"readyNoteCount {readyNoteCount}", this);
            LOG.VeryImportant($"wrongTextNoteCount : {wrongTextNoteCount}", this);
            LOG.VeryImportant($"beatProblem {beatProblem}", this);
            LOG.VeryImportant($"musinInProblemCount {musinInProblemCount}", this);
            LOG.VeryImportant($"beatFever {beatFever}", this);
            LOG.VeryImportant($"beatRemain {beatRemain}", this);

            noteNO = 0;
            var list = new List<MusicNote>();
            list.AddRange(buildMusicNotes(musicBeatCountForOpening));
            for (var i = 0; i < roundData.Problems.Length; i++)
            {
                var problem = roundData.Problems[i];
                list.AddRange(buildProblem(problem, wrongTextNoteCount, readyNoteCount, musinInProblemCount, i + 1));
                list.AddRange(buildMusicNotes(beatInterval));
            }
            list.AddRange(buildMusicNotes(beatFever));

            return new MusicSheet
            {
                BeginningSkipTime = roundData.Music.BeginningSkipTime,
                Notes = list.ToArray()
            };
        }



        // Fields
        private int noteNO = 0;

        // Functions
        private MusicNote[] buildMusicNotes(int count)
        {
            var list = new List<MusicNote>();
            for (var i = 0; i < count; i++)
            {
                var empty = UtilRandom.RandomSuccess(probEmptyInMusic);
                if (empty)
                    list.Add(MusicNote.Empty(++noteNO));
                else list.Add(MusicNote.Music(++noteNO));
            }

            return list.ToArray();
        }
        private MusicNote[] buildProblem(ProblemData problem, int textNoteCount, int readyNoteCount, int musicBeatCount, int pNO)
        {
            var list = new List<MusicNote>();

            // problem
            list.Add(MusicNote.Problem(++noteNO, pNO));

            // wait
            for (var i = 0; i < readyNoteCount; i++)
                list.Add(MusicNote.Empty(++noteNO));

            // example
            var lrTexts = UtilArray.Extract(MusicNote.lrText, 3)
                    .SelectMany(lr => Enumerable.Repeat(lr, 3))
                    .ToArray();

            var pList = new List<MusicNote>();
            // 정답 텍스트 추가
            var answers = problem.Examples.Where(p => p.IsAnswer).SelectMany(p => Enumerable.Repeat(p, 3));
            foreach (var (exam, i) in answers.Select((exam, i) => (exam, i)))
            {
                var note = MusicNote.Text(++noteNO, exam, pNO);
                note.LR = UtilArray.ExtractOne(MusicNote.lrText);
                pList.Add(note);
            }

            // 오답 텍스트 추가
            var wrongs = problem.Examples.Where(p => !p.IsAnswer).ToArray();
            var wrongExtracted = UtilArray.Extract(wrongs, 3);
            foreach (var (exam, i) in wrongExtracted.Select((exam, i) => (exam, i)))
            {
                var note = MusicNote.Text(++noteNO, exam, pNO);
                note.LR = UtilArray.ExtractOne(MusicNote.lrText);
                pList.Add(note);
            }

            // 음악 비트 추가
            for (var i = 0; i < musicBeatCount; i++)
                pList.Add(MusicNote.Music(++noteNO));

            var exams = UtilArray.Shuffled(pList.ToArray());
            list.AddRange(exams);

            exams
                .Where(p => p.IsAnswer)
                .ForEach((i, p) =>
                    {
                        p.ProblemID = i + 1;
                        p.IsLastSeq = i + 1 == 3;
                    });

            list.Last().IsLast = true;

            return list.ToArray();
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private int musicBeatCountForOpening = 5;
        [SerializeField] private float probEmptyInMusic = 0.2f;
    }

    public class MusicSheet
    {
        public float BeginningSkipTime;
        public MusicNote[] Notes;

        public void Log()
        {
            foreach (var note in Notes)
                LOG.Info<MusicSheet>($"{note}");
        }
    }


    public class MusicNote
    {
        // Definition
        public enum Command { Empty, Problem, Music, Text }

        // Properties
        public int NoteNO { get; set; }
        public Command Cmd { get; set; }
        public LR LR { get; set; }
        public int PNO { get; set; }               // for Problem & Text
        public string Phonics { get; set; }        // for Text
        public AudioClip PhonicsCLIP { get; set; } // for Text
        public bool IsAnswer { get; set; }         // for Text
        public bool IsLast { get; set; }           // for Text
        public int ProblemID { get; set; }        // for Problem
        public bool IsLastSeq { get; set; }        // for Problem

        // Methods
        public static MusicNote Empty(int noteNO)
        {
            return new MusicNote
            {
                NoteNO = noteNO,
                Cmd = Command.Empty,
            };
        }
        public static MusicNote Music(int noteNO)
        {
            return new MusicNote
            {
                NoteNO = noteNO,
                Cmd = Command.Music,
                LR = UtilArray.ExtractOne(lrMusic)
            };
        }
        public static MusicNote Problem(int noteNO, int pNO)
        {
            return new MusicNote
            {
                NoteNO = noteNO,
                Cmd = Command.Problem,
                PNO = pNO
            };
        }
        public static MusicNote Text(int noteNO, string text, bool isAnswer, LR lr, int pNO, bool isLast)
        {
            return new MusicNote
            {
                NoteNO = noteNO,
                Cmd = Command.Text,
                LR = UtilArray.ExtractOne(lrText),
                Phonics = text,
                IsAnswer = isAnswer,
                PNO = pNO,
                IsLast = isLast
            };
        }
        public static MusicNote Text(int noteNO, ExampleData example, int pNO)
        {
            return new MusicNote
            {
                NoteNO = noteNO,
                Cmd = Command.Text,
                LR = UtilArray.ExtractOne(lrText),
                PNO = pNO,
                Phonics = example.Phonics,
                PhonicsCLIP = example.PhonicsCLIP,
                IsAnswer = example.IsAnswer
            };
        }



        // Overrides
        public override string ToString()
        {
            var ox = IsAnswer ? "O" : "X";

            return Cmd switch
            {
                Command.Empty => $"[Note-{NoteNO:D3}] {Cmd,-10}",
                Command.Problem => $"[Note-{NoteNO:D3}] {Cmd,-10}",
                Command.Music => $"[Note-{NoteNO:D3}] {Cmd,-10} {LR}",
                Command.Text => $"[Note-{NoteNO:D3}] {Cmd,-10} {LR} {Phonics}({ox}) {IsLast}",
                _ => string.Empty
            };
        }



        // Fields
        public static readonly LR[] lrMusic = new LR[] { LR.L, LR.R, LR.Both };
        public static readonly LR[] lrText = new LR[] { LR.L, LR.R };
    }
}