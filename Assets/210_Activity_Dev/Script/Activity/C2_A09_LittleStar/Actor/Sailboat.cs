using beyondi.Util;
using DoDoEng.Common;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A09
{
    public class Sailboat : MonoBehaviour
    {
        // Properties
        public Edmond Edmond => edmond;

        // Methods
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cloudBTN.interactable = enable;
        }
        public void Setup(ProblemData problem)
        {
            LOG.Info($"Setup() | {problem}", this);

            wordCLIP = problem.WordCLIP;
            phoneticCLIP = problem.PhonicsCLIP;

            problemIMG.sprite = problem.WordSPR;
            problemTXT.text = problem.Word;

            problemIMG.gameObject.SetActive(true);
            problemTXT.gameObject.SetActive(false);
        }
        public void ShowText()
        {
            LOG.Info($"ShowText()", this);

            problemIMG.gameObject.SetActive(false);
            problemTXT.gameObject.SetActive(true);
        }
        public void ShowImage()
        {
            LOG.Info($"ShowImage()", this);

            problemIMG.gameObject.SetActive(true);
            problemTXT.gameObject.SetActive(false);
        }



        // Fields
        private AudioClip wordCLIP = null;
        private AudioClip phoneticCLIP = null;

        // Event Handlers
        private void cloudBTN_onClick()
        {
            LOG.Info($"cloudBTN_onClick()", this);

            AudioMGR.One.PlayNarration(phoneticCLIP, phoneticCLIP, wordCLIP);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Edmond edmond = null;
        [SerializeField] private Image problemIMG = null;
        [SerializeField] private TextMeshProUGUI problemTXT = null;
        [SerializeField] private Button cloudBTN = null;

        // Unity Messages
        private void Awake()
        {
            cloudBTN.interactable = false;
            cloudBTN.onClick.AddListener(cloudBTN_onClick);
        }
        private void Start()
        {
        }
    }
}