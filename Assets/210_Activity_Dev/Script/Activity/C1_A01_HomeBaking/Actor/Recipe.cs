using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A01
{
    [RequireComponent(typeof(Button))]
    public class Recipe : MonoBehaviour
    {
        // Methods
        public void Setup(string text)
        {
            LOG.Info($"Setup() | {text}", this);

            textTMP.text = text;
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI textTMP = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}

