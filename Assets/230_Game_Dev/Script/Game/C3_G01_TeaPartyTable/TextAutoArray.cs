using System.Collections.Generic;
using UnityEngine;

namespace DoDoEng.Game.C3_G01
{
    public class TextAutoArray : MonoBehaviour
    {

        // Methods
        public void CheckText(char[] letters)
        {
            spellTotalLists.Clear();
            float height = 0f;


            foreach (var letter in letters)
            {
                if (letter == 'k' || letter == 'l')
                {
                    if (spellTotalLists.Contains(letter) == false)
                    {
                        spellTotalLists.Add(letter);
                        height += downValue;
                    }
                }
                else if (letter == 'g' || letter == 'j')
                {
                    if (spellTotalLists.Contains(letter) == false)
                    {
                        spellTotalLists.Add(letter);
                        height += upValue;
                    }
                }

            }

            if (_TargetTR != null)
                _TargetTR.localPosition = new Vector3(0, height, 0f);

        }


        // Fields
        private List<char> spellTotalLists = new List<char>();


        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private Transform _TargetTR = null;
        [Header("¡Ú Config")]
        [SerializeField] private float upValue = 5;
        [SerializeField] private float downValue = -5f;


    }
}