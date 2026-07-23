using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Phillip_Menu_Temp.Mods
{
    internal class StumpBoards
    {
        private static TextMeshPro motdBody;
        private static TextMeshPro motdHeading;
        private static TextMeshPro cocHeading;
        private static TextMeshPro mapInfo;
        private static TextMeshPro cocBody;
        private static Renderer monitorScreen;
        private static Renderer wallMonitor;

        // FIX 1: Change these to strings to actually save the text!
        private static string OGmotdBodyText;
        private static string OGmotdHeadingText;
        private static string OGcocHeadingText;
        private static string OGmapInfoText;
        private static string OGcocBodyText;

        private static float _nextUpdateTime = 0f;
        private static bool _boardsCacheInit = false;

        private static void InitBoardsCache()
        {
            if (_boardsCacheInit) return;

            try { motdBody = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdBodyText")?.GetComponent<TextMeshPro>(); } catch { }
            try { motdHeading = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/motdHeadingText")?.GetComponent<TextMeshPro>(); } catch { }
            try { cocHeading = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/CodeOfConductHeadingText")?.GetComponent<TextMeshPro>(); } catch { }
            try { mapInfo = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/MapInfo_TMP")?.GetComponent<TextMeshPro>(); } catch { }
            try { cocBody = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/COCBodyText_TitleData")?.GetComponent<TextMeshPro>(); } catch { }
            try { monitorScreen = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/ComputerUI/monitor/monitorScreen")?.GetComponent<Renderer>(); } catch { }
            try { wallMonitor = GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomBoundaryStones/BoundaryStoneSet_Forest/wallmonitorforestbg")?.GetComponent<Renderer>(); } catch { }

            if (motdBody == null || cocBody == null)
            {
                InvalidateBoardsCache();
                return;
            }

            // FIX 2: Save the actual text string, not the component!
            if (motdBody != null) OGmotdBodyText = motdBody.text;
            if (motdHeading != null) OGmotdHeadingText = motdHeading.text;
            if (cocHeading != null) OGcocHeadingText = cocHeading.text;
            if (mapInfo != null) OGmapInfoText = mapInfo.text;
            if (cocBody != null) OGcocBodyText = cocBody.text;

            _boardsCacheInit = true;

            Debug.Log($"[PhillipMenu] Successfully found the boards!");
        }

        public static void InvalidateBoardsCache()
        {
            _boardsCacheInit = false;
            motdBody = null;
            motdHeading = null;
            cocHeading = null;
            mapInfo = null;
            cocBody = null;
            monitorScreen = null;
            wallMonitor = null;
            Debug.Log($"[PhillipMenu] Cleared the cache and reset values");
        }

        public static void Boards()
        {
            // FIX 3: Automatically initialize if it hasn't been done yet
            if (!_boardsCacheInit) InitBoardsCache();

            // Use Time.time for a more reliable 1-second delay in static menu methods
            if (Time.time < _nextUpdateTime) return;
            _nextUpdateTime = Time.time + 1.0f;

            if (motdHeading != null)
            {
                motdHeading.text = "PHILLLIP MENU";
                motdHeading.fontStyle = FontStyles.Bold;
            }
            try
            {
                if (motdBody != null)
                {
                    motdBody.richText = true;
                    string playerName = PhotonNetwork.LocalPlayer.NickName;
                    string room = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "Not In Room";
                    int players = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
                    motdBody.text = "=========================================================================\nName: " + playerName + "\nRoom: " + room + "\nPlayers: " + players + "\nStatus: <#00FF00>Undetected</color>\n=========================================================================";
                }
            }
            catch { }

            if (cocHeading != null)
            {
                cocHeading.text = "ABOUT PHILLIP MENU";
                cocHeading.fontStyle = FontStyles.Bold;
            }
            try
            {
                if (cocBody != null)
                {
                    cocBody.richText = true;
                    cocBody.fontStyle = FontStyles.Bold;
                    cocBody.text = "==============================================\n\nHello! Welcome to <#5714C4>Phillip Menu</color> !\n<#5714C4>Phillip Menu</color> is a <#00FF00>FREE</color> and <#00FF00>SIMPLE</color> Gorilla Tag Mod Menu .\nThis is a beginner project, so don't expect <#FF0000>big, fancy mods</color>, but rather <#00FF00>mastery</color> of the <#5714C4>very basics</color> .\n<#5714C4>With that being said, please enjoy using Phillip Menu !</color>\n\n==============================================";
                }
            }
            catch { }
        }

        public static void BoardsOff()
        {
            if (!_boardsCacheInit) return;

            // FIX 4: Put the original text back!
            if (motdHeading != null)
            {
                motdHeading.text = OGmotdHeadingText;
                motdHeading.fontStyle = FontStyles.Normal;
            }
            if (motdBody != null)
            {
                motdBody.richText = false;
                motdBody.fontStyle = FontStyles.Normal;
                motdBody.text = OGmotdBodyText;
            }
            if (cocHeading != null)
            {
                cocHeading.text = OGcocHeadingText;
                cocHeading.fontStyle = FontStyles.Normal;
            }
            if (cocBody != null)
            {
                cocBody.richText = false;
                cocBody.fontStyle= FontStyles.Normal;
                cocBody.text = OGcocBodyText;
            }
        }
    }
}