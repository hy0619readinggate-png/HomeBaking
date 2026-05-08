using beyondi.Util;
using DoDoEng.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A11
{
    public enum FlowerState
    {
        None,
        Idle, Collect, Float, Hidden
    }

    [RequireComponent(typeof(Button))]
    public class Flower : MonoBehaviour, IID
    {
        // Properties
        public bool IsAnswer { get; private set; }
        public int TypeIndex { get; private set; }
        public int ColorIndex { get; private set; }
        public string Alphabet => alphabetTXT.text;
        public bool IsCollectable { get; private set; }
        public bool IsCollected => !IsCollectable;
        public FlowerNormalParam FlowerParam { get; private set; }

        // Properties
        public int ID { get; set; }

        // Methods
        public void Setup(ExampleData examData, int typeIdx, FlowerNormalParam param)
        {
            LOG.Info($"Setup() | {examData.Text}", this);

            FlowerParam = param;

            IsAnswer = examData.IsAnswer;
            alphabetTXT.text = examData.Text;
            IsCollectable = true;
            var colorIdx = Random.Range(0, FlowerParam.colorCount);
            updateSkin(typeIdx, colorIdx);
            updateState(FlowerState.Idle);
        }
        public void SetupFrom(Flower flower)
        {
            LOG.Info($"SetupFrom()", this);

            FlowerParam = flower.FlowerParam;

            IsAnswer = true;
            alphabetTXT.text = flower.alphabetTXT.text;
            updateSkin(flower.TypeIndex, flower.ColorIndex);
            updateState(FlowerState.Float);
        }
        public void EnableInteraction(bool enable)
        {
            LOG.Info($"EnableInteraction() | {enable}", this);

            cg.blocksRaycasts = enable;
        }
        public void Show()
        {
            LOG.Info($"Show()", this);

            updateState(FlowerState.Idle);
        }
        public void Hide()
        {
            LOG.Info($"Hide()", this);

            updateState(FlowerState.Hidden);
        }
        public void Collect()
        {
            LOG.Info($"Collect()", this);

            cg.blocksRaycasts = false;
            IsCollectable = false;
            updateState(FlowerState.Collect);
        }

        // Events
        public event System.Action<Flower> OnClick;



        // Fields : caching
        private CanvasGroup cg_ = null;
        private CanvasGroup cg => cg_ ??= GetComponent<CanvasGroup>();
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();

        // Functions
        private void updateSkin(int typeIdx, int colorIdx)
        {
            TypeIndex = typeIdx;
            ColorIndex = colorIdx;
            flowerAnim.SetSkin(typeIdx, colorIdx);

            if (typeIdx == 0)
                alphabetTXT.color = FlowerParam.petalTextColor;
            if (typeIdx == 1)
            {
                var textColor = colorIdx switch
                {
                    0 => FlowerParam.blueTulipTextColor,
                    1 => FlowerParam.redTulipTextColor,
                    2 => FlowerParam.purpleTulipTextColor,
                    3 => FlowerParam.yellowTulipTextColor,
                    4 => FlowerParam.pinkTulipTextColor,
                    _ => Color.white
                };
                alphabetTXT.color = textColor;
            }
        }
        private void updateState(FlowerState state)
        {
            flowerAnim.gameObject.SetActive(state == FlowerState.Idle || state == FlowerState.Float);

            soilFrontGO.SetActive(state == FlowerState.Idle);
            soilNormalGO.SetActive(state == FlowerState.Idle);
            soilHoleGO.SetActive(state == FlowerState.Collect);

            if (state == FlowerState.Idle)
            {
                var anis = new FlowerAnimation[] { FlowerAnimation.Idle1, FlowerAnimation.Idle2, FlowerAnimation.Idle3 };
                var aniName = UtilArray.ExtractOne(anis);
                flowerAnim.PlayAnimationLoop(aniName);
            }
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private FlowerAni flowerAnim = null;
        [SerializeField] private TextMeshProUGUI alphabetTXT = null;
        [SerializeField] private GameObject soilFrontGO = null;
        [SerializeField] private GameObject soilNormalGO = null;
        [SerializeField] private GameObject soilHoleGO = null;
        
        // Unity Messages
        private void Awake()
        {
            cg.blocksRaycasts = false;

            btn.onClick.AddListener(() => OnClick?.Invoke(this));
        }
        private void Start()
        {
        }
    }
}