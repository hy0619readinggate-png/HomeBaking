using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C2_A07
{
    public class Problem : MonoBehaviour
    {
        // Methods
        public void Setup(ProblemData problem)
        {
            LOG.Info($"Setup()", this);

            phoneticTXT.text = problem.Text;
            phoneticCLIP = problem.PhoneticCLIP;
        }



        // Fields
        private AudioClip phoneticCLIP = null;

        // Event Handlers
        private void button_onClick()
        {
            LOG.Info($"button_onClick()", this);

            AudioMGR.One.PlayNarration(phoneticCLIP);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI phoneticTXT = null;
        [SerializeField] private Button button = null;

        // Unity Messages
        private void Awake()
        {
            button.onClick.AddListener(button_onClick);
        }
        private void Start()
        {
        }
    }
}