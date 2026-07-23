using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Photon.Pun;
using GorillaLocomotion;

namespace Phillip_Menu_Temp.Mods
{
    internal class OP
    {
        public static bool tpGunOn = false;
        public static bool spiderMonkeyOn = false;

        public static void TPGun()
        {
            if (!tpGunOn)
            {
                tpGunOn = true;
            }
        }

        public static void TPGunOff()
        {
            if (tpGunOn)
            {
                tpGunOn = false;
            }
        }

        public static void SpiderMonkey()
        {
            if (GTPlayer.Instance != null)
            {
                Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
                if (rb == null) return;

                Vector3 leftHandPos = GTPlayer.Instance.LeftHand.controllerTransform.position;
                Vector3 rightHandPos = GTPlayer.Instance.RightHand.controllerTransform.position;

                float stickRadius = 0.25f;

                bool leftNearWall = Physics.CheckSphere(leftHandPos, stickRadius, ~LayerMask.GetMask("GorillaParticipant"));
                bool rightNearWall = Physics.CheckSphere(rightHandPos, stickRadius, ~LayerMask.GetMask("GorillaParticipant"));

                bool holdingLeftGrip = ControllerInputPoller.instance.leftControllerGripFloat > 0.1f;
                bool holdingRightGrip = ControllerInputPoller.instance.rightControllerGripFloat > 0.1f;

                if ((leftNearWall && holdingLeftGrip) || (rightNearWall && holdingRightGrip))
                {
                    rb.useGravity = false;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    spiderMonkeyOn = true;
                }
                else
                {
                    rb.useGravity = true;
                    spiderMonkeyOn = false;
                }
            }
        }

        public static void SpiderMonkeyOff()
        {
            if (GTPlayer.Instance != null)
            {
                if ((ControllerInputPoller.instance.leftControllerGripFloat !> 0.1f || ControllerInputPoller.instance.rightControllerGripFloat! > 0.1f) && spiderMonkeyOn)
                {
                    Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
                    rb.useGravity = true;
                    spiderMonkeyOn = false;
                }
            }
        }
    }
}
