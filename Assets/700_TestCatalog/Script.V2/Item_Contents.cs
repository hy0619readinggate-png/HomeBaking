using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Tester
{
    public class Item_Contents : MonoBehaviour
    {
        // Properties
        public IndexBase Index { get; set; }

        // Properties
        public string Title
        {
            get => titleTXT.text;
            set => titleTXT.text = value;
        }
        public string Description
        {
            get => descTXT.text;
            set
            {
                descTXT.text = value;
                descTXT.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }
        public bool Enabled
        {
            get => btn.interactable;
            set => btn.interactable = value;
        }
        public Color Color
        {
            get => img.color;
            set => img.color = value;
        }



        // Fields : caching
        private Button btn_ = null;
        private Button btn => btn_ ??= GetComponent<Button>();
        private Image img_ = null;
        private Image img => img_ ??= GetComponent<Image>();

        // Event Handlers
        private void btn_OnClick()
        {
            EventBus.Raise<EventBus.Contents_Select>(Index);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Text titleTXT = null;
        [SerializeField] private Text descTXT = null;

        // Unity Messages
        private void Awake()
        {
            descTXT.gameObject.SetActive(false);
            btn.onClick.AddListener(btn_OnClick);
        }
        private void Start()
        {

        }
    }
}