using FlexFramework.Excel;
using System;
using System.Linq;

namespace DoDoEng.Common
{
    //Index    Group Words Image_Word Sound_Word Word
    //14006001 1	 visit visit      visit      visit
    //14006002 1	 hero  hero       hero       hero
    //14006003 1	 young young      young      young
    //14006004 1	 carve carve      carve      carve
    //14006005 1	 silly silly      silly      silly
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A01 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("D")] public string ImageWord;
        [Column("E")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C4_A01({Index},{Group}) " +
                $"{Word}";
        }
    }

    //Index    Group Sentence          Sound_Sentence Image_Sentence
    //13006001 1     Hello, I am Dodo. helloiamdodo   helloiamdodo
    //13006002 1     Nice to meet you. nicetomeetyou  nicetomeetyou
    //13006003 1     What's your name? whatsyourname  whatsyourname
    //13006004 1     My name is Leoni. mynameisleoni  mynameisleoni
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A02 : ActivityData
    {
        [Column("C")] public string Sentence;
        [Column("D")] public string SoundSentence;
        [Column("E")] public string ImageSentence;

        public override string ToString()
        {
            return
                $"C4_A02({Index},{Group}) " +
                $"{Sentence} | {SoundSentence}";
        }
    }

    //Index    Group Sentence          Sound_Sentence Image_Sentence
    //13005001 1     Hello, I am Dodo. helloiamdodo   helloiamdodo
    //13005002 1     Nice to meet you. nicetomeetyou  nicetomeetyou
    //13005003 1     What's your name? whatsyourname  whatsyourname
    //13005004 1     My name is Leoni. mynameisleoni  mynameisleoni
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A03 : ActivityData
    {
        [Column("C")] public string Sentence;
        [Column("D")] public string SoundSentence;
        [Column("E")] public string ImageSentence;

        public override string ToString()
        {
            return
                $"C4_A03({Index},{Group}) " +
                $"{Sentence} | {SoundSentence} | {ImageSentence}";
        }
    }

    //Index    Group Words Movie    Image_Motion Sound_Word
    //14004001 1     kick  14004001 kick         kick
    //14004002 1     catch 14004002 catch        catch
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A04 : ActivityData
    {
        [Column("C")] public string Words;
        [Column("D")] public string Movie;
        [Column("F")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C4_A04({Index},{Group}) " +
                $"{Words} | {Movie} | {SoundWord}";
        }
    }

    //Index    Group words Image_word Sound_word Sentence                chunk1 chunk2 chunk3 chunk4    chunk5
    //14005001 1     kick  kick       kick       I kick the soccer ball. I      kick   the    soccer    ball.
    //14005002 1     kick  kick       kick       They kick the rocks.    They   kick   the    rocks.	
    //14005003 1     catch catch      catch      I catch the baseball.   I      catch  the    baseball.	
    //14005004 1     catch catch      catch      Let's catch some fish!  Let's  catch  some   fish!	
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A05 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("D")] public string ImageWord;
        [Column("E")] public string SoundWord;
        [Column("F")] public string Sentence;
        [Column("G")] public string chunk1;
        [Column("H")] public string chunk2;
        [Column("I")] public string chunk3;
        [Column("J")] public string chunk4;
        [Column("K")] public string chunk5;
        [Column("L")] public string SoundSentence;

        public override string ToString()
        {
            return
                $"C4_A05({Index},{Group}) " +
                $"{Sentence} | {chunk1} | {chunk2} | {chunk3} | {chunk4} | {chunk5} | {SoundSentence}";
        }
    }

    //Index    Group Words Image_Word Sound_Word Word
    //14006001 1	 visit visit      visit      visit
    //14006002 1	 hero  hero       hero       hero
    //14006003 1	 young young      young      young
    //14006004 1	 carve carve      carve      carve
    //14006005 1	 silly silly      silly      silly
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A06 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("D")] public string ImageWord;
        [Column("E")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C4_A06({Index},{Group}) " +
                $"{Word}";
        }
    }

    //Index    Group Sentence                    chunk1 chunk2 chunk3   chunk4    chunk5 Blank_Index Sound_Sentence         Image_Sentence
    //14007001 1     Max visits Grampy's house.  Max    visits Grampy's house.           2           maxvisitsgrampyshouse  maxvisitsgrampyshouse
    //14007002 1     Max sees Grampy sleeping.   Max    sees   Grampy   sleeping.		 4           maxseesgrampysleeping  maxseesgrampysleeping
    //14007003 1     Grampy plays music for Max. Grampy plays  music    for       Max.   3           grampyplaysmusicformax grampyplaysmusicformax
    //14007004 1     Grampy plays with the dog.  Grampy plays  with     the       dog.   5           grampyplayswiththedog  grampyplayswiththedog
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A07 : ActivityData
    {
        [Column("C")] public string Sentence;
        [Column("D")] public string chunk1;
        [Column("E")] public string chunk2;
        [Column("F")] public string chunk3;
        [Column("G")] public string chunk4;
        [Column("H")] public string chunk5;
        [Column("I")] public int BlankIndex;
        [Column("J")] public string SoundSentence;
        [Column("K")] public string ImageSentence;

        public override string ToString()
        {
            return
                $"C4_A07({Index},{Group}) " +
                $"{Sentence} | {chunk1} | {chunk2} | {chunk3} | {chunk4} | {chunk5} | {BlankIndex} | {SoundSentence}";
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

    //Index    Group words Image_word Sound_word Sentence                chunk1 chunk2 chunk3 chunk4 chunk5 Sound_Sentence
    //14008001 1     kick  kick       kick       I kick the soccer ball. I      kick   the    soccer ball.  ikickthesoccerball
    //14008002 1     kick  kick       kick       They kick the rocks.    They   kick   the    rocks.        theykicktherocks
    //14008003 1     catch catch      catch      I catch the baseball.   I      catch  the    baseball.     icatchthebaseball
    //14008004 1     catch catch      catch      Let's catch some fish!  Let's  catch  some   fish!         letscatchsomefish
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A08 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("D")] public string ImageWord;
        [Column("E")] public string SoundWord;
        [Column("F")] public string Sentence;
        [Column("G")] public string chunk1;
        [Column("H")] public string chunk2;
        [Column("I")] public string chunk3;
        [Column("J")] public string chunk4;
        [Column("K")] public string chunk5;
        [Column("L")] public string SoundSentence;

        public override string ToString()
        {
            return
                $"C4_A08({Index},{Group}) " +
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

    //Index    Group Words Image_Word Sound_Word
    //14009001 1     visit visit      visit
    //14009002 1     hero  hero       hero
    //14009003 1     young young      young
    //14009004 1     carve carve      carve
    //14009005 1     silly silly      silly
    //14009006 1     sofa  sofa       sofa
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A09 : ActivityData
    {
        [Column("C")] public string Word;
        [Column("D")] public string ImageWord;
        [Column("E")] public string SoundWord;

        public override string ToString()
        {
            return
                $"C4_A09({Index},{Group}) " +
                $"{Word} | {ImageWord} | {SoundWord}";
        }
    }

    //Index    Group Words Image_Word Sound_Word Word
    //14010001 1     visit visit      visit      visit
    //14010002 1     hero  hero       hero       hero
    //14010003 1     young young      young      young
    //14010004 1     carve carve      carve      carve
    //14010005 1     silly silly      silly      silly
    [Serializable, Table(1, SafeMode = true)]
    public class ActivityData_C4_A10 : ActivityData
    {
        [Column("C")] public string Text;
        [Column("D")] public string ImageWord;
        [Column("E")] public string SoundWord;
    }

}
