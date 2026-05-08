using FlexFramework.Excel;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace DoDoEng.Common
{
    // For Template
    public class GameData_C1_G00 : GameData
    {
        [Column("B")] public string Text1;
        [Column("C")] public string Text2;
        [Column("D")] public string SoundWord;
    }

    //Index Text_1  Text_2 Sound_Word
    //51001	A       a      a
    //51002	B       b      b
    //51003	C       c      c
    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C1_G01 : GameData
    {
        [Column("B")] public string Text1;
        [Column("C")] public string Text2;
        [Column("D")] public string SoundWord;
        [Column("E", "")] public string Avoid;

        public override string ToString()
        {
            return
                $"C1_G01({Index}) " +
                $"{Text1} | {Text2} | {SoundWord} | {Avoid}";
        }

        // Properties
        public string[] AvoidTexts
        {
            get
            {
                var avoids = Avoid.Split(",");
                return avoids.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            }
        }
    }

    //Index Text Sound_Phonetic Avoid
    //52001	a    pho_a          e
    //52002	b    pho_b
    //52003	c    pho_c          k
    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C1_G02 : GameData 
    {
        [Column("B")] public string Text;
        [Column("C")] public string SoundPhonetic;
        [Column("D", "")] public string Avoid;

        public override string ToString()
        {
            return
                $"C1_G02({Index}) " +
                $"{Text} | {SoundPhonetic}";
        }

        // Properties
        public string[] AvoidTexts
        {
            get
            {
                var avoids = Avoid.Split(",");
                return avoids.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            }
        }
    }

    //Index     Image_Word Sound_Word
    //51003001  apple      apple
    //51003002  ant        ant
    //51003003	alligator  alligator
    //51003004	ball       ball
    //51003005	bed        bed
    //51003006	bus        bus
    //51003007	candle     candle
    //51003008	cake       cake
    //51003009	cup        cup
    [Serializable, Table(1, SafeMode = true)]
    public class GameData_C1_G03 : GameData
    {
        [Column("B")] public string ImageWord;
        [Column("C")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C1_G03({Index}) " +
                $"{ImageWord} | {SoundWord}";
        }
    }
}
