using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DoDoEng.Launcher.UI
{
    public class ParentsOnlyButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // Definitions

        // Methods
        public void Init(int num)
        {
            this.num = num;
            num1TMP.text = num.ToString();
            num2TMP.text = num.ToString();

            pressedGO.SetActive(false);
        }

        // Events
        public Action<int> OnPress;
        public Action<int> OnRelease;



        // Fields
        private int num;


        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject pressedGO = null;
        [SerializeField] private TMP_Text num1TMP = null;
        [SerializeField] private TMP_Text num2TMP = null;

        // Unity Messages
        private void Awake()
        {
            pressedGO.SetActive(false);
        }
        private void Start()
        {
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            LOG.Function(this);

            pressedGO.SetActive(true);

            OnPress?.Invoke(num);
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            LOG.Function(this);

            pressedGO.SetActive(false);

            OnRelease?.Invoke(num);
        }
    }
}