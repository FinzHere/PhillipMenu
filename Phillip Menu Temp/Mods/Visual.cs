using System;
using UnityEngine;
using GorillaLocomotion;

namespace Phillip_Menu_Temp.Mods
{
    internal class Visual
    {
        public static void BoxESP()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs)
            {
                // Safety checks to ensure the player actually exists and has a skin
                if (p != null && !p.isOfflineVRRig && p.mainSkin != null)
                {
                    // Check if we already gave this player a LineRenderer
                    LineRenderer box = p.gameObject.GetComponent<LineRenderer>();
                    if (box == null)
                    {
                        // Create it ONCE per player
                        box = p.gameObject.AddComponent<LineRenderer>();
                        box.startWidth = 0.025f; // Nice thin outline
                        box.endWidth = 0.025f;
                        box.positionCount = 5;   // 4 corners + loop back to start
                        box.material.shader = Shader.Find("GUI/Text Shader");
                        box.useWorldSpace = true;
                    }

                    box.enabled = true;
                    box.material.color = p.mainSkin.material.color; // Perfectly matches player color

                    // Calculate the corners to face the camera (Billboarding)
                    Transform cam = Camera.main.transform;
                    Vector3 pos = p.transform.position;

                    float height = 0.5f; // Adjust these to make the box fit the monkey perfectly
                    float width = 0.35f;

                    Vector3 up = cam.up * height;
                    Vector3 right = cam.right * width;

                    // Draw the 2D box outline
                    box.SetPosition(0, pos + up - right); // Top Left
                    box.SetPosition(1, pos + up + right); // Top Right
                    box.SetPosition(2, pos - up + right); // Bottom Right
                    box.SetPosition(3, pos - up - right); // Bottom Left
                    box.SetPosition(4, pos + up - right); // Connect back to Top Left
                }
            }
        }

        // Run this when the button is toggled off!
        public static void BoxESPOff()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs)
            {
                if (p != null)
                {
                    LineRenderer box = p.gameObject.GetComponent<LineRenderer>();
                    if (box != null)
                    {
                        box.enabled = false;
                    }
                }
            }
        }

        public static void Tracers()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs)
            {
                if (p != null && !p.isOfflineVRRig && p.mainSkin != null)
                {
                    // Look for our specific custom child object so it doesn't break BoxESP
                    Transform tracerTransform = p.transform.Find("MenuTracerLine");
                    LineRenderer lr;

                    if (tracerTransform == null)
                    {
                        // Create it ONCE as a child of the player if it doesn't exist
                        GameObject tracerObj = new GameObject("MenuTracerLine");
                        tracerObj.transform.SetParent(p.transform);

                        lr = tracerObj.AddComponent<LineRenderer>();
                        lr.startWidth = 0.01f;
                        lr.endWidth = 0.01f;
                        lr.positionCount = 2;
                        lr.useWorldSpace = true;

                        Renderer r = lr.GetComponent<Renderer>();
                        r.material.shader = Shader.Find("GUI/Text Shader");
                    }
                    else
                    {
                        lr = tracerTransform.GetComponent<LineRenderer>();
                    }

                    // Turn it on and update it
                    lr.enabled = true;

                    Color playerColor = p.mainSkin.material.color;
                    lr.startColor = playerColor;
                    lr.endColor = playerColor;

                    // Make sure your player instance exists before grabbing the hand position
                    if (GorillaLocomotion.GTPlayer.Instance != null)
                    {
                        lr.SetPosition(0, GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.position); // Start at your hand
                        lr.SetPosition(1, p.transform.position); // End at the target monkey
                    }
                }
            }
        }

        // Run this when the Tracers button is turned off!
        public static void TracersOff()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs)
            {
                if (p != null)
                {
                    Transform tracerTransform = p.transform.Find("MenuTracerLine");
                    if (tracerTransform != null)
                    {
                        LineRenderer lr = tracerTransform.GetComponent<LineRenderer>();
                        if (lr != null)
                        {
                            lr.enabled = false; // Hide the line cleanly
                        }
                    }
                }
            }
        }
    }
}