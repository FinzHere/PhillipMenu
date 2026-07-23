using GorillaLocomotion;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Phillip_Menu_Temp.Mods
{
    internal class Movement
    {
        public static void Speedboost()
        {
            if (GTPlayer.Instance != null)
            {
                float appliedMaxSpeed = 9f;
                float appliedJumpMultiplier = 1.35f;

                if (GorillaGameManager.instance != null)
                {
                    float[] trueSpeeds = GorillaGameManager.instance.LocalPlayerSpeed();

                    if (trueSpeeds != null && trueSpeeds.Length >= 2)
                    {
                        if (trueSpeeds[0] > 6.5f)
                        {
                            appliedMaxSpeed = trueSpeeds[0];
                            appliedJumpMultiplier = trueSpeeds[1];
                        }
                    }
                }
                
                GTPlayer.Instance.maxJumpSpeed = appliedMaxSpeed;
                GTPlayer.Instance.jumpMultiplier = appliedJumpMultiplier;
            }
        }

        public static void NormalSpeed()
        {
            if (GTPlayer.Instance != null)
            {
                // 1. Set our safe fallbacks (in case they are not in a lobby yet)
                float targetMaxSpeed = 6.5f;
                float targetMultiplier = 1.1f;

                // 2. Check if a game mode is currently active
                if (GorillaGameManager.instance != null)
                {
                    // LocalPlayerSpeed() returns a float array: [0] = maxJumpSpeed, [1] = jumpMultiplier
                    // It automatically calculates if you are a Lava Monkey, Rock Monkey, etc.
                    float[] trueSpeeds = GorillaGameManager.instance.LocalPlayerSpeed();

                    if (trueSpeeds != null && trueSpeeds.Length >= 2)
                    {
                        targetMaxSpeed = trueSpeeds[0];
                        targetMultiplier = trueSpeeds[1];
                    }
                }

                // 3. Apply the correct native speeds
                GTPlayer.Instance.maxJumpSpeed = targetMaxSpeed;
                GTPlayer.Instance.jumpMultiplier = targetMultiplier;
            }
        }

        public static void Fly()
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton)
            {
                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * 13f;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }

        public static GameObject rplat, lplat;
        public static void Platforms()
        {
            if (ControllerInputPoller.instance.rightGrab && rplat == null)
            {
                rplat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rplat.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                rplat.transform.rotation = GorillaTagger.Instance.rightHandTransform.rotation;
                rplat.transform.position = GorillaTagger.Instance.rightHandTransform.position;

                rplat.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                rplat.GetComponent<Renderer>().material.color = Switchers.platCurrentColor;
            }
            if (ControllerInputPoller.instance.leftGrab && lplat == null)
            {
                lplat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lplat.transform.localScale = new Vector3(0.025f, 0.3f, 0.4f);
                lplat.transform.rotation = GorillaTagger.Instance.leftHandTransform.rotation;
                lplat.transform.position = GorillaTagger.Instance.leftHandTransform.position;

                lplat.GetComponent<Renderer>().material.shader = Shader.Find("GorillaTag/UberShader");
                lplat.GetComponent<Renderer>().material.color = Switchers.platCurrentColor;
            }
            if (!ControllerInputPoller.instance.rightGrab && rplat != null)
            {
                GameObject.Destroy(rplat);
                rplat = null;
            }
            if (!ControllerInputPoller.instance.leftGrab && lplat != null)
            {
                GameObject.Destroy(lplat);
                lplat = null;
            }
        }

        public static void PlatOff()
        {
            if (rplat != null && !ControllerInputPoller.instance.rightGrab)
            {
                GameObject.Destroy(rplat);
                rplat = null;
            }
            if (lplat != null && !ControllerInputPoller.instance.leftGrab)
            {
                GameObject.Destroy(lplat);
                lplat = null;
            }
        }

        // State trackers for the toggles (Menu opening/closing)
        private static bool wasNoclipOn = false;
        private static bool wasNoclipFlyOn = false;

        // NEW: State trackers for the actual button inputs
        private static bool isNoclipping = false;
        private static bool isNoclipFlying = false;

        public static void Noclip()
        {
            wasNoclipOn = true;
            bool isPressing = ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f;

            if (isPressing && !isNoclipping)
            {
                // Just started pressing: disable colliders ONCE
                isNoclipping = true;
                foreach (Collider c in Resources.FindObjectsOfTypeAll<MeshCollider>()) c.enabled = false;
            }
            else if (!isPressing && isNoclipping)
            {
                // Just let go: re-enable colliders ONCE
                isNoclipping = false;
                foreach (Collider c in Resources.FindObjectsOfTypeAll<MeshCollider>()) c.enabled = true;
            }
        }

        public static void NoclipOff()
        {
            if (wasNoclipOn)
            {
                foreach (Collider c in Resources.FindObjectsOfTypeAll<MeshCollider>()) c.enabled = true;
                wasNoclipOn = false;
                isNoclipping = false; // Reset the input state too
            }
        }

        public static void NoclipFly()
        {
            wasNoclipFlyOn = true;
            bool isPressing = ControllerInputPoller.instance.rightControllerPrimaryButton;

            if (isPressing)
            {
                // 1. Movement happens every frame
                GTPlayer.Instance.transform.position += GTPlayer.Instance.headCollider.transform.forward * Time.deltaTime * 13f;
                GTPlayer.Instance.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

                // 2. Colliders are disabled ONLY on the first frame of the press
                if (!isNoclipFlying)
                {
                    isNoclipFlying = true;
                    foreach (Collider c in Resources.FindObjectsOfTypeAll<MeshCollider>()) c.enabled = false;
                }
            }
            else if (!isPressing && isNoclipFlying)
            {
                // Re-enable ONCE when you let go
                isNoclipFlying = false;
                foreach (Collider c in Resources.FindObjectsOfTypeAll<MeshCollider>()) c.enabled = true;
            }
        }

        public static void NoclipFlyOff()
        {
            if (wasNoclipFlyOn)
            {
                foreach (Collider c in Resources.FindObjectsOfTypeAll<MeshCollider>()) c.enabled = true;
                wasNoclipFlyOn = false;
                isNoclipFlying = false; // Reset the input state too
            }
        }

        public static bool longArmsOn = false;

        public static void LongArms()
        {
            if (GTPlayer.Instance != null && longArmsOn == false)
            {
                GTPlayer.Instance.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                longArmsOn = true;
            }
        }

        public static void LongArmsOff()
        {
            if (GTPlayer.Instance != null && longArmsOn == true)
            {
                GTPlayer.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
                longArmsOn = false;
            }
        }

        public static int globalGravInt = 0;

        public static void GlobalGravity()
        {
            globalGravInt++;
            if (globalGravInt > 3)
            {
                globalGravInt = 0;
            }

            switch (globalGravInt) {

                case 0:
                    Physics.gravity = new Vector3(0f, -9.81f, 0f);
                    
                    break;

                case 1:
                    Physics.gravity = new Vector3(0f, -4.5f, 0f);
                    
                    break;

                case 2:
                    Physics.gravity = new Vector3(0f, -14.3f, 0f);
                    
                    break;

                case 3:
                    Physics.gravity = new Vector3(0f, 0f, 0f);
                    
                    break;
            }
        }

        public static int playerGravInt = 0;
        public static float gravAmount = 0f;

        public static void PlayerGravity()
        {
            if (GTPlayer.Instance == null) return;

            playerGravInt++;
            if (playerGravInt > 3)
            {
                playerGravInt = 0;
            }

            switch (playerGravInt)
            {
                case 0:
                    gravAmount = 0f;
                    break;

                case 1:
                    gravAmount = 6.5f;
                    break;

                case 2:
                    gravAmount = -4.5f;
                    break;

                case 3:
                    gravAmount = 9.78f;
                    break;
            }
        }
    }
}