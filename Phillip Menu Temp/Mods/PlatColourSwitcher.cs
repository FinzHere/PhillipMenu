using System;
using System.Collections.Generic;
using UnityEngine;

namespace Phillip_Menu_Temp.Mods
{
    internal class Switchers
    {
        public static int platColourIndexer = 0;
        public static Color platCurrentColor = Phillip_Menu_Temp.Settings.platColours[platColourIndexer];

        public static void PlatColourChanger()
        {
            platColourIndexer++;
            if (platColourIndexer >= Phillip_Menu_Temp.Settings.platColours.Count)
            {
                platColourIndexer = 0;
            }

            platCurrentColor = Phillip_Menu_Temp.Settings.platColours[platColourIndexer];
        }
    }
}