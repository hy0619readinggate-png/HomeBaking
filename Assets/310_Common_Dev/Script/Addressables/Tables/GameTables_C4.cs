using FlexFramework.Excel;
using System;

namespace DoDoEng.Common
{
    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C4_G01 : GameData
    {
        [Column("D")] public string SoundWord;
        [Column("C")] public string Image;
        [Column("E")] public string Word;
        public override string ToString()
        {
            return
                $"C4_G01({Index}) " +
                $"{Word} | {SoundWord} | {Image}";
        }
    }



    // Index    Sentence chunk1 ~ chunk5    Blank_Index    SoundSentence
    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C4_G02 : GameData
    {
        [Column("B")] public string Sentence;
        [Column("C")] public string Chunk1;
        [Column("D")] public string Chunk2;
        [Column("E")] public string Chunk3;
        [Column("F")] public string Chunk4;
        [Column("G")] public string Chunk5;
        [Column("H")] public int Blank_Index;
        [Column("I")] public string SoundSentence;

        public override string ToString()
        {
            return
                $"C4_G02({Index})" +
                $"|{Sentence} |{Chunk1} | {Chunk2}| {Chunk3}| {Chunk4}| {Chunk5}| {Blank_Index}| {SoundSentence}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C4_G03 : GameData
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
         $"C4_G03({Index}) " +
         $"{Sentence} | {Chunk1} | {Chunk2}| {Chunk3}| {Chunk4}| {Chunk5}| {SoundSentence}| {ImageSentence}";
        }

    }

}