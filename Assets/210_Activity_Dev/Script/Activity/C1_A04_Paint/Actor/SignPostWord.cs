using DoDoEng.Common;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C1_A04
{
    public class SignPostWord : MonoBehaviour
    {
        // Methods
        public void Setup(string alphabet, string trimWord)
        {
            LOG.Info($"Setup() | {alphabet} | {trimWord}", this);

            alphabetTXT.text = alphabet;
            trimWordTXT.text = "";
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI alphabetTXT;
        [SerializeField] private TextMeshProUGUI trimWordTXT;

        // Unity Messages
        private void Awake()
        {

        }
        private void Start()
        {

        }
    }
}