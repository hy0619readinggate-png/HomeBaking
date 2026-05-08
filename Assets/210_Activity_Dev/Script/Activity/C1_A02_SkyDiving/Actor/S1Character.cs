using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;

namespace DoDoEng.Activity.C1_A02
{
    public class S1Character : MonoBehaviour
    {
        // Methods
        public void Setup(string text, int characterID)
        {
            LOG.Info($"Setup() | {text} {characterID}", this);

            phoneticTXTs1.ForEach(t => t.text = text);
            phoneticTXTs2.ForEach(t => t.text = text);
            characters.SetActiveOnly(characterID - 1);
        }
        public void ShowInitial()
        {
            LOG.Info($"ShowInitial()", this);

            characters.SetActiveOnly(0);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] characters = null;
        [SerializeField] private TextMeshProUGUI[] phoneticTXTs1 = null;
        [SerializeField] private TextMeshProUGUI[] phoneticTXTs2 = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}