using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

namespace DoDoEng.Common
{
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizationText : MonoBehaviour
    {
        // Properties
        protected string Hierarchy
        {
            get
            {
                Transform tf = transform;
                string hira = tf.name;
                while (tf.parent)
                {
                    tf = tf.parent;
                    hira = System.String.Format("{0}/{1}", tf.name, hira);
                }
                return hira;
            }
        }

        // Methods
        public void SetText(string id)
        {
            ID = id;
            var text = LocalizationMGR.One.GetText(ID);
            if (text != null)
                UpdateText(text);
            //else
            //    LOG.Warning($"there is no key \"{ID}\" in {Hierarchy}", this);
        }
        public void UpdateText(string text)
        {
            tmp.text = Regex.Unescape(text);
        }

        // Fields : caching
        private TMP_Text tmp_;
        private TMP_Text tmp => tmp_ ??= GetComponent<TMP_Text>();



        // Unity Inspectors
        [SerializeField] public string ID;

        private void Awake()
        {
            SetText(ID);
        }
        private void OnValidate()
        {
            SetText(ID);
        }
        private void Start()
        {
        }
    }
}