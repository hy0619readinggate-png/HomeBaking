using FlexFramework.Excel;
using System;

namespace DoDoEng.Common
{
    // For Template
    public class GameData_C2_G00 : GameData
    {
        [Column("B")] public string Text1;
        [Column("C")] public string Text2;
        [Column("D")] public string SoundWord;
    }

    //Index     Phonics blank_Word Image_Word Sound_Word Sound_Phonetic
    //52001001	b       _at        bat1       bat        phonics_b
    //52001002	b       _ook       book       book       phonics_b
    //52001003	d       _og        dog        dog        phonics_d
    //52001004	d       _oll       doll       doll       phonics_d
    //52001005	g       _ate       gate       gate       phonics_g
    //52001006	g       _oat       goat       goat       phonics_g
    //52001007	h       _at        hat        hat        phonics_h
    //52001008	h       _en        hen        hen        phonics_h
    //52001009	n       _et        net        net        phonics_n
    //52001010	n       _est       nest       nest       phonics_n
    //52001011	p       _anda      panda      panda      phonics_p
    //52001012	p       _up        pup        pup        phonics_p
    //52001013	f       _ish       fish       fish       phonics_f
    //52001014	f       _it        fit        fit        phonics_f
    //52001015	k       _ey        key        key        phonics_k
    //52001016	k       _ing       king       king       phonics_k
    //52001017	t       _able      table      table      phonics_t
    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C2_G01 : GameData
    {
        [Column("B")] public string Phonics;
        [Column("C")] public string BlankWord;
        [Column("D")] public string ImageWord;
        [Column("E")] public string SoundWord;
        [Column("F")] public string SoundPhonetic;

        [Column("G")] public string Word;

        public override string ToString()
        {
            return
                $"C2_G01({Index}) " +
                $"{Phonics} | {Word} | {BlankWord} | {ImageWord} | {SoundWord} | {SoundPhonetic}";
        }
    }

    //Index     Text Sound_Phonetic Sound_Word Image_Word
    //52002001	b    phonics_b      bat        bat1
    //52002002	b    phonics_b      book       book
    //52002003	d    phonics_d      dog        dog
    //52002004	d    phonics_d      doll       doll
    //52002005	g    phonics_g      gate       gate
    //52002006	g    phonics_g      goat       goat
    //52002007	h    phonics_h      hat        hat
    //52002008	h    phonics_h      hen        hen
    //52002009	n    phonics_n      net        net
    //52002010	n    phonics_n      nest       nest
    //52002011	p    phonics_p      panda      panda
    //52002012	p    phonics_p      pup        pup
    //52002013	f    phonics_f      fish       fish
    //52002014	f    phonics_f      fit        fit
    //52002015	k    phonics_k      key        key
    //52002016	k    phonics_k      king       king
    //52002017	t    phonics_t      table      table
    //52002018	t    phonics_t      taxi       taxi
    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C2_G02 : GameData
    {
        [Column("B")] public string Text;
        [Column("C")] public string SoundPhonetic;
        [Column("D")] public string SoundWord;
        [Column("E")] public string Image;

        [Column("F")] public string Word;

        public override string ToString()
        {
            return
                $"C2_G02({Index}) " +
                $"{Text} | {SoundPhonetic} | {SoundWord} | {Image} | {Word}";
        }
    }

    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C2_G03 : GameData
    {
        [Column("B")] public string Text;
        //[Column("D")] public string SoundPhonetic;
        [Column("D")] public string SoundWord;
        [Column("C")] public string Image;
        [Column("E")] public string Word;
        public override string ToString()
        {
            return
                $"C2_G03({Index}) " +
                $"{Text} | {SoundWord} | {Image} | {Word}";
        }
    }
}
