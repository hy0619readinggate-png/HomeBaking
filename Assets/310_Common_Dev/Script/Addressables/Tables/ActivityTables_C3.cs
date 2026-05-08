using DoDoEng.Activity.C1_A05;
using DoDoEng.Activity.C1_A10;
using FlexFramework.Excel;
using System;
using System.Linq;

namespace DoDoEng.Common
{
    //Index    Group Words Sound_Word Image_Word
    //13001001 1     hello hello      hello
    //13001002 1     name  name       name
    //13001003 1     nice  nice       nice
    //13001004 1     meet  meet       meet
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A01 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("D")] public string SoundWord;
        [Column("E")] public string ImageWord;

        public override string ToString()
        {
            return
                $"C3_A01({Index},{Group}) " +
                $"{Word} | {SoundWord} | {ImageWord}";
        }
    }


    //Index    Group Words Sound_Word Image_Word Words
    //13002001 1     hello hello      hello      hello
    //13002002 1     name  name       name       name
    //13002003 1     nice  nice       nice       nice
    //13002004 1     meet  meet       meet       meet
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A02 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("D")] public string SoundWord;
        [Column("E")] public string ImageWord;

        public override string ToString()
        {
            return
                $"C3_A02({Index},{Group}) " +
                $"{Word} | {SoundWord} | {ImageWord}";
        }
    }

    //Index    Group Text Sound_Word Words
    //13003001 1     I    i          I
    //13003002 1     am   am         am
    //13003003 1     to   to         to
    //13003004 1     you  you        you
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A03 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C3_A03({Index},{Group}) " +
                $"{Text} | {SoundWord}";
        }
    }

    //Index    Group Sentence          blank_Word          Sound_Sentence Image_Sentence
    //13004001 1     Hello, I am Dodo. |Hello|, I am Dodo. helloiamdodo   helloiamdodo
    //13004002 2     Nice to meet you. Nice to |meet| you. nicetomeetyou  nicetomeetyou
    //13004003 3     What's your name? What's your |name|? whatsyourname  whatsyourname
    //13004004 4     My name is Leoni. My name |is| Leoni. mynameisleoni  mynameisleoni
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A04 : ActivityData
    {
        [Column("C")] public string Sentence;
        [Column("D")] public string BlankWord;
        [Column("E")] public string SoundSentence;
        [Column("F")] public string ImageSentence;
        [Column("G")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C3_A06({Index},{Group}) " +
                $"{Sentence} | {BlankWord} | {SoundSentence} | {SoundWord}";
        }
    }

    //Index    Group Sentence          Sound_Sentence Image_Sentence
    //13005001 1     Hello, I am Dodo. helloiamdodo   helloiamdodo
    //13005002 1     Nice to meet you. nicetomeetyou  nicetomeetyou
    //13005003 1     What's your name? whatsyourname  whatsyourname
    //13005004 1     My name is Leoni. mynameisleoni  mynameisleoni
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A05 : ActivityData
    {
        [Column("C")] public string Sentence;
        [Column("D")] public string SoundSentence;
        [Column("D")] public string ImageSentence;

        public override string ToString()
        {
            return
                $"C3_A05({Index},{Group}) " +
                $"{Sentence} | {SoundSentence} | {ImageSentence}";
        }
    }

    //Index    Group Sentence          Sound_Sentence Image_Sentence
    //13006001 1     Hello, I am Dodo. helloiamdodo   helloiamdodo
    //13006002 1     Nice to meet you. nicetomeetyou  nicetomeetyou
    //13006003 1     What's your name? whatsyourname  whatsyourname
    //13006004 1     My name is Leoni. mynameisleoni  mynameisleoni
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A06 : ActivityData
    {
        [Column("C")] public string Sentence;
        [Column("D")] public string SoundSentence;
        [Column("E")] public string ImageSentence;

        public override string ToString()
        {
            return
                $"C3_A06({Index},{Group}) " +
                $"{Sentence} | {SoundSentence}";
        }
    }

    //Index    Group Word len_Word Sound_Word Words
    //13007001 1     I    1        i          I
    //13007002 1     am   2        am         am
    //13007003 1     to   2        to         to
    //13007004 1     you  3        you        you
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A07 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("E")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C3_A07({Index},{Group}) " +
                $"{Word} | {SoundWord}";
        }
    }

    //Index    Group Words Sound_Word Image_Word
    //13008001 1     hello hello      hello
    //13008002 1     name  name       name
    //13008003 1     nice  nice       nice
    //13008004 1     meet  meet       meet
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A08 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("D")] public string SoundWord;
        [Column("D")] public string ImageWord;

        public override string ToString()
        {
            return
                $"C3_A08({Index},{Group}) " +
                $"{Word} | {SoundWord} | {ImageWord}";
        }
    }

    //Index    Group Sentence          chunk1 chunk2 chunk3 chunk4 chunk5 Sound_Sentence Image_Sentence
    //13009001 1     Hello, I am Dodo. Hello, I      am     Dodo.         helloiamdodo   helloiamdodo
    //13009002 1     Nice to meet you. Nice   to     meet   you.          nicetomeetyou  nicetomeetyou
    //13009003 1     What's your name? What's your   name?                whatsyourname  whatsyourname
    //13009004 1     My name is Leoni. My     name   is     Leoni.        mynameisleoni  mynameisleoni
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C3_A09 : ActivityData
    {
        [Column("C")] public string Sentence;
        [Column("D")] public string chunk1;
        [Column("E")] public string chunk2;
        [Column("F")] public string chunk3;
        [Column("G")] public string chunk4;
        [Column("H")] public string chunk5;
        [Column("I")] public string SoundSentence;
        [Column("J")] public string ImageSentence;

        public override string ToString()
        {
            return
                $"C3_A06({Index},{Group}) " +
                $"{Sentence} | {chunk1} | {chunk2} | {chunk3} | {chunk4} | {chunk5} | {SoundSentence}";
        }

        // Properties
        public string[] Texts
        {
            get
            {
                var texts = new string[] { chunk1, chunk2, chunk3, chunk4, chunk5 };
                return texts.Where(t => !string.IsNullOrEmpty(t)).ToArray();
            }
        }
    }

}
