using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using HarmonyLib;
using Oculus.Platform;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.Events;
using PlayFab.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms;
using Debug = UnityEngine.Debug;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Object = UnityEngine.Object;

namespace Phillip_Menu_Temp.Mods
{
    internal class Safety
    {
        public static void BypassVCBan()
        {
            GorillaTagger.moderationMutedTime = -1f;
            GorillaTelemetry.PostNotificationEvent("Unmute");
            GorillaTagger.Instance.myRecorder.TransmitEnabled = true;
            if (KIDManager.Instance != null)
            {
                GameObject.Destroy(KIDManager.Instance);
            }
        }

        public static void SetTick(float tickMultiplier)
        {
            var photonMono = GameObject.Find("PhotonMono")?.GetComponent<PhotonHandler>();
            if (photonMono != null)
            {
                Traverse.Create(photonMono).Field("nextSendTickCountOnSerialize").SetValue((int)(Time.realtimeSinceStartup * tickMultiplier));
                PhotonHandler.SendAsap = true;
            }
        }

        public static void FlushNetwork()
        {
            try
            {
                PhotonNetwork.SendAllOutgoingCommands();
                PhotonNetwork.NetworkingClient.LoadBalancingPeer.SendOutgoingCommands();
            }
            catch { }
        }

        public static object RunViewUpdate()
        {
            return typeof(PhotonNetwork).GetMethod("RunViewUpdate", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
        }
        private static DateTime lastAntiBanCall = DateTime.MinValue;
        private static readonly TimeSpan antiBanInterval = TimeSpan.FromSeconds(5);
        private static bool initialized;
        private static FieldInfo authContextField;
        private static FieldInfo photonViewListField;
        private static FieldInfo userRPCCallsField;
        private static FieldInfo reportedPlayersField;
        private static FieldInfo sendReportField;
        private static FieldInfo suspiciousPlayerIdField;
        private static FieldInfo suspiciousReasonField;
        private static FieldInfo suspiciousPlayerNameField;
        private static FieldInfo cachedDataField;
        private static FieldInfo monoRPCMethodsCacheField;
        private static MethodInfo clearAllEventsMethod;
        private static FieldInfo staticPlayerField;
        private static FieldInfo requestTimeoutField;
        private static FieldInfo compressApiDataField;
        private static FieldInfo disableFocusTimeCollectionField;
        private static FieldInfo sentCountAllowanceField;
        private static FieldInfo quickResendAttemptsField;
        private static FieldInfo outgoingStreamQueueField;
        public static void InitializeAntiBanHelper()
        {
            if (initialized) return;
            try
            {
                var playFabHttpType = typeof(PlayFabHttp);
                clearAllEventsMethod = playFabHttpType.GetMethod("ClearAllEvents", BindingFlags.Public | BindingFlags.Static);
                authContextField = typeof(PlayFabAuthenticationAPI).GetField("_authenticationContext",
                    BindingFlags.Static | BindingFlags.NonPublic);
                staticPlayerField = typeof(PlayFabSettings).GetField("staticPlayer",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                requestTimeoutField = typeof(PlayFabSettings).GetField("RequestTimeout",
                    BindingFlags.Static | BindingFlags.Public);
                compressApiDataField = typeof(PlayFabSettings).GetField("CompressApiData",
                    BindingFlags.Static | BindingFlags.Public);
                disableFocusTimeCollectionField = typeof(PlayFabSettings).GetField("DisableFocusTimeCollection",
                    BindingFlags.Static | BindingFlags.Public);
                var monkeAgentType = typeof(MonkeAgent);
                userRPCCallsField = monkeAgentType.GetField("userRPCCalls",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                reportedPlayersField = monkeAgentType.GetField("reportedPlayers",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                sendReportField = monkeAgentType.GetField("_sendReport",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousPlayerIdField = monkeAgentType.GetField("_suspiciousPlayerId",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousPlayerNameField = monkeAgentType.GetField("_suspiciousPlayerName",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                suspiciousReasonField = monkeAgentType.GetField("_suspiciousReason",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                var photonNetworkType = typeof(Photon.Pun.PhotonNetwork);
                photonViewListField = photonNetworkType.GetField("photonViewList",
                    BindingFlags.Static | BindingFlags.NonPublic);
                cachedDataField = photonNetworkType.GetField("cachedData",
                    BindingFlags.Static | BindingFlags.NonPublic);
                monoRPCMethodsCacheField = photonNetworkType.GetField("monoRPCMethodsCache",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var peerType = typeof(LoadBalancingPeer);
                sentCountAllowanceField = peerType.GetField("SentCountAllowance",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                quickResendAttemptsField = peerType.GetField("QuickResendAttempts",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                outgoingStreamQueueField = peerType.GetField("outgoingStreamQueue",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                initialized = true;
            }
            catch { }
        }

        public static void AntiBan()
        {
            InitializeAntiBanHelper();
            if (!PhotonNetwork.InRoom) return;
            try
            {
                var instance = MonkeAgent.instance;
                if (instance != null)
                {
                    instance.rpcErrorMax = int.MaxValue;
                    instance.rpcCallLimit = int.MaxValue;
                    instance.logErrorMax = int.MaxValue;
                    userRPCCallsField?.SetValue(instance, new Dictionary<string, Dictionary<string, object>>());
                    reportedPlayersField?.SetValue(instance, new List<string>());
                    sendReportField?.SetValue(instance, false);
                    suspiciousPlayerIdField?.SetValue(instance, "");
                    suspiciousPlayerNameField?.SetValue(instance, "");
                    suspiciousReasonField?.SetValue(instance, "");
                }
                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;
                PhotonNetwork.NetworkStatisticsEnabled = false;
                var peer = PhotonNetwork.NetworkingClient?.LoadBalancingPeer;
                if (peer != null)
                {
                    sentCountAllowanceField?.SetValue(peer, int.MaxValue);
                    quickResendAttemptsField?.SetValue(peer, (byte)3);
                    var queue = outgoingStreamQueueField?.GetValue(peer) as System.Collections.IList;
                    queue?.Clear();
                    var resentField = peer.GetType().GetField("resentCommandsCount", BindingFlags.NonPublic | BindingFlags.Instance);
                    resentField?.SetValue(peer, 0);
                    peer.SendOutgoingCommands();
                }
                PhotonNetwork.SendAllOutgoingCommands();
                photonViewListField?.SetValue(null, Activator.CreateInstance(photonViewListField.FieldType));
                cachedDataField?.SetValue(null, new Dictionary<int, Dictionary<int, Queue<object[]>>>());
                monoRPCMethodsCacheField?.SetValue(null, new Dictionary<Type, List<MethodInfo>>());
                if (DateTime.UtcNow - lastAntiBanCall < antiBanInterval) return;
                lastAntiBanCall = DateTime.UtcNow;
                if (!PhotonNetwork.IsConnected)
                {
                    authContextField?.SetValue(null, null);
                    clearAllEventsMethod?.Invoke(null, null);
                    return;
                }
                if (!PlayFabAuthenticationAPI.IsEntityLoggedIn()) return;
                try
                {
                    clearAllEventsMethod?.Invoke(null, null);
                    requestTimeoutField?.SetValue(null, 30000);
                    compressApiDataField?.SetValue(null, true);
                    disableFocusTimeCollectionField?.SetValue(null, true);
                    var staticPlayer = staticPlayerField?.GetValue(null) as PlayFabAuthenticationContext;
                    staticPlayer?.ForgetAllCredentials();
                }
                catch { }
            }
            catch { }
        }
        public static void NoFinger()
        {
            ControllerInputPoller.instance.leftControllerGripFloat = 0f;
            ControllerInputPoller.instance.rightControllerGripFloat = 0f;
            ControllerInputPoller.instance.leftControllerIndexFloat = 0f;
            ControllerInputPoller.instance.rightControllerIndexFloat = 0f;
        }

        public static void RestartGame()
        {
            Process.Start("steam://rungameid/1533390");
            UnityEngine.Application.Quit();
        }

        public static void QuitGame()
        {
            UnityEngine.Application.Quit();
        }

        public static void DisconnectLT()
        {
            if (PhotonNetwork.InRoom)
            {
                if (ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f || Mouse.current.leftButton.isPressed)
                {
                    PhotonNetwork.Disconnect();
                }
            }
        }

        public static void DisconnectRT()
        {
            if (PhotonNetwork.InRoom)
            {
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f || Mouse.current.rightButton.isPressed)
                {
                    PhotonNetwork.Disconnect();
                }
            }
        }

        public static void DisableNetworkTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(false);
        }

        public static void EnableNetworkTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab").SetActive(true);
        }

        public static void DisableMapTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab").SetActive(false);
        }

        public static void EnableMapTriggers()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab").SetActive(true);
        }

        public static void DisableQuitBox()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(false);
        }

        public static void EnableQuitBox()
        {
            GameObject.Find("Environment Objects/TriggerZones_Prefab/ZoneTransitions_Prefab/QuitBox").SetActive(true);
        }

        public static void EnableAntiAFK()
        {
            PhotonNetworkController.Instance.disableAFKKick = true;
        }

        public static void DisableAntiAFK()
        {
            PhotonNetworkController.Instance.disableAFKKick = false;
        }

        public static void JoinRandom()
        {
            // Start the coroutine to handle the timing safely
            GorillaTagger.Instance.StartCoroutine(JoinRandomRoutine());
        }

        private static IEnumerator JoinRandomRoutine()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();

                while (PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady)
                {
                    yield return null;
                }
            }

            // 1. Try to get current zone or default to "Forest" (capitalized)
            string zone = PhotonNetworkController.Instance.currentJoinTrigger == null
                ? "Forest"
                : PhotonNetworkController.Instance.currentJoinTrigger.networkZone;

            GorillaNetworkJoinTrigger joinTrigger = GorillaComputer.instance.GetJoinTriggerForZone(zone);

            // 2. Fallback: If trigger is null, find any active join trigger in the scene
            if (joinTrigger == null)
            {
                joinTrigger = UnityEngine.Object.FindObjectOfType<GorillaNetworkJoinTrigger>();
            }

            // 3. Attempt join
            if (joinTrigger != null)
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(joinTrigger, 0);
            }
            else
            {
                Debug.LogError("JoinRandom: No valid GorillaNetworkJoinTrigger found in scene!");
            }
        }

        public static void JoinRoom(string roomCode)
        {
            GorillaTagger.Instance.StartCoroutine(JoinRoomRoutine(roomCode));
        }

        private static IEnumerator JoinRoomRoutine(string roomCode)
        {
            if (PhotonNetwork.InRoom)
            {
                // 1. Leave the current room
                PhotonNetwork.LeaveRoom();

                // 2. Wait until Photon safely returns us to the Master Server
                while (PhotonNetwork.InRoom || !PhotonNetwork.IsConnectedAndReady)
                {
                    yield return null;
                }
            }

            // 3. Connect to the specific room!
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomCode, 0);
        }

        public static VRRig reportRig;
        public static float antiReportRadius = 0.45f;
        public static GameObject antiReportSphere;

        public static void AntiReport(System.Action<VRRig, Vector3> onReport)
        {
            if (!NetworkSystem.Instance.InRoom) return;

            if (reportRig != null)
            {
                onReport?.Invoke(reportRig, reportRig.transform.position);
                reportRig = null;
                return;
            }

            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;
                Transform report = line.reportButton.gameObject.transform;

                foreach (var vrrig in from vrrig in VRRigCache.ActiveRigs where !vrrig.isLocal let D1 = Vector3.Distance(vrrig.rightHandTransform.position, report.position) let D2 = Vector3.Distance(vrrig.leftHandTransform.position, report.position) where D1 < antiReportRadius || D2 < antiReportRadius select vrrig)
                    onReport?.Invoke(vrrig, report.transform.position);
            }
        }
        public static void VisualizeAntiReportRadius()
        {
            if (!NetworkSystem.Instance.InRoom)
            {
                CleanupAntiReportVisualization();
                return;
            }

            bool foundButton = false;
            foreach (GorillaPlayerScoreboardLine line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line == null || line.linePlayer != NetworkSystem.Instance.LocalPlayer) continue;

                if (line.reportButton == null || line.reportButton.gameObject == null)
                {
                    continue;
                }

                Transform report = line.reportButton.gameObject.transform;
                foundButton = true;

                if (antiReportSphere == null)
                {
                    antiReportSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Object.Destroy(antiReportSphere.GetComponent<Collider>());

                    Renderer renderer = antiReportSphere.GetComponent<Renderer>();
                    
                    renderer.material.shader = Shader.Find("GorillaTag/UberShader");

                    // Use menu theme color
                    Color sphereColor = Color.green;
                    sphereColor.a = 0.4f;
                    renderer.material.color = sphereColor;

                    antiReportSphere.layer = 0; // Default layer
                }
                else
                {
                    // Update color with pulsing gradient using menu theme color
                    Renderer renderer = antiReportSphere.GetComponent<Renderer>();
                    if (renderer != null && renderer.material != null)
                    {
                        Color themeColor = Color.green;
                        Color lightThemeColor = Color.Lerp(themeColor, Color.white, 0.5f);
                        Color sphereColor = Color.Lerp(themeColor, lightThemeColor, Mathf.PingPong(Time.time * 0.8f, 1f));
                        sphereColor.a = 0.4f;
                        renderer.material.color = sphereColor;
                    }
                }

                antiReportSphere.transform.position = report.position;
                antiReportSphere.transform.localScale = Vector3.one * (antiReportRadius * 2f);
                antiReportSphere.SetActive(true);
                break; // Only need to update once per frame
            }
        }

        public static void CleanupAntiReportVisualization()
        {
            if (antiReportSphere != null)
            {
                GameObject.Destroy(antiReportSphere);
                antiReportSphere = null;
            }
        }

        public static string GetAntiReportRadiusName()
        {
            return antiReportRadius.ToString("F2");
        }

        public static float antiReportDelay;
        public static void AntiReportDisconnect()
        {
            AntiReport((vrrig, position) =>
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
            });
        }
        public static void AntiReportQuit()
        {
            AntiReport((vrrig, position) =>
            {
                UnityEngine.Application.Quit();
            });
        }
        public static void AntiReportReconnect()
        {
            AntiReport((vrrig, position) =>
            {
                string name = PhotonNetwork.CurrentRoom.Name;
                NetworkSystem.Instance.ReturnToSinglePlayer();
                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(name, GorillaNetworking.JoinType.Solo);
            });
        }
        public static void AntiAFKKick()
        {
            PhotonNetworkController.Instance.disableAFKKick = true;
        }

        static float rpcDel;
        public static bool IsRPCPatched = false;
        public static bool visAntiReport = false;
        public static void UncapFPS()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = int.MaxValue;
        }
        public static void SetFPS144()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 144;
        }
        public static void SetFPS120()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 120;
        }

        public static void SetFPS90()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 90;
        }

        public static void SetFPS80()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 80;
        }

        public static void SetFPS72()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 72;
        }

        public static void SetFPS60()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 60;
        }

        public static void SetFPS45()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 45;
        }

        public static void SetFPS15()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 15;
        }

        public static void SetFPS1()
        {
            QualitySettings.vSyncCount = 0;
            UnityEngine.Application.targetFrameRate = 1;
        }

        public static void AcceptTOS()
        {
            GameObject.Find("Miscellaneous Scripts/PopUpMessage").SetActive(false);
        }

        private static float lastRpcClear = 0f;
        private static float rpcClearInterval = 5f;

        public static void AntiCrash()
        {
            try
            {
                if (Time.time > lastRpcClear + rpcClearInterval)
                {
                    lastRpcClear = Time.time;
                    if (PhotonNetwork.NetworkingClient != null)
                    {
                        var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
                        var outgoingQueueField = peer.GetType().GetField("outgoingStreamQueue",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        var queue = outgoingQueueField?.GetValue(peer) as System.Collections.IList;
                        if (queue != null && queue.Count > 1000)
                        {
                            queue.Clear();
                            Debug.Log("Cleared outgoing RPC queue to prevent crash");
                        }
                    }
                }
                if (GorillaTagger.Instance == null || GorillaTagger.Instance.myVRRig == null)
                    return;
            }
            catch { }
        }
    }
}
