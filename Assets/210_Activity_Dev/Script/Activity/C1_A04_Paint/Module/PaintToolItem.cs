using System;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Activity.C1_A04
{
    public enum ToolType { NA = -1, Pencil, Brush, Sticker, Eraser }

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Button))]
    public class PaintToolItem : MonoBehaviour
    {
        // Properties
        public ToolType ToolType => toolType;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected == value)
                    return;

                isSelected = value;
                btn.interactable = !isSelected;
                anim.SetTrigger(isSelected ? "appear" : "disappear");

                if (isSelected)
                    OnSelected?.Invoke(this);
            }
        }
        public Color Color
        {
            set
            {
                if (colorIMG != null)
                    colorIMG.color = value;
            }
        }

        // Events
        public event Action<PaintToolItem> OnSelected;



        // Fields : caching
        private Animator anim => GetComponent<Animator>();
        private Button btn => GetComponent<Button>();

        // Fields
        private bool isSelected = false;



        // Unity Inspectors
        [Header("¡Ú Bindings")]
        [SerializeField] private ToolType toolType = ToolType.NA;
        [SerializeField] private Image colorIMG = null;

        // Unity Messages
        private void Awake()
        {
            btn.onClick.AddListener(() => IsSelected = true);
        }
        private void Start()
        {

        }
        private void OnEnable()
        {
            isSelected = false;
        }
    }
}