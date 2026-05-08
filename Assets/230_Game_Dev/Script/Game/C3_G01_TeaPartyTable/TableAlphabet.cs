using UnityEngine;

namespace DoDoEng.Game.C3_G01
{
    public class TableAlphabet : TableObject
    {

        // Methods
        public void ChangeColor(int a)
        {
            colorANI.SetTrigger(a);
        }

        // Overrides
        protected override void setup(char? value)
        {
            if (value == null)
                return;

            letter = value;
            switch (value)
            {
                case 'a': alphabetSR.sprite = alphabetIMG[0]; break;
                case 'b': alphabetSR.sprite = alphabetIMG[1]; break;
                case 'c': alphabetSR.sprite = alphabetIMG[2]; break;
                case 'd': alphabetSR.sprite = alphabetIMG[3]; break;
                case 'e': alphabetSR.sprite = alphabetIMG[4]; break;
                case 'f': alphabetSR.sprite = alphabetIMG[5]; break;
                case 'g': alphabetSR.sprite = alphabetIMG[6]; break;
                case 'h': alphabetSR.sprite = alphabetIMG[7]; break;
                case 'i': alphabetSR.sprite = alphabetIMG[8]; break;
                case 'j': alphabetSR.sprite = alphabetIMG[9]; break;
                case 'k': alphabetSR.sprite = alphabetIMG[10]; break;
                case 'l': alphabetSR.sprite = alphabetIMG[11]; break;
                case 'm': alphabetSR.sprite = alphabetIMG[12]; break;
                case 'n': alphabetSR.sprite = alphabetIMG[13]; break;
                case 'o': alphabetSR.sprite = alphabetIMG[14]; break;
                case 'p': alphabetSR.sprite = alphabetIMG[15]; break;
                case 'q': alphabetSR.sprite = alphabetIMG[16]; break;
                case 'r': alphabetSR.sprite = alphabetIMG[17]; break;
                case 's': alphabetSR.sprite = alphabetIMG[18]; break;
                case 't': alphabetSR.sprite = alphabetIMG[19]; break;
                case 'u': alphabetSR.sprite = alphabetIMG[20]; break;
                case 'v': alphabetSR.sprite = alphabetIMG[21]; break;
                case 'w': alphabetSR.sprite = alphabetIMG[22]; break;
                case 'x': alphabetSR.sprite = alphabetIMG[23]; break;
                case 'y': alphabetSR.sprite = alphabetIMG[24]; break;
                case 'z': alphabetSR.sprite = alphabetIMG[25]; break;

                default: alphabetSR.sprite = null; break;
            }
        }




        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private SpriteRenderer alphabetSR = null;
        [SerializeField] private Sprite[] alphabetIMG = null;
        [SerializeField] private Animator colorANI = null;
    }
}