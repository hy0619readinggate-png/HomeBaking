using beyondi.Util;
using System.Linq;
using DoDoEng.Common;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Playground.Behaviour
{
    public class PlaygroundSlot : MonoBehaviour
	{
		// Definitions
		public enum StateType
		{
			Disabled,
			Enabled,
            Previous,
            Current
		}

		// Properties
		public int Num => num;
		public int Course { get; set; }
        public GameIndex Index { get; set; }
		public bool IsComplete { get; set; }
        public StateType State
		{
			get
			{
				return state;
			}
			set
			{
                GetComponent<Animator>().ResetTrigger(getStateName(state));
                GetComponent<Animator>().SetTrigger(getStateName(value));

                state = value;
            }
        }
		public bool Lock { get => lockGO.activeSelf; set {
                numTXTList.ForEach(t => t.gameObject.SetActive(!value));
                lockGO.SetActive(value); 
			} }
		public int Stars
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
                    star.SetActive(i < score);
                }

            }
		}
		public bool EnableInteract
		{
			get { return button.enabled; }
			set {  button.enabled = value; }
		}

        // Methods
		public void Init(int num)
		{
            this.num = num;

            numTXTList.ForEach(t => t.text = num.ToString());

			State = StateType.Disabled;
		}
		public void InitForLMS(int num, int score, bool isComplete)
		{
			this.num = num;

            numTXTList.ForEach(t => t.text = num.ToString());
			Stars = score;
            State = isComplete ? StateType.Enabled : StateType.Disabled;
			Lock = false;
			EnableInteract = false;
        }

        // Events
        public event Action<PlaygroundSlot> OnClick;



		// Fields : caching
		private Button button_;
		private Button button => button_ ??= GetComponent<Button>();

        // Fields
        private int num;
		private int score;
		private StateType state;

        // Functions
        private string getStateName(StateType state)
		{
			if (state == StateType.Disabled) return "Disabled";
            else if (state == StateType.Enabled) return "Enabled";
            else if (state == StateType.Previous) return "Previous";
            else if (state == StateType.Current) return "Current";
            else return "";
        }
		// Event Handlers
		private void button_OnClick()
		{
            LOG.Info($"button_OnClick() | Num={Num} | Index={Index}", this);

            OnClick?.Invoke(this);
        }

		// Overrides



		// Unity Inspectors
		[Header("★ Bindings")]
        [SerializeField] private TMP_Text[] numTXTList = null;
        [SerializeField] private GameObject[] stars;
        [SerializeField] private GameObject lockGO;

        // Unity Messages
        private void Awake()
		{
            numTXTList.ForEach(t => t.text = string.Empty);
			State = StateType.Disabled;

			stars.ForEach(s => s.SetActive(false));

            button.onClick.AddListener(button_OnClick);
		}
		private void Start()
		{
		}

		// Unity Coroutine
	}
}