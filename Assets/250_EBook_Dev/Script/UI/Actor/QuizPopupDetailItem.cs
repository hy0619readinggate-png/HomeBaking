using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.DoDoEng.EBook.UI
{
    [RequireComponent(typeof(Button))]
    public class QuizPopupDetailItem : MonoBehaviour
    {
        // Methods
        public void Setup(int pNO, bool correct)
        {
            LOG.Function(this, $"{pNO}, correct:{correct}");

            this.pNO = pNO;

            pNOTXT.text = pNO.ToString();
            correctGO.SetActive(correct);
            wrongGO.SetActive(!correct);
        }

        // Events
        public event Action<int> OnClick;



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Fields
        private int pNO = 0;

        // Event Handlers
        private void btn_OnClick()
        {
            LOG.Function(this);

            OnClick?.Invoke(pNO);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TextMeshProUGUI pNOTXT = null;
        [SerializeField] private GameObject correctGO = null;
        [SerializeField] private GameObject wrongGO = null;

        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(btn_OnClick);
        }
        private void Start()
        {
        }
    }
}