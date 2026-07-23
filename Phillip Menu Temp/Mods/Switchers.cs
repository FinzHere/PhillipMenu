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

        public static int tpGunColourIndexer = 0;
        public static Color tpGunCurrentColor = Phillip_Menu_Temp.Settings.tpGunColours[tpGunColourIndexer];

        public static void TPGunColourChanger()
        {
            tpGunColourIndexer++;
            if (tpGunColourIndexer >= Phillip_Menu_Temp.Settings.tpGunColours.Count)
            {
                tpGunColourIndexer = 0;
            }

            tpGunCurrentColor = Phillip_Menu_Temp.Settings.tpGunColours[tpGunColourIndexer];
        }
    }
}