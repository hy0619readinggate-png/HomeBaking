using beyondi.Behaviour;
using UnityEngine;

namespace DoDoEng.Game.C1_G01
{
    public class ColorTable : BYDSingleton<ColorTable>
    {
        // Methods
        public Color TinTextColor(int colorID) => tinTextColors[colorID - 1];
        public Color OrderTextColor(int colorID) => orderTextColors[colorID - 1];
        public Color OrderIceCreamColor(int colorID) => orderIceCreamColors[colorID - 1];
        public Color OrderShadowColor(int colorID) => orderShadowColors[colorID - 1];
        public Color OrderTextColorDefault => orderTextColorDefault;
        public Color OrderIceCreamColorDefault => orderIceCreamColorDefault;
        public Color OrderShadowColorDefault => orderShadowColorDefault;



        // Unity Inspectors
        [Header("★ Config")]
        [SerializeField] private Color[] tinTextColors = null;
        [SerializeField] private Color[] orderTextColors = null;
        [SerializeField] private Color[] orderIceCreamColors = null;
        [SerializeField] private Color[] orderShadowColors = null;
        [SerializeField] private Color orderTextColorDefault = Color.gray;
        [SerializeField] private Color orderIceCreamColorDefault = Color.white;
        [SerializeField] private Color orderShadowColorDefault = Color.white;
    }
}