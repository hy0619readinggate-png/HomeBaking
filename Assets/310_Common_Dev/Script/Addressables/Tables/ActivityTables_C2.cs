using FlexFramework.Excel;
using System;
using System.Linq;

namespace DoDoEng.Common
{
    public class ActivityData_C2_A00 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C2_A00({Index},{Group}) " +
                $"{Text} | {SoundWord}";
        }
    }


    //Index    Group Phonics Word                        Image Sound_Word Word
    //12001001 1     b       <color=#F4284F>b</color>at  bat   bat        bat
    //12001002 1     b       <color=#F4284F>b</color>ook book  book       book
    //12001003 1     d       <color=#F4284F>d</color>og  dog   dog        dog
    //12001004 1     d       <color=#F4284F>d</color>oll doll  doll       doll
    //12001005 1     g       <color=#F4284F>g</color>ate gate  gate       gate
    //12001006 1     g       <color=#F4284F>g</color>oat goat  goat       goat
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A01 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string Image;
        [Column("F")] public string SoundWord;
        [Column("F")] public string WordSTR;

        public override string ToString()
        {
            return
                $"C2_A01({Index},{Group}) " +
                $"{Phonics} | {Word} | {Image}| {SoundWord} | {SoundWord}";
        }
    }

    //Index    Group Phonics Word                        Text_1 Text_2 Text_3 Highlight Fix Image_Word Sound_Text1 Sound_Text2 Sound_Text3 Sound_Word
    //12002001 1     b       <color=#F4284F>b</color>at  b      at            1         T   bat1       phonics_b                           bat
    //12002002 1     b       <color=#F4284F>b</color>ook b      ook           1         T   book       phonics_b                           book
    //12002037 1     x       fo<color=#F4284F>x</color>  fo     x             2         T   fox        phonics_x                           fox
    //12002038 1     x       si<color=#F4284F>x</color>  si     x             2         T   six        phonics_x                           six
    //12002043 2     an      f<color=#F4284F>an</color>  f      an            2         F   fan        phonics_f   phonics_an              fan
    //12002044 2     an      p<color=#F4284F>an</color>  p      an            2         F   pan        phonics_p   phonics_an              pan
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A02 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string Text1;
        [Column("F")] public string Text2;
        [Column("G")] public string Text3;
        [Column("H")] public int Highlight;
        [Column("I")] public string Fix;
        [Column("J")] public string ImageWord;
        [Column("K")] public string SoundText1;
        [Column("L")] public string SoundText2;
        [Column("M")] public string SoundText3;
        [Column("N")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C2_A02({Index},{Group}) " +
                $"{Phonics} | {Word} | {Text1} | {Text2} | {Text3} | {Highlight} | {Fix} | {ImageWord} | {SoundText1} | {SoundText2} | {SoundText3} | {SoundWord}";
        }

        // Properties
        public bool IsFix => Fix == "T";
        public PhonicsText[] Texts
        {
            get
            {
                var texts = new string[] { Text1, Text2, Text3 };
                var sounds = new string[] { SoundText1, SoundText2, SoundText3 };
                var phonicsTexts = texts.Select((t, i) => new PhonicsText
                {
                    Text = t,
                    Sound = sounds[i]
                }).ToArray();
                phonicsTexts[Highlight - 1].IsPhonics = true;

                return phonicsTexts.Where(k => !string.IsNullOrEmpty(k.Text)).ToArray();
            }
        }
        public string PhoneticText => Texts.Where(t => t.IsPhonics).Select(t => t.Text).First();
        public string PhoneticSound => Texts.Where(t => t.IsPhonics).Select(t => t.Sound).First();



        // Inner Class
        public class PhonicsText
        {
            public string Text;
            public bool IsPhonics;
            public string Sound;
        }

    }

    //Index    Group Phonics Word                        blank_Word Image_Word Sound_Word Sound_Phonetic
    //12003001 1     b       <color=#F4284F>b</color>at  _at        bat1       bat        phonics_b
    //12003002 1     b       <color=#F4284F>b</color>ook _ook       book       book       phonics_b
    //12003003 1     d       <color=#F4284F>d</color>og  _og        dog        dog        phonics_d
    //12003004 1     d       <color=#F4284F>d</color>oll _oll       doll       doll       phonics_d
    //12003005 1     g       <color=#F4284F>g</color>ate _ate       gate       gate       phonics_g
    //12003006 1     g       <color=#F4284F>g</color>oat _oat       goat       goat       phonics_g

    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A03 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string BlankWord;
        [Column("F")] public string ImageWord;
        [Column("G")] public string SoundWord;
        [Column("H")] public string SoundPhonetic;

        public override string ToString()
        {
            return
                $"C2_A03({Index},{Group}) " +
                $"{Phonics} | {Word} | {BlankWord} | {ImageWord} | {SoundWord} | {SoundPhonetic}";
        }
    }

    //Index    Group Phoncis Word                        image Sound_Word
    //12004001 1     b       <color=#F4284F>b</color>at  bat1  bat
    //12004002 1     b       <color=#F4284F>b</color>ook book  book
    //12004003 1     d       <color=#F4284F>d</color>og  dog   dog
    //12004004 1     d       <color=#F4284F>d</color>oll doll  doll
    //12004005 1     g       <color=#F4284F>g</color>ate gate  gate
    //12004006 1     g       <color=#F4284F>g</color>oat goat  goat
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A04 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string ImageWord;
        [Column("F")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C2_A04({Index},{Group}) " +
                $"{Phonics} | {Word} | {ImageWord} | {SoundWord}";
        }
    }


    //Index    Group Phonics Word                        blank_Word Img_Word Sound_Word Sound_Phonetic
    //12005001 1     b       <color=#F4284F>b</color>at  _at        bat      bat        phonics_b
    //12005002 1     b       <color=#F4284F>b</color>ook _ook       book     book       phonics_b
    //12005003 1     d       <color=#F4284F>d</color>og  _og        dog      dog        phonics_d
    //12005004 1     d       <color=#F4284F>d</color>oll _oll       doll     doll       phonics_d
    //12005005 1     g       <color=#F4284F>g</color>ate _ate       gate     gate       phonics_g
    //12005006 1     g       <color=#F4284F>g</color>oat _oat       goat     goat       phonics_g
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A05 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string BlankWord;
        [Column("F")] public string ImageWord;
        [Column("G")] public string SoundWord;
        [Column("H")] public string SoundPhonetic;
        [Column("I")] public string Examples;

        public override string ToString()
        {
            return
                $"C2_A05({Index},{Group}) " +
                $"{Phonics} | {Word} | {BlankWord} | {ImageWord} | {SoundWord} | {SoundPhonetic}";
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

    //Index    Group Phonics Word                        image Sound_Word
    //12006001 1     b       <color=#F4284F>b</color>at  bat   bat
    //12006002 1     b       <color=#F4284F>b</color>ook book  book
    //12006003 1     d       <color=#F4284F>d</color>og  dog   dog
    //12006004 1     d       <color=#F4284F>d</color>oll doll  doll
    //12006005 1     g       <color=#F4284F>g</color>ate gate  gate
    //12006006 1     g       <color=#F4284F>g</color>oat goat  goat
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A06 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string ImageWord;
        [Column("F")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C2_A06({Index},{Group}) " +
                $"{Phonics} | {Word} | {ImageWord} | {SoundWord}";
        }
    }

    //Index    Group Phonics Word                        image Sound_Phonetic Sound_Word PhonicsForProblem
    //12007001 1     b       <color=#FFE046>b</color>at  bat   phonics_b      bat        b
    //12007002 1     b       <color=#FFE046>b</color>ook book  phonics_b      book       b
    //12007003 1     d       <color=#FFE046>d</color>og  dog   phonics_d      dog        d
    //12007004 1     d       <color=#FFE046>d</color>oll doll  phonics_d      doll       d
    //12007005 1     g       <color=#FFE046>g</color>ate gate  phonics_g      gate       g
    //12007006 1     g       <color=#FFE046>g</color>oat goat  phonics_g      goat       g
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A07 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string ImageWord;
        [Column("F")] public string SoundPhonetic;
        [Column("G")] public string SoundWord;
        [Column("I")] public string PhonicsForProblem;

        public override string ToString()
        {
            return
                $"C2_A07({Index},{Group}) " +
                $"{Phonics} | {Word}| {ImageWord}| {SoundPhonetic}| {SoundWord} | {PhonicsForProblem}";
        }
    }

    //Index    Group Phonics Word                        image Sound_Phonetic Sound_Word
    //12008001 1     b       <color=#FFE046>b</color>at  bat   phonics_b      bat
    //12008002 1     b       <color=#FFE046>b</color>ook book  phonics_b      book
    //12008003 1     d       <color=#FFE046>d</color>og  dog   phonics_d      dog
    //12008004 1     d       <color=#FFE046>d</color>oll doll  phonics_d      doll
    //12008005 1     g       <color=#FFE046>g</color>ate gate  phonics_g      gate
    //12008006 1     g       <color=#FFE046>g</color>oat goat  phonics_g      goat
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A08 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string ImageWord;
        [Column("F")] public string SoundPhonetic;
        [Column("G")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C2_A08({Index},{Group}) " +
                $"{Phonics} | {Word} | {ImageWord} | {SoundPhonetic} | {SoundWord}";
        }
    }


    //Index    Group   Phonics Word                        image Sound_Phonetic Sound_Word
    //12009001 1       b       <color=#FFE046>b</color>at  bat   phonics_b      bat
    //12009002 1       b       <color=#FFE046>b</color>ook book  phonics_b      book
    //12009003 1       d       <color=#FFE046>d</color>og  dog   phonics_d      dog
    //12009004 1       d       <color=#FFE046>d</color>oll doll  phonics_d      doll
    //12009005 1       g       <color=#FFE046>g</color>ate gate  phonics_g      gate
    //12009006 1       g       <color=#FFE046>g</color>oat goat  phonics_g      goat
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A09 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string ImageWord;
        [Column("F")] public string SoundPhonetic;
        [Column("G")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C2_A09({Index},{Group}) " +
                $"{Phonics} | {Word} | {ImageWord} | {SoundPhonetic} | {SoundWord}";
        }
    }

    //Index    Group Phonics Word                        image Sound_Word
    //12010001 1     b       <color=#F4284F>b</color>at  bat   bat
    //12010002 1     b       <color=#F4284F>b</color>ook book  book
    //12010003 1     d       <color=#F4284F>d</color>og  dog   dog
    //12010004 1     d       <color=#F4284F>d</color>oll doll  doll
    //12010005 1     g       <color=#F4284F>g</color>ate gate  gate
    //12010006 1     g       <color=#F4284F>g</color>oat goat  goat
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A10 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string ImageWord;
        [Column("F")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C2_A10({Index},{Group}) " +
                $"{Phonics} | {Word} | {ImageWord} | {SoundWord}";
        }
    }


    //Index    Group Phonics Word                        blank_Word Img_Word Sound_Word Sound_Phonetic
    //12011001 1     b       <color=#F4284F>b</color>at  _at        bat      bat        phonics_b
    //12011002 1     b       <color=#F4284F>b</color>ook _ook       book     book       phonics_b
    //12011003 1     d       <color=#F4284F>d</color>og  _og        dog      dog        phonics_d
    //12011004 1     d       <color=#F4284F>d</color>oll _oll       doll     doll       phonics_d
    //12011005 1     g       <color=#F4284F>g</color>ate _ate       gate     gate       phonics_g
    //12011006 1     g       <color=#F4284F>g</color>oat _oat       goat     goat       phonics_g
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A11 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string BlankWord;
        [Column("F")] public string ImageWord;
        [Column("G")] public string SoundWord;
        [Column("H")] public string SoundPhonetic;

        public override string ToString()
        {
            return
                $"C2_A11({Index},{Group}) " +
                $"{Phonics} | {Word} | {BlankWord} | {ImageWord} | {SoundWord} | {SoundPhonetic}";
        }
    }

    //Index    Group Phonics Word                        Img_Word Sound_Word
    //12012001 1     b       <color=#F4284F>b</color>at  bat      bat
    //12012002 1     b       <color=#F4284F>b</color>ook book     book
    //12012003 1     d       <color=#F4284F>d</color>og  dog      dog
    //12012004 1     d       <color=#F4284F>d</color>oll doll     doll
    //12012005 1     g       <color=#F4284F>g</color>ate gate     gate
    //12012006 1     g       <color=#F4284F>g</color>oat goat     goat
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C2_A12 : ActivityData
    {
        [Column("C")] public string Phonics;
        [Column("D")] public string Word;
        [Column("E")] public string ImageWord;
        [Column("F")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C2_A12({Index},{Group}) " +
                $"{Phonics} | {Word} | {ImageWord} | {SoundWord}";
        }
    }
}