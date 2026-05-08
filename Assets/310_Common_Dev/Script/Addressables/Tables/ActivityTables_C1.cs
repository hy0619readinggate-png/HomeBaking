using FlexFramework.Excel;
using System;
using System.Linq;

namespace DoDoEng.Common
{
    // For Template
    public class ActivityData_C1_A00 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string SoundWord;
    }

    //Index  Group   Text    Prefabs_Tracing_1               Prefabs_Tracing_2                   Prefabs_Deco_1              Prefabs_Deco_2                  Sound_Text_1    Sound_Text_2    Sound_Text_3
    //101001 1       Aa      prefabs_co1_act4_big_a_tracing  prefabs_co1_act4_small_a_tracing    prefabs_co1_act4_big_a_deco prefabs_co1_act4_small_a_deco   big_a           small_a         a
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A01 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string PrefabsTracing1;
        [Column("E")] public string PrefabsTracing2;
        [Column("F")] public string PrefabsDeco1;
        [Column("G")] public string PrefabsDeco2;
        [Column("H")] public string SoundText1;
        [Column("I")] public string SoundText2;
        [Column("J")] public string SoundText3;

        public override string ToString()
        {
            return
                $"C1_A01({Index},{Group}) " +
                $"{Text} | {PrefabsTracing1} | {PrefabsTracing2} | {PrefabsDeco1} | {PrefabsDeco2} |" +
                $"{SoundText1} | {SoundText2} | {SoundText3}";
        }
    }

    //Index  Group  Text_1  Text_2  Text_3  Sound_Text_1    Sound_Text_2    Sound_Text_3
    //102001 1	    A       a       Aa      big_a           small_a         a
    //102002 1	    B       b       Bb      big_b           small_b         b
    //102003 1	    C       c       Cc      big_c           small_c         c
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A02 : ActivityData
    {
        [Column("C")] public string Text1;
        [Column("D")] public string Text2;
        [Column("E")] public string Text3;
        [Column("F")] public string SoundText1;
        [Column("G")] public string SoundText2;
        [Column("H")] public string SoundText3;

        public override string ToString()
        {
            return
                $"C1_A02({Index},{Group}) " +
                $"{Text1} | {Text2} | {Text3} | " +
                $"{SoundText1} | {SoundText2} | {SoundText3}";
        }
    }

    //Index  Group   Text    Sound_Phonetic
    //103001 1	    a       pho_a
    //103002 1	    b       pho_b
    //103003 1	    c       pho_c
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A03 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string SoundPhonetic;

        public override string ToString()
        {
            return
                $"C1_A03({Index},{Group}) " +
                $"{Text} | {SoundPhonetic}";
        }
    }

    //Index  Group Text Alphabet Trim_Word Prefabs_Painting                Sound_Phonetic  Sound_Word Word
    //104001 1     Aa   a        pple      prefabs_co1_act4_apple_line     pho_a           apple      apple
    //104002 1     Aa   a        lligator  prefabs_co1_act4_alligator_line pho_a           alligator  alligator
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A04 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string Alphabet;
        [Column("E")] public string TrimWord;
        [Column("F")] public string PrefabsPainting;
        [Column("G")] public string SoundPhonetic;
        [Column("H")] public string SoundWord;
        [Column("I")] public string Word;

        public override string ToString()
        {
            return
                $"C1_A04({Index},{Group}) " +
                $"{Text} | {Word} | {Alphabet} | {TrimWord} | {PrefabsPainting} | " +
                $"{SoundPhonetic} | {SoundWord}";
        }
    }

    //Index  Group Text_1 Text_2 Sound Sound_Text_1 Sound_Text_2
    //105001 1	   A      a      a     big_a        small_a
    //105002 1	   B      b      b     big_b        small_b
    //105003 1	   C      c      c     big_c        small_c
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A05 : ActivityData
    {
        [Column("C")] public string Text1;
        [Column("D")] public string Text2;
        [Column("E")] public string Sound;
        [Column("F")] public string SoundText1;
        [Column("G")] public string SoundText2;

        public override string ToString()
        {
            return
                $"C1_A04({Index},{Group}) " +
                $"{Text1} | {Text2} | {Sound} | " +
                $"{SoundText1} | {SoundText2}";
        }
    }

    //Index  Group Text Sound_Phonetic Sound_Text_1
    //106001 1	   a    pho_a          a
    //106002 1	   b    pho_b          b
    //106003 1	   c    pho_c          c
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A06 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string SoundPhonetic;
        [Column("E")] public string SoundText;

        public override string ToString()
        {
            return
                $"C1_A04({Index},{Group}) " +
                $"{Text} | {SoundPhonetic} | {SoundText}";
        }
    }

    //Index  Group Text Word      Image_Word Sound_Word Sound_Text
    //107001 1	   Aa   apple     apple      apple      a
    //107002 1	   Aa   ant       ant        ant        a
    //107003 1	   Aa   alligator alligator  alligator  a
    //107004 1	   Bb   ball      ball       ball       b
    //107005 1	   Bb   bed       bed        bed        b
    //107006 1	   Bb   bus       bus        bus        b
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A07 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string Word;
        [Column("E")] public string ImageWord;
        [Column("F")] public string SoundWord;
        [Column("G")] public string SoundText;

        public override string ToString()
        {
            return
                $"C1_A04({Index},{Group}) " +
                $"{Text} | {Word} | {ImageWord} | " +
                $"{SoundWord} | {SoundText}";
        }
    }

    //Index    Group Word      Alphabet Exam1 Exam2 Exam3 Exam4 Image_Word_1 Sound_Word
    //11008001 1     apple     A        A     B     C     D     apple        apple
    //11008002 1     ant       A        A     B     C     D     ant          ant
    //11008003 1     alligator A        A     B     C     D     alligator    alligator
    //11008004 1     ball      B        A     B     C     D     ball         ball
    //11008005 1     bed       B        A     B     C     D     bed          bed
    //11008006 1     bus       B        A     B     C     D     bus          bus
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A08 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("I")] public string Image;
        [Column("J")] public string SoundWord;
        [Column("D")] public string Alphabet;
        [Column("E")] public string Exam1;
        [Column("F")] public string Exam2;
        [Column("G")] public string Exam3;
        [Column("H")] public string Exam4;

        public override string ToString()
        {
            return
                $"C1_A04({Index},{Group}) " +
                $"{Text} | {Image} | {SoundWord} | {Alphabet} | {string.Join(",", Exams)}";
        }

        // Properties
        public string[] Exams
        {
            get
            {
                var exams = new string[] { Exam1, Exam2, Exam3, Exam4 };
                return exams.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            }
        }
    }

    //Index	 Group	Text	Sound_Word
    //109001 1	    A	    big_a
    //109002 1	    a	    small_a
    //109003 1	    B	    big_b
    //109004 1	    b	    small_b
    //109005 1	    C	    big_c
    //109006 1	    c	    small_c
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A09 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string SoundWord;
        [Column("E")] public string Examples;

        public override string ToString()
        {
            return
                $"C1_A04({Index},{Group}) " +
                $"{Text} | {SoundWord}";
        }

        // Properties
        public string[] ExampleTexts
        {
            get
            {
                var exams = Examples.Split(",");
                return exams.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            }
        }
    }

    //Index  Group  Text    Sound_Phonetic  Sound_Text_1
    //110001 1      a       pho_a           a
    //110002 1      b       pho_b           b
    //110003 1      c       pho_c           c

    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A10 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string SoundPhonetic;
        [Column("E")] public string SoundText;

        public override string ToString()
        {
            return
                $"C1_A04({Index},{Group}) " +
                $"{Text} | {SoundPhonetic} | {SoundText}";
        }
    }

    //Index  Group Text_1 Sound_Phonetic
    //111001 1	   a      pho_a
    //111002 1	   b      pho_b
    //111003 1	   c      pho_c
    //111004 1	   d      pho_d
    //111005 1	   e      pho_e
    //111006 1	   f      pho_f
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A11 : ActivityData
    {
        [Column("C")] public string Text1;
        [Column("D")] public string SoundPhonetic;
    }


    //Index    Group Text      Image_Word Sound_Word
    //11012001 1     apple     apple      apple
    //11012002 1     ant       ant        ant
    //11012003 1     alligator alligator  alligator
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C1_A12 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string ImageWord;
        [Column("E")] public string SoundWord;
    }
}
