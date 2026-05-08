using beyondi.Util;
using Cysharp.Threading.Tasks;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Launcher.UI
{
    public class ParentsOnlyPopup : PopupBase<SimplePopupResult>
    {
        // Definitions
        private static string[] NUMBERS = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };

        // Methods
        public async UniTask<SimplePopupResult> ShowPopup()
        {
            LOG.Function(this);

            answerNum = UnityEngine.Random.Range(0, NUMBERS.Length);
            guideTMP.text = LocalizationMGR.One.GetText("POPUP_24", NUMBERS[answerNum]);
            var randIndice = UtilArray.Random(0, 9, buttons.Length, answerNum);
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Init(randIndice[i]);
            }
            sliderIMG.fillAmount = 0;
            pressedTime = pressTime;

            return await showPopup();
        }



        // Event Handlers
        private void button_onPress(int num)
        {
            LOG.Function(this, $"| num={num}");

            if (answerNum == num)
            {
                pressed = true;
            }
            else
            {
                CloseWithResult(SimplePopupResult.No);
            }
        }
        private void button_onRelease(int num)
        {
            LOG.Function(this);

            if (answerNum == num)
            {
                pressed = false;
                pressedTime = pressTime;
            }
        }

        // Fields
        private int answerNum;
        private float pressedTime = 0;
        private bool pressed = false;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private TMP_Text guideTMP = null;
        [SerializeField] private ParentsOnlyButton[] buttons = null;
        [SerializeField] private Image sliderIMG = null;
        [Header("★ Config")]
        [SerializeField] private float pressTime = 3.0f;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
        private void OnEnable()
        {
            foreach (var button in buttons)
            {
                button.OnPress += button_onPress;
                button.OnRelease += button_onRelease;
            }
        }
        private void OnDisable()
        {
            foreach (var button in buttons)
            {
                button.OnPress -= button_onPress;
                button.OnRelease -= button_onRelease;
            }
        }
        private void Update()
        {
            if (pressed)
            {
                pressedTime -= Time.deltaTime;
                if (pressedTime < 0)
                {
                    pressed = false;
                    pressedTime = 0;
                    CloseWithResult(SimplePopupResult.Yes);
                }
            }
            sliderIMG.fillAmount = pressedTime / pressTime;
        }
    }
}