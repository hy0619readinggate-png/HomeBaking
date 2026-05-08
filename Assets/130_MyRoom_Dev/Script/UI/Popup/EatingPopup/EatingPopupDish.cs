using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Reflection;

// devBOX - 삭제해야 함
#pragma warning disable 0414

namespace DoDoEng.MyRoom.UI.Popup
{
	public class EatingPopupDish : MonoBehaviour
	{
        // Definitions
        // Properties
        public int Index => index;
        public GameObject Food => foods[index];
        public int Point => point;
        public int Coin => coin;

        // Methods
        public void Init(int index, int point, int coin)
        {
            LOG.Function(this, $"{index}");

            this.index = index;
            this.point = point;
            this.coin = coin;

            for (int i = 0; i < foods.Length; i++)
            {
                foods[i].SetActive(i == index);
            }

            upTMP.text = $"{point}p UP!";
            coinTMP.text = $"x{coin}";
        }
        public void ShowFood(bool value)
        {
            foods[index].SetActive(value);
        }

        // Events
        public Action<EatingPopupDish> OnMouseDown;



        // Fields : caching

        // Fields
        private int index;
        private int point;
        private int coin;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] foods = null;
        [SerializeField] private TMP_Text upTMP = null;
        [SerializeField] private TMP_Text coinTMP = null;

        // Unity Messages
        private void Awake()
		{
            //GetComponent<Button>().onClick.AddListener(() => OnMouseDown?.Invoke(this));
        }
		private void Start()
		{
		}
        protected void OnEnable()
        {
        }
        protected void OnDisable()
        {
        }

        // Unity Coroutine
	}
}