using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A04
{
    [RequireComponent(typeof(Toggle))]
    public class PaletteItem : MonoBehaviour
    {
        // Properties
        public bool Selected
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }
        public BrushInfo BrushInfo => brushInfo;

        // Events
        public event Action<PaletteItem> OnSelected;



        // Fields : caching
        private Toggle toggle => GetComponent<Toggle>();

        // Event Handlers
        private void toggle_onValueChanged(bool isOn)
        {
            if (isOn)
                OnSelected?.Invoke(this);
        }



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private BrushInfo brushInfo = null;
        [SerializeField] private Image img = null;

        // Unity Messages
        private void Awake()
        {
            toggle.onValueChanged.AddListener(toggle_onValueChanged);

            if (img != null)
                brushInfo.Color = img.color;
        }
        private void Start()
        {
        }
    }

    [Serializable]
    public class BrushInfo
    {
        // Unity Inspectors
        public XDPaint.Core.PaintTool PaintTool = XDPaint.Core.PaintTool.Brush;
        public Texture SourceTexture = null;
        public FilterMode FilterMode = FilterMode.Bilinear;
        public Color Color = Color.white;
        [Range(0.01f, 8)]
        public float Size = 0.2f;
        [Range(0.0001f, 1)]
        public float Hardness = 0.9f;
        [Range(0, 360)]
        public float RenderAngle = 0;
        public bool RandomRenderAngle = false;



        // Override
        public override string ToString()
        {
            return $"{Color}";
        }
    }
}