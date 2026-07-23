using GorillaLocomotion;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Phillip_Menu_Temp.Mods
{
    internal class DebugMods
    {
        public static float maxJumpSpeed;
        public static float jumpMulti;

        public static void SpeedValueChecker()
        {
            if (GTPlayer.Instance != null)
            {
                maxJumpSpeed = GTPlayer.Instance.maxJumpSpeed;
                jumpMulti = GTPlayer.Instance.jumpMultiplier;
                Debug.Log($"[PhillipMenu] Successfully grabbed default values.");
                Debug.Log($"[PhillipMenu] Default maxJumpSpeed {maxJumpSpeed}");
                Debug.Log($"[PhillipMenu] Default jumpMultiplier {jumpMulti}");
            }
        }

        public static void ConnectedLobbyCode()
        {
            string finalText;
            string gamemode = "None";
            int playerCount = 0;

            if (GorillaGameManager.instance != null)
            {
                gamemode = GorillaGameManager.instance.GetType().Name;
            }

            if (PhotonNetwork.CurrentRoom == null)
            {
                finalText = $"DISCONNECTED - {gamemode}";
            }
            else
            {
                string roomName = PhotonNetwork.CurrentRoom.Name;
                playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
                finalText = $"CONNECTED: {roomName} - {gamemode}";
            }

            Debug.Log($"[PhillipMenu] Current Room Status: {finalText}");
            if (playerCount != 0)
            {
                Debug.Log($"[PhillipMenu] Amount of Players: {playerCount}");
            }
        }
        
        public static void ActiveColliders()
        {
            MeshCollider[] totalMeshColliders = UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None);
            BoxCollider[] totalBoxColliders = UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None);
            int allCols = totalMeshColliders.Length + totalBoxColliders.Length;

            Debug.Log($"[PhillipMenu] Current Active Mesh Colliders: {totalMeshColliders.Length}");
            Debug.Log($"[PhillipMenu] Current Active Box Colliders: {totalBoxColliders.Length}");
            Debug.Log($"[PhillipMenu] Total Active Colliders: {allCols}");
        }

        public static void CurrentGrav()
        {
            Vector3 currentGrav;

            currentGrav = Physics.gravity;

            Debug.Log($"[PhillipMenu] Current Gravity: {currentGrav.y}");
        }

        public static GameObject xAxis, yAxis, zAxis;
        public static Vector3 axisScale = new Vector3(0.1f, 0.1f, 0.1f);
        
        public static void ShowHandAxis()
        {
            if (GTPlayer.Instance == null) return;

            Transform handTarget = GTPlayer.Instance.RightHand.controllerTransform;

            // X Axis
            if (xAxis == null)
            {
                xAxis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                xAxis.transform.SetParent(handTarget, false);
                xAxis.transform.localRotation = Quaternion.identity;
                xAxis.transform.localScale = axisScale;
                xAxis.transform.localPosition = new Vector3(0.15f, 0f, 0f);

                UnityEngine.Object.Destroy(xAxis.GetComponent<Collider>());

                var xRend = xAxis.GetComponent<Renderer>();
                xRend.material.shader = Shader.Find("GorillaTag/UberShader");
                xRend.material.color = Color.red;
            }

            if (yAxis == null)
            {
                yAxis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                yAxis.transform.SetParent(handTarget, false);
                yAxis.transform.localRotation = Quaternion.identity;
                yAxis.transform.localScale = axisScale;
                yAxis.transform.localPosition = new Vector3(0f, 0.15f, 0f);

                UnityEngine.Object.Destroy(yAxis.GetComponent<Collider>());

                var yRend = yAxis.GetComponent<Renderer>();
                yRend.material.shader = Shader.Find("GorillaTag/UberShader");
                yRend.material.color = Color.green;
            }

            if (zAxis == null)
            {
                zAxis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                zAxis.transform.SetParent(handTarget, false);
                zAxis.transform.localRotation = Quaternion.identity;
                zAxis.transform.localScale = axisScale;
                zAxis.transform.localPosition = new Vector3(0f, 0f, 0.15f);

                UnityEngine.Object.Destroy(zAxis.GetComponent<Collider>());

                var zRend = zAxis.GetComponent<Renderer>();
                zRend.material.shader = Shader.Find("GorillaTag/UberShader");
                zRend.material.color = Color.blue;
            }
        }

        public static void ShowHandAxisOff()
        {
            if (xAxis != null)
            {
                GameObject.Destroy(xAxis);
                xAxis = null;
            }

            if (yAxis != null)
            {
                GameObject.Destroy(yAxis);
                yAxis = null;
            }

            if (zAxis != null)
            {
                GameObject.Destroy(zAxis);
                zAxis = null;
            }
        }
    }
}
