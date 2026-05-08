using beyondi.Util;
using DoDoEng.Activity.C2_A08;
using DoDoEng.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DoDoEng
{
    public class OutroBottles : MonoBehaviour
    {
        // Methods
        public void Init()
        {
            LOG.Info($"Init()", this);

            bottleGOs.ForEach(go => go.gameObject.SetActive(false));
            index = 0;
            sprites = new List<Sprite>();
            counts = new List<int>();
        }
        public void Setup(Sprite image, int count)
        {
            LOG.Info($"Setup()", this);

            sprites.Add(image);
            counts.Add(count);
            index++;
        }
        public void Show()
        {
            LOG.Info($"Show() | {index} bottles", this);

            for (int i = 0; i < index; i++)
            {
                bottleGOs[i].gameObject.SetActive(true);
                decoIMG[i].sprite = sprites[i];
                bottlesFirefly[i].Setup(counts[i]);
            }
            sprites.Clear();
            counts.Clear();
        }



        // Fields
        private int index = 0;
        private List<Sprite> sprites;
        private List<int> counts;



        // Unity Inspectors
        [Header("★ Bindings")]
        [SerializeField] private GameObject[] bottleGOs = null;
        [SerializeField] private Image[] decoIMG = null;
        [SerializeField] private FireflyInBottle[] bottlesFirefly = null;

        // Unity Messages
        private void Awake()
        {
        }
        private void Start()
        {
        }
    }
}