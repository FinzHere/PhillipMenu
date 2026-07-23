using System;
using System.Collections.Generic;
using System.Text;

namespace Phillip_Menu_Temp.Mods
{
    internal class Rig
    {
        public static VRRig rig = GorillaTagger.Instance.offlineVRRig;
        public static bool isGhost = false;
        public static bool isInvis = false;

        public static void GhostMonke()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton && !isInvis)
            {
                rig.enabled = false;
                isGhost = true;
            } else
            {
                rig.enabled = true;
                isGhost = false;
            }
        }

        public static void GhostMonkeOff()
        {
            if (!ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                rig.enabled = true;
                isGhost = false;
            }
        }

        public static void Invis()
        {
            if (ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                rig.headBodyOffset.y = 100f;
                isInvis = true;
            }
            else
            {
                rig.headBodyOffset.y = 0f;
                isInvis = false;
            }
        }

        public static void InvisOff()
        { 
            if (!ControllerInputPoller.instance.rightControllerSecondaryButton)
            {
                if (rig.headBodyOffset.y != 0f)
                {
                    rig.headBodyOffset.y = 0f;
                    isInvis = false;
                }
            } 
        }

        public static void GrabRig()
        {
            if (ControllerInputPoller.instance.rightGrab)
            {

            }
        }
    }
}
