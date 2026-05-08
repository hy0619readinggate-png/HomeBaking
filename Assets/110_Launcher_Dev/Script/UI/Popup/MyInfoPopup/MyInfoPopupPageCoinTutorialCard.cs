using beyondi.Util;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DoDoEng.Common;
using System.Text.RegularExpressions;

namespace DoDoEng.Launcher.UI
{
	public class MyInfoPopupPageCoinTutorialCard : MonoBehaviour
	{
        // Definitions
        // Properties

        // Methods
        public void Init(string name, string imageName)
        {
            LOG.Function(this, $"| name={name} | imageName={imageName}");
            
            image.sprite = null;
            foreach (var ch in contentRT.GetChildren())
            {
                var line = ch.GetComponent<TMP_Text>();
                if (line != null && line != contentTMP)
                    Destroy(ch.gameObject);
            }
            contentTMP.gameObject.SetActive(false);

            nameTMP.text = name;
            imageName = Regex.Replace(imageName, @"[^a-zA-Z0-9_\s]", "");
            var imagePath = $"Image/MyCoin/{imageName}";
            image.sprite = Resources.Load<Sprite>(imagePath);
            LOG.Info($"image: {image.sprite}", this);
        }
        public void AddText(string text, int coin)
        {
            LOG.Function(this, $"| text={text} | coin={coin}");

            var line = Instantiate(contentTMP, contentRT);
            var plus = coin > 0 ? "+" : "";
            line.text = $"{text} {plus}{coin}";
            line.gameObject.SetActive(true);
        }

        // Events



        // Fields : caching
        // Fields
        // Functions
        // Event Handlers
        // Overrides



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private Image image = null;
        [SerializeField] private TMP_Text nameTMP = null;
        [SerializeField] private RectTransform contentRT = null;
        [SerializeField] private TMP_Text contentTMP = null;

        // Unity Messages
        private void Awake()
		{
            //image.sprite = null;
            //foreach (var ch in contentRT.GetChildren())
            //{
            //    var line = ch.GetComponent<TMP_Text>();
            //    if (line != null && line != contentTMP)
            //        Destroy(ch.gameObject);
            //}
            //contentTMP.gameObject.SetActive(false);
        }
        private void Start()
		{
		}

		// Unity Coroutine
	}
}