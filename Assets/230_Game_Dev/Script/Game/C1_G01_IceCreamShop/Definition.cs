using UnityEngine;

namespace DoDoEng.Game.C1_G01
{
    public class Definition
    {
        // Methods
        public static Color TextColorFrom(int colorID)
        {
            return textColors[colorID - 1];
        }



        // Fields
        private static Color[] textColors = new Color[]
        {
            new Color32(0xFA, 0x51, 0xB5, 0xFF),
            new Color32(0xFF, 0xA7, 0x0B, 0xFF),
            new Color32(0x4D, 0xBF, 0xF9, 0xFF),
            new Color32(0xA2, 0x37, 0xBC, 0xFF),
            new Color32(0x4B, 0xAA, 0x00, 0xFF),
            new Color32(0x47, 0x27, 0x00, 0xFF)
        };
        private static Color[] iceCreamColors = new Color[]
        {
            new Color32(0xFE, 0xD3, 0xE2, 0xFF),
            new Color32(0xFD, 0xF5, 0xC1, 0xFF),
            new Color32(0xD3, 0xE8, 0xFF, 0xFF),
            new Color32(0xF0, 0xD4, 0xFC, 0xFF),
            new Color32(0xDC, 0xFF, 0xBB, 0xFF),
            new Color32(0xD3, 0x71, 0x5B, 0xFF)
        };
    }
}