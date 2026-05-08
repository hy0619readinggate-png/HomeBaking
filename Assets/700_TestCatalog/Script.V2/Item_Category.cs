using DoDoEng.Common;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng.Tester
{
    public class Item_Category : MonoBehaviour
    {
        // Properties
        public Category Category { get; set; }

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
            get => tgl.interactable;
            set => tgl.interactable = value;
        }
        public Color Color
        {
            get => img.color;
            set => img.color = value;
        }
        public ToggleGroup ToggleGroup
        {
            get => tgl.group;
            set => tgl.group = value;
        }



        // Fields : caching
        private Toggle tgl_ = null;
        private Toggle tgl => tgl_ ??= GetComponent<Toggle>();
        private Image img_ = null;
        private Image img => img_ ??= GetComponent<Image>();

        // Event Handlers
        private void tgl_OnValueChanged(bool isOn)
        {
            if (isOn)
                EventBus.Raise<EventBus.Cateogry_Select>(Category);
        }



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Text titleTXT = null;
        [SerializeField] private Text descTXT = null;

        // Unity Messages
        private void Awake()
        {
            descTXT.gameObject.SetActive(false);
            tgl.onValueChanged.AddListener(tgl_OnValueChanged);
        }
        private void Start()
        {

        }
    }

    public enum ContentsType { Activity, ReviewGame, Playground, EBook };

    public class Category
    {
    }
    public class Category1 : Category
    {
        public ContentsType Contents;
        public int Course;

        public override string ToString()
        {
            return $"<color=red>[Category1] {Contents} {Course}</color>";
        }
    }
    public class Category2 : Category
    {
    }
    public class Category2_Activity : Category2
    {
        public ActivityID ActivityID;

        public override string ToString()
        {
            return $"<color=red>[Category2(Activity)] {ActivityID}</color>";
        }
    }
    public class Category2_ReviewGame : Category2
    {
        public GameID GameID;

        public override string ToString()
        {
            return $"<color=red>[Category2(ReviewGame)] {GameID}</color>";
        }
    }
    public class Category2_Playground : Category2
    {
        public GameID GameID;

        public override string ToString()
        {
            return $"<color=red>[Category2(Playground)] {GameID}</color>";
        }
    }
    public class Category2_EBook : Category2
    {
        public int MainCategory;
        public int SubCategory;
        //public int Num;

        //public int ReadOrRecord;
        //public EBookReadMode ReadMode;

        public override string ToString()
        {
            //return $"<color=red>[Category2(EBook)] {MainCategory} {SubCategory} {Num} {ReadOrRecord} {ReadMode}</color>";
            return $"<color=red>[Category2(EBook)] {MainCategory} </color>";
        }
    }
}