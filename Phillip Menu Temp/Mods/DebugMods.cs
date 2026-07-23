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
    }
}
