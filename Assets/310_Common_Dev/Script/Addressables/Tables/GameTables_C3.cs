using FlexFramework.Excel;
using System;

namespace DoDoEng.Common
{
    // For Template
    public class GameData_C3_G00 : GameData
    {
        [Column("B")] public string Word;
        [Column("C")] public string LenWord;
        [Column("D")] public string SoundWord;
        [Column("E")] public string SightWord;
    }



    //Index     Word    LenWord     SoundWord   SightWord
    //63101001  I       1           i           I
    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C3_G01 : GameData
    {
        [Column("B")] public string Word;
        [Column("C")] public string WordLength;
        [Column("D")] public string SoundWord;
        [Column("E")] public string SightWord;

        public override string ToString()
        {
            return
         $"C3_G01({Index}) " +
         $"{Word} | {WordLength} | {SoundWord}| {SightWord}";
        }

    }

    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C3_G02 : GameData
    {
        [Column("B")] public string Word;
        [Column("C")] public string SoundWord;
        [Column("D")] public string ImageWord;
        [Column("E")] public string StoryWord;

        public override string ToString()
        {
            return
         $"C3_G02({Index}) " +
         $"{Word} | {SoundWord} | {SoundWord}| {StoryWord}";
        }

    }

    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C3_G03 : GameData
    {
        [Column("B")] public string Sentence;
        [Column("C")] public string Chunk1;
        [Column("D")] public string Chunk2;
        [Column("E")] public string Chunk3;
        [Column("F")] public string Chunk4;
        [Column("G")] public string Chunk5;
        [Column("H")] public string SoundSentence;
        [Column("I")] public string ImageSentence;

        public override string ToString()
        {
            return
         $"C3_G03({Index}) " +
         $"{Sentence} | {Chunk1} | {Chunk2}| {Chunk3}| {Chunk4}| {Chunk5}| {SoundSentence}| {ImageSentence}";
        }

    }




}