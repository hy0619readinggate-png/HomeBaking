using beyondi.Util;
using DoDoEng.Common;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A01
{
    public enum ToolType { NA = -1, Cream, Topping }

    [RequireComponent(typeof(Toggle))]
    [RequireComponent(typeof(AffBase))]
    public class PaletteItem : MonoBehaviour, IID
    {
        // Properties
        public bool Selected
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }
        public ToolType ToolType => toolType;
        public BrushInfo BrushInfo => brushInfo;

        // Events
        public event Action<PaletteItem> OnSelected;



        // Fields : caching
        private Toggle toggle_ = null;
        private Toggle toggle => toggle_ ??= GetComponent<Toggle>();
        private AffBase aff_ = null;
        private AffBase aff => aff_ ??= GetComponent<AffBase>();

        // Event Handlers
        private void toggle_onValueChanged(bool isOn)
        {
            if (isOn)
                OnSelected?.Invoke(this);
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private ToolType toolType = ToolType.NA;
        [SerializeField] private BrushInfo brushInfo = null;

        // Unity Messages
        private void Awake()
        {
            toggle.onValueChanged.AddListener(toggle_onValueChanged);

            aff.Enabler = () => !Selected;
        }
        private void Start()
        {
        }



        // Interface : IID
        public int ID { get; set; }
    }

    [Serializable]
    public class BrushInfo
    {
        // Properties
        public float Angle
        {
            get
            {
                if (!RandomAngle)
                    return 0;

                return UnityEngine.Random.Range(RandomAngleMin, RandomAngleMax);
            }
        }
        public float Scale
        {
            get
            {
                if (!RandomScale)
                    return 1;

                return UnityEngine.Random.Range(RandomScaleMin, RandomScaleMax);
            }
        }


        // Unity Inspectors
        public XDPaint.Core.PaintTool PaintTool = XDPaint.Core.PaintTool.Brush;
        public Texture SourceTexture = null;
        public FilterMode FilterMode = FilterMode.Bilinear;
        public Color Color = Color.white;
        [Range(0.01f, 8)]
        public float Size = 0.2f;
        [Range(0.0001f, 1)]
        public float Hardness = 0.9f;
        [Header("★ Configs - Angle")]
        public bool RandomAngle = false;
        [Range(-360, 360)]
        public float RandomAngleMin = 0;
        [Range(-360, 360)]
        public float RandomAngleMax = 0;
        [Header("★ Configs - Scale")]
        public bool RandomScale = false;
        [Range(0.5f, 1.5f)]
        public float RandomScaleMin = 1;
        [Range(0.5f, 1.5f)]
        public float RandomScaleMax = 1;

        // Override
        public override string ToString()
        {
            return $"{Color}";
        }
    }
}