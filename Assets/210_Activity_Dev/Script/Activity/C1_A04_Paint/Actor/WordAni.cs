using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C1_A04
{
    // Set AnimationNameAttribute to Animator Trigger Name
    public enum WordAnimation
    {
        [DefaultAnimation]
        [Animation("hidden")] Hidden,
        [Animation("word1")] Phonetic,
        [Animation("word2")] Word,
        [Animation("completion")] Completion,
    }
    public class WordAni : AnimationMecanim<WordAnimation>
    {
        // Methods
        public void Setup(string word, string alphabet, string trimWord)
        {
            LOG.Info($"Setup() | {word} | {alphabet} | {trimWord}", this);

            wordTXT.text = word;
            alphabetTXT.text = alphabet;
            trimWordTXT.text = trimWord;
        }

        // Unity Inspectors
        [SerializeField] private TextMeshProUGUI wordTXT;
        [SerializeField] private TextMeshProUGUI alphabetTXT;
        [SerializeField] private TextMeshProUGUI trimWordTXT;
    }
}