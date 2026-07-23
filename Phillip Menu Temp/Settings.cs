using System.Collections.Generic;
using UnityEngine;

namespace Phillip_Menu_Temp
{
    internal class Settings
    {
        public static Color black = new Color32(0, 0, 0, 255);
        public static Color purple = new Color32(51, 28, 112, 255);
        public static Color red = new Color32(207, 0, 0, 255);
        public static Color green = new Color32(0, 214, 0, 255);
        public static Color blue = new Color32(0, 0, 230, 255);
        public static Color white = new Color32(224, 224, 224, 255);
        public static Color clouded = new Color32(176, 176, 176, 20);

        public static List<Color> platColours = new List<Color>
        {
                purple, red, green, blue, white, clouded, black,
        };

        public static List<Color> tpGunColours = new List<Color>
        {
                purple, red, green, blue, white, clouded, black,
        };
    }
}
