using beyondi.Util;
using System.Linq;
using System.Collections;
using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace DoDoEng.MyRoom.UI.Popup
{
	public class EatingPopupTable : MonoBehaviour
	{
        // Definitions
        public enum Swipe { Left, Right };

        // Properties

        // Methods
        public void Init(int[] points, int[] coins)
        {
            LOG.Function(this);

            this.points = points;
            this.coins = coins;
        }
        public void SwipeTable(Swipe swipe)
        {
            LOG.Function(this, $"{swipe}");

            if (swipe == Swipe.Left)
            {
                animator.SetTrigger("left");
                offset = (offset + 1) % dishes.Length;
            }
            else
            {
                animator.SetTrigger("right");
                offset = (offset + dishes.Length - 1) % dishes.Length;
            }
        }
        public void Idle()
        {
            LOG.Function(this);

            for (int i = 0; i < dishes.Length; i++)
            {
                int index = (offset + i) % dishes.Length;
                if (index < points.Length)
                    dishes[i].Init(index, points[index], coins[index]);
                else
                    dishes[i].Init(index, 0, 0);
            }

            OnIdle?.Invoke();
        }

        // Events
        public Action OnIdle;



        // Fields : caching
        private Animator animator_;
        private Animator animator => animator_ ??= GetComponent<Animator>();

        // Fields
        private int[] points;
        private int[] coins;
        private int offset = 0;

        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private EatingPopupDish[] dishes = null;

        // Unity Messages
        private void Awake()
		{
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