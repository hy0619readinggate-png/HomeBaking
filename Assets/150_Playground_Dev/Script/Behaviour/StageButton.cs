using beyondi.Util;
using System.Linq;
using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Playground.Behaviour
{
    public class StageButton : MonoBehaviour
	{
		// Definitions
		public enum StateType
		{
			Disabled,
			Enabled,
			Current
		}

		// Properties
		public int Num => num;
		public StateType State
		{
			get
			{
				return state;
			}
			set
			{
				state = value;
				GetComponent<Animator>().SetTrigger(getStateName(state));
			}
        }
		public int Score
		{
			get
			{ 
				return score;
			}
			set
			{
				score = value;
                foreach (var (star, i) in stars.Select((v, i) => (v, i)))
				{
                    star.SetActive(i >= score);
                }

            }
		}
        public IndexBase Index => index;

        // Methods
        public void Init(int num, IndexBase index)
		{
			this.num = num;
            this.index = index;

            numTXTList.ForEach(t => t.text = num.ToString());
            State = StateType.Disabled;
        }

        // Events
        public event Action<StageButton> OnClick;



        // Fields : caching

        // Fields
        private int num;
		private int score;
		private StateType state;
        private IndexBase index;

        // Functions
        private string getStateName(StateType state)
		{
			if (state == StateType.Disabled) return "Disabled";
            else if (state == StateType.Enabled) return "Enabled";
            else if (state == StateType.Current) return "Current";
			else return "";
        }
		// Event Handlers
		private void button_OnClick()
		{
            LOG.Info($"button_OnClick()", this);

			if (State != StateType.Disabled)
			{
				OnClick?.Invoke(this);
            }
        }

		// Overrides



		// Unity Inspectors
		[Header("★ Bindings")]
        [SerializeField] private TMP_Text[] numTXTList = null;
        [SerializeField] private GameObject[] stars;

        // Unity Messages
        private void Awake()
		{
            numTXTList.ForEach(t => t.text = string.Empty);
			State = StateType.Disabled;

            stars.ForEach(s => s.SetActive(false));

			GetComponent<Button>().onClick.AddListener(button_OnClick);
		}
		private void Start()
		{
		}

		// Unity Coroutine
	}
}