using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using System.Collections.Generic;
using Phillip_Menu_Temp.Mods;
using Phillip_Menu_Temp.Libs;
using System.ComponentModel;
using GorillaLocomotion;

namespace Phillip_Menu_Temp
{
    // This holds the state even when the physical menu is destroyed
    public class ButtonData
    {
        public string buttonText;
        public Color32 textColour;
        public Color32? buttonColour;
        public Action method;
        public Action offMethod;
        public bool isToggleable;
        public bool isToggled;
        public string audioFilePath = "";
    }

    // FIX 1: Make ConstButtonData inherit from ButtonData
    // Now it automatically has buttonText, textColour, etc., PLUS your new Vectors!
    public class ConstButtonData : ButtonData
    {
        public Vector3 localPos;
        public Vector3 localScale;
    }

    public class MenuPage
    {
        public string pageName;
        public List<ButtonData> buttons = new List<ButtonData>();
    }

    [Description(Phillip_Menu_Temp.PluginInfo.description)]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.name, PluginInfo.version)]
    public class Main : BaseUnityPlugin
    {
        public static Main instance;

        List<MenuPage> allPages = new List<MenuPage>();

        string currentPageName = "Main [1]";

        private TextMeshPro titleText;

        public const int menuLayer = 0;

        public static SphereCollider menuHandCollider;
        GameObject pointerObj;

        bool isMenuCreated;
        GameObject menuObj;
        List<GameObject> btnObjs = new List<GameObject>();
        // You actually don't strictly need a separate GameObject list for const buttons 
        // if DestroyAllButtons clears everything, but it's fine to have!
        List<GameObject> constBtnObjs = new List<GameObject>();

        // This list holds all of your button data and states
        List<ButtonData> modButtons = new List<ButtonData>();
        List<ConstButtonData> constModButtons = new List<ConstButtonData>();

        public void GoToPage(string targetPageName)
        {
            MenuPage targetPage = allPages.Find(x => x.pageName == targetPageName);
            if (targetPage == null) return;

            // 1. Save state
            currentPageName = targetPageName;

            if (titleText != null)
            {
                titleText.text = targetPage.pageName;
            }

            // 2. Destroy only dynamic buttons
            foreach (GameObject btn in btnObjs) Destroy(btn);
            btnObjs.Clear();

            // 3. Spawn new buttons
            float currentZ = 0.11f;
            foreach (ButtonData btnData in targetPage.buttons)
            {
                AddButton(currentZ, btnData);
                currentZ -= 0.05f;
            }
        }

        public void NextPage()
        {
            // 1. Find our current index in the list
            int currentIndex = allPages.FindIndex(x => x.pageName == currentPageName);
            if (currentIndex == -1) return; // Safety check

            // 2. Add 1 to go to the next page
            int nextIndex = currentIndex + 1;

            // 3. If we go past the end of the list, loop back to page 0
            if (nextIndex >= allPages.Count)
            {
                nextIndex = 0;
            }

            // 4. Tell GoToPage to load the name of the next page
            GoToPage(allPages[nextIndex].pageName);
        }

        public void PrevPage()
        {
            // 1. Find our current index in the list
            int currentIndex = allPages.FindIndex(x => x.pageName == currentPageName);
            if (currentIndex == -1) return;

            // 2. Subtract 1 to go backward
            int prevIndex = currentIndex - 1;

            // 3. If we go past the beginning (less than 0), loop to the very last page
            if (prevIndex < 0)
            {
                prevIndex = allPages.Count - 1;
            }

            // 4. Load the previous page
            GoToPage(allPages[prevIndex].pageName);
        }

        void Awake()
        {
            instance = this;
            Harmony harmony = new Harmony(PluginInfo.GUID);
            harmony.PatchAll();
            Physics.IgnoreLayerCollision(menuLayer, menuLayer, false);
            Debug.Log("[PhillipMenu] Initialized menu on layer " + menuLayer + ". Ignores self-collision = " + Physics.GetIgnoreLayerCollision(menuLayer, menuLayer) + ".");
            SoundManager.Initialize();
        }

        void Start()
        {
            isMenuCreated = false;

            // --- Main ---
            MenuPage main = new MenuPage { pageName = "Main [1]" };
            main.buttons.Add(new ButtonData { buttonText = "Settings", method = () => GoToPage("Settings [1]") });
            main.buttons.Add(new ButtonData { buttonText = "Safety", method = () => GoToPage("Safety [1]") });
            main.buttons.Add(new ButtonData { buttonText = "Movement", method = () => GoToPage("Movement [1]") });
            main.buttons.Add(new ButtonData { buttonText = "Rig", method = () => GoToPage("Rig [1]") });
            main.buttons.Add(new ButtonData { buttonText = "Visual", method = () => GoToPage("Visual [1]") });
            main.buttons.Add(new ButtonData { buttonText = "OP", method = () => GoToPage("OP [1]") });
            allPages.Add(main);

            MenuPage main2 = new MenuPage { pageName = "Main [2]" };
            main2.buttons.Add(new ButtonData { buttonText = "Network", method = () => GoToPage("Network [1]") });
            main2.buttons.Add(new ButtonData { buttonText = "Soundboard", method = () => GoToPage("Soundboard [1]") });
            main2.buttons.Add(new ButtonData { buttonText = "Models", method = () => GoToPage("Models [1]") });
            main2.buttons.Add(new ButtonData { buttonText = "Debug", method = () => GoToPage("Debug [1]") });
            allPages.Add(main2);

            // --- Settings ---
            MenuPage settings = new MenuPage { pageName = "Settings [1]" };
            settings.buttons.Add(new ButtonData { buttonText = "Custom Stump Boards", method = () => StumpBoards.Boards(), offMethod = () => StumpBoards.BoardsOff(), isToggleable = true, isToggled = true });
            settings.buttons.Add(new ButtonData { buttonText = "Plat Colour", method = Switchers.PlatColourChanger, isToggleable = false, isToggled = false });
            settings.buttons.Add(new ButtonData { buttonText = "TPGun Colour", method = Switchers.TPGunColourChanger, isToggleable = false, isToggled = false });
            allPages.Add(settings);

            // --- Safety ---
            MenuPage safety = new MenuPage { pageName = "Safety [1]" };
            safety.buttons.Add(new ButtonData { buttonText = "Anti-Rep(Disconnect)", method = Safety.AntiReportDisconnect, isToggleable = true, isToggled = false });
            safety.buttons.Add(new ButtonData { buttonText = "Anti-Rep(Quit)", method = Safety.AntiReportQuit, isToggleable = true, isToggled = false });
            safety.buttons.Add(new ButtonData { buttonText = "Anti-Rep(Reconnect)", method = Safety.AntiReportReconnect, isToggleable = true, isToggled = false });
            safety.buttons.Add(new ButtonData { buttonText = "Bypass VC Ban", method = Safety.BypassVCBan, isToggleable = true, isToggled = false });
            safety.buttons.Add(new ButtonData { buttonText = "Anti-Ban", method = Safety.AntiBan, isToggleable = true, isToggled = false });
            safety.buttons.Add(new ButtonData { buttonText = "Visualise Anti-Report", method = Safety.VisualizeAntiReportRadius, offMethod = Safety.CleanupAntiReportVisualization, isToggleable = true, isToggled = false });
            allPages.Add(safety);

            MenuPage safety2 = new MenuPage { pageName = "Safety [2]" };
            safety2.buttons.Add(new ButtonData { buttonText = "Restart Game", method = Safety.RestartGame, isToggleable = false, isToggled = false });
            safety2.buttons.Add(new ButtonData { buttonText = "Quit Game", method = Safety.QuitGame, isToggleable = false, isToggled = false });
            safety2.buttons.Add(new ButtonData { buttonText = "Join Rand(Discon 1st)", method = () => Safety.JoinRandom(), isToggleable = false, isToggled = false });
            allPages.Add(safety2);

            // --- Movement ---
            MenuPage movement = new MenuPage { pageName = "Movement [1]" };
            movement.buttons.Add(new ButtonData { buttonText = "Speed Boost", method = Movement.Speedboost, offMethod = Movement.NormalSpeed, isToggleable = true, isToggled = false });
            movement.buttons.Add(new ButtonData { buttonText = "Fly", method = Movement.Fly, isToggleable = true, isToggled = false });
            movement.buttons.Add(new ButtonData { buttonText = "Platforms", method = Movement.Platforms, offMethod = Movement.PlatOff, isToggleable = true, isToggled = false });
            movement.buttons.Add(new ButtonData { buttonText = "Plat Colour", method = Switchers.PlatColourChanger, isToggleable = false, isToggled = false });
            movement.buttons.Add(new ButtonData { buttonText = "Noclip", method = Movement.Noclip, offMethod = Movement.NoclipOff, isToggleable = true, isToggled = false });
            movement.buttons.Add(new ButtonData { buttonText = "Noclip Fly", method = Movement.NoclipFly, offMethod = Movement.NoclipFlyOff, isToggleable = true, isToggled = false });
            allPages.Add(movement);

            MenuPage movement2 = new MenuPage { pageName = "Movement [2]" };
            movement2.buttons.Add(new ButtonData { buttonText = "Long Arms", method = Movement.LongArms, offMethod = Movement.LongArmsOff, isToggleable = true, isToggled = false });
            movement2.buttons.Add(new ButtonData { buttonText = "Obj Grav: Normal", method = Movement.GlobalGravity, isToggleable = false, isToggled = false });
            movement2.buttons.Add(new ButtonData { buttonText = "P Grav: Normal", method = Movement.PlayerGravity, isToggleable = false, isToggled = false });
            allPages.Add(movement2);

            // --- Rig ---
            MenuPage rig = new MenuPage { pageName = "Rig [1]" };
            rig.buttons.Add(new ButtonData { buttonText = "Ghost Monke", method = Rig.GhostMonke, offMethod = Rig.GhostMonkeOff, isToggleable = true, isToggled = false });
            rig.buttons.Add(new ButtonData { buttonText = "Invis", method = Rig.Invis, offMethod = Rig.InvisOff, isToggleable = true, isToggled = false });
            allPages.Add(rig);

            // --- Visual ---
            MenuPage visual = new MenuPage { pageName = "Visual [1]" };
            visual.buttons.Add(new ButtonData { buttonText = "Box ESP", method = Visual.BoxESP, offMethod = Visual.BoxESPOff, isToggleable = true, isToggled = false });
            visual.buttons.Add(new ButtonData { buttonText = "Tracers", method = Visual.Tracers, offMethod = Visual.TracersOff, isToggleable = true, isToggled = false });
            allPages.Add(visual);

            // --- OP ---
            MenuPage op = new MenuPage { pageName = "OP [1]" };
            op.buttons.Add(new ButtonData { buttonText = "TP Gun", method = OP.TPGun, offMethod = OP.TPGunOff, isToggleable = true, isToggled = false });
            op.buttons.Add(new ButtonData { buttonText = "TPGun Colour", method = Switchers.TPGunColourChanger, isToggleable = false, isToggled = false });
            allPages.Add(op);

            // --- Network ---
            MenuPage network = new MenuPage { pageName = "Network [1]" };
            network.buttons.Add(new ButtonData { buttonText = "Join Menu Code", method = () => Safety.JoinRoom("PLPMENU"), isToggleable = false, isToggled = false });
            network.buttons.Add(new ButtonData { buttonText = "Join Rand", method = () => Safety.JoinRandom(), isToggleable = false, isToggled = false });
            network.buttons.Add(new ButtonData { buttonText = "Join DAISY09", method = () => Safety.JoinRoom("DAISY09"), isToggleable = false, isToggled = false });
            network.buttons.Add(new ButtonData { buttonText = "Join J3VU", method = () => Safety.JoinRoom("J3VU"), isToggleable = false, isToggled = false });
            network.buttons.Add(new ButtonData { buttonText = "Join PBBV", method = () => Safety.JoinRoom("PPBV"), isToggleable = false, isToggled = false });
            network.buttons.Add(new ButtonData { buttonText = "Join ECHO", method = () => Safety.JoinRoom("ECHO"), isToggleable = false, isToggled = false });
            allPages.Add(network);

            // --- Soundboard ---
            MenuPage sb = new MenuPage { pageName = "Soundboard [1]" };
            sb.buttons.Add(new ButtonData { buttonText = "Stop Sounds", method = VirtualMicPlayer.StopAll, isToggleable = false, isToggled = false });

            // Loop through every sound we found in the folder
            for (int i = 0; i < SoundManager.soundFileNames.Count; i++)
            {
                string nameForButton = SoundManager.soundFileNames[i];
                string actualFilePath = SoundManager.soundFilePaths[i];

                sb.buttons.Add(new ButtonData
                {
                    buttonText = nameForButton,
                    isToggleable = false,
                    isToggled = false,
                    audioFilePath = actualFilePath // Store the path here!
                });
            }
            allPages.Add(sb);

            // --- Models ---
            MenuPage models = new MenuPage { pageName = "Models [1]" };
            models.buttons.Add(new ButtonData { buttonText = "Sword 1", method = () => ModelLoader.SpawnModel("sword1", "sword1"), offMethod = () => ModelLoader.DespawnModel(), isToggleable = true, isToggled = false });
            allPages.Add(models);

            // --- Debug ---
            MenuPage debug = new MenuPage { pageName = "Debug [1]" };
            debug.buttons.Add(new ButtonData { buttonText = "Base Spd Vals", method = DebugMods.SpeedValueChecker, isToggleable = false, isToggled = false });
            debug.buttons.Add(new ButtonData { buttonText = "Lobby Stats", method = DebugMods.ConnectedLobbyCode, isToggleable = false, isToggled = false });
            debug.buttons.Add(new ButtonData { buttonText = "Active Colliders", method = DebugMods.ActiveColliders, isToggleable = false, isToggled = false });
            debug.buttons.Add(new ButtonData { buttonText = "Current Gravity", method = DebugMods.CurrentGrav, isToggleable = false, isToggled = false });
            debug.buttons.Add(new ButtonData { buttonText = "Show Hand Axis", method = () => DebugMods.ShowHandAxis(), offMethod = () => DebugMods.ShowHandAxisOff(), isToggleable = true, isToggled = false });
            allPages.Add(debug);

            // Example of registering a Constant Button:
            // constModButtons.Add(new ConstButtonData { buttonText = "Next Page", method = Switchers.NextPage, isToggleable = false, isToggled = false, localPos = new Vector3(0.062f, 0.2f, 0.11f), localScale = new Vector3(0.03f, 0.05f, 0.04f) });
            constModButtons.Add(new ConstButtonData { buttonText = "Disconnect", method = Disconnect.DisconnectButton, isToggleable = false, isToggled = false, localPos = new Vector3(0.055f, 0f, 0.23f), localScale = new Vector3(0.04f, 0.25f, 0.04f), buttonColour = Color.black });
            constModButtons.Add(new ConstButtonData { buttonText = ">", method = NextPage, isToggleable = false, isToggled = false, localPos = new Vector3(0.055f, -0.17f, -0.21f), localScale = new Vector3(0.04f, 0.05f, 0.05f), buttonColour = Color.black });
            constModButtons.Add(new ConstButtonData { buttonText = "<", method = PrevPage, isToggleable = false, isToggled = false, localPos = new Vector3(0.055f, 0.17f, -0.21f), localScale = new Vector3(0.04f, 0.05f, 0.05f), buttonColour = Color.black });
            constModButtons.Add(new ConstButtonData { buttonText = "Home", method = () => GoToPage("Main [1]"), localPos = new Vector3(0.055f, 0f, -0.21f), localScale = new Vector3(0.04f, 0.25f, 0.05f), buttonColour = Color.black });
        }

        void Update()
        {
            if (ControllerInputPoller.instance == null) return;

            if (!isMenuCreated && ControllerInputPoller.instance.leftControllerPrimaryButton)
            {
                Debug.Log($"The button was pressed");
                CreateMenu();
            }
            else if (isMenuCreated && !ControllerInputPoller.instance.leftControllerPrimaryButton)
            {
                DestroyMenu();
            }

            // Loop for normal buttons
            foreach (MenuPage page in allPages)
            {
                foreach (ButtonData btn in page.buttons)
                {
                    if (btn.isToggled && btn.method != null)
                    {
                        btn.method.Invoke();
                    }
                    if (!btn.isToggled && btn.offMethod != null)
                    {
                        btn.offMethod.Invoke();
                    }
                }
            }

            // FIX 2: Added loop for constant buttons so their logic actually runs!
            foreach (ConstButtonData cBtn in constModButtons)
            {
                if (cBtn.isToggled && cBtn.method != null)
                {
                    cBtn.method.Invoke();
                }
                if (!cBtn.isToggled && cBtn.offMethod != null)
                {
                    cBtn.offMethod.Invoke();
                }
            }

            if (Movement.playerGravInt != 0)
            {
                // Standard Gorilla Locomotion Rigidbody reference
                Rigidbody playerRb = GorillaTagger.Instance.GetComponent<Rigidbody>();

                if (playerRb != null)
                {
                    playerRb.AddForce(Vector3.up * Movement.gravAmount, ForceMode.Acceleration);
                }
            }
        }

        void CreateMenu()
        {
            isMenuCreated = true;
            var player = GTPlayer.Instance;

            menuObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            menuObj.transform.parent = player.LeftHand.controllerTransform;
            menuObj.transform.localPosition = new Vector3(0.05f, 0f, 0f);
            menuObj.transform.localRotation = Quaternion.identity;
            menuObj.transform.localScale = new Vector3(0.03f, 0.25f, 0.35f);

            Destroy(menuObj.GetComponent<Rigidbody>());
            Destroy(menuObj.GetComponent<Collider>());

            var rend = menuObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = Color.black;

            // --- Create Page Title Text ---
            GameObject titleObj = new GameObject("MenuPageTitle");
            titleObj.transform.SetParent(menuObj.transform, false);

            // Using your exact depth (X), centered (Y), and top height (Z) offset
            titleObj.transform.localPosition = new Vector3(0.501f, 0f, 0.45f);

            // Match the rotation and scale used by your button labels
            titleObj.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);
            // The smaller Y value (0.01f) stops the vertical stretching!
            titleObj.transform.localScale = new Vector3(0.015f, 0.02f, 0.05f);

            titleText = titleObj.AddComponent<TextMeshPro>();
            titleText.fontSize = 35;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white; // Set your preferred title color
            titleText.enableAutoSizing = true;

            // Wider rect box so longer page names like "Cosmetics" don't wrap weirdly
            titleText.rectTransform.sizeDelta = new Vector2(160f, 25f);
            // -----------------------------

            // --- Create Custom Hand Pointer ---
            pointerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointerObj.name = "MenuPointer";
            // Attach it strictly to the raw VR controller, bypassing the Gorilla Rig
            pointerObj.transform.SetParent(player.RightHand.controllerTransform, false);
            pointerObj.transform.localPosition = new Vector3(0f, -0.1f, 0f); // Roughly at the fingertips
            pointerObj.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f); // Tiny sphere

            // Make it invisible
            var pointerRend = pointerObj.GetComponent<Renderer>();
            pointerRend.material.shader = Shader.Find("GUI/Text Shader");
            pointerRend.material.color = Color.white;

            pointerObj.layer = menuLayer;

            // Set up the colliders and physics
            menuHandCollider = pointerObj.GetComponent<SphereCollider>();
            menuHandCollider.isTrigger = true;

            Rigidbody rb = pointerObj.AddComponent<Rigidbody>();
            rb.isKinematic = true; // Prevents gravity from pulling it down
            rb.useGravity = false;
            // ----------------------------------

            // FIX 3: Actually generate the constant buttons when the menu opens!
            foreach (ConstButtonData cBtnData in constModButtons)
            {
                AddConstButton(cBtnData);
            }

            GoToPage(currentPageName);
        }

        void DestroyMenu()
        {
            isMenuCreated = false;
            GameObject.Destroy(menuObj);
            if (pointerObj != null) GameObject.Destroy(pointerObj);
            DestroyAllButtons();
        }

        void AddButton(float zOffset, ButtonData bData)
        {
            GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            btnObj.GetComponent<Collider>().isTrigger = true;
            var player = GTPlayer.Instance;

            btnObj.transform.parent = player.LeftHand.controllerTransform;
            btnObj.transform.localPosition = new Vector3(0.062f, 0f, zOffset);
            btnObj.transform.localRotation = Quaternion.identity;

            btnObj.transform.localScale = new Vector3(0.03f, 0.2f, 0.04f);

            var rend = btnObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = bData.isToggled ? Color32Lib.menuButtonRed : Color32Lib.menuButtonWhite;

            Rigidbody btnRb = btnObj.AddComponent<Rigidbody>();
            btnRb.isKinematic = true;
            btnRb.useGravity = false;

            if (bData.buttonColour != null)
            {
                rend.material.color = bData.buttonColour.Value;
            }

            if (bData.buttonText == "Plat Colour")
            {
                rend.material.color = Switchers.platCurrentColor;
            }

            if (bData.buttonText == "TPGun Colour")
            {
                rend.material.color = Switchers.tpGunCurrentColor;
            }

            if (rend.material.color == Settings.black || rend.material.color == Settings.purple || rend.material.color == Settings.blue)
            {
                bData.textColour = Settings.white;
            }
            else
            {
                bData.textColour = Settings.black;
            }

            btnObj.layer = menuLayer;

            var trigger = btnObj.AddComponent<ButtonActivation>();
            trigger.referenceData = bData;
            trigger.buttonRenderer = rend;

            var textObject = new GameObject("ButtonLabel");
            textObject.transform.SetParent(btnObj.transform);
            textObject.transform.localPosition = new Vector3(0.501f, 0f, 0f);
            textObject.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);

            if (bData.buttonText.Contains("Obj Grav"))
            {
                switch (Movement.globalGravInt)
                {
                    case 0: bData.buttonText = "Obj Grav: Normal"; break;
                    case 1: bData.buttonText = "Obj Grav: Low"; break;
                    case 2: bData.buttonText = "Obj Grav: High"; break;
                    case 3: bData.buttonText = "Obj Grav: Zero"; break;
                }
            }

            if (bData.buttonText.Contains("P Grav"))
            {
                // Check what the integer was changed to, and update the text string
                switch (Movement.playerGravInt)
                {
                    case 0: bData.buttonText = "P Grav: Normal"; break;
                    case 1: bData.buttonText = "P Grav: Low"; break;
                    case 2: bData.buttonText = "P Grav: High"; break;
                    case 3: bData.buttonText = "P Grav: Zero"; break;
                }
            }

            var text = textObject.AddComponent<TextMeshPro>();
            text.text = bData.buttonText;
            text.fontSize = 30;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = bData.textColour;
            text.enableAutoSizing = true;
            text.rectTransform.sizeDelta = new Vector2(80f, 15f);
            text.transform.localScale = new Vector3(0.01f, 0.066f, 0.05f);

            trigger.buttonText = text;
            btnObjs.Add(btnObj);
        }

        void AddConstButton(ConstButtonData bData)
        {
            GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            btnObj.GetComponent<Collider>().isTrigger = true;
            var player = GTPlayer.Instance;

            btnObj.transform.parent = player.LeftHand.controllerTransform;
            btnObj.transform.localPosition = bData.localPos;
            btnObj.transform.localRotation = Quaternion.identity;

            btnObj.transform.localScale = bData.localScale;

            var rend = btnObj.GetComponent<Renderer>();
            rend.material.shader = Shader.Find("GorillaTag/UberShader");
            rend.material.color = bData.isToggled ? Color32Lib.menuButtonRed : Color32Lib.menuButtonWhite;

            Rigidbody btnRb = btnObj.AddComponent<Rigidbody>();
            btnRb.isKinematic = true;
            btnRb.useGravity = false;

            if (bData.buttonColour != null)
            {
                rend.material.color = bData.buttonColour.Value;
            }

            if (rend.material.color == Settings.black || rend.material.color == Settings.purple || rend.material.color == Settings.blue)
            {
                bData.textColour = Settings.white;
            }
            else
            {
                bData.textColour = Settings.black;
            }

            btnObj.layer = menuLayer;

            var trigger = btnObj.AddComponent<ButtonActivation>();
            // This line now works flawlessly because ConstButtonData inherits from ButtonData!
            trigger.referenceData = bData;
            trigger.buttonRenderer = rend;

            var textObject = new GameObject("ButtonLabel");
            textObject.transform.SetParent(btnObj.transform);
            textObject.transform.localPosition = new Vector3(0.501f, 0f, 0f);
            textObject.transform.localRotation = Quaternion.Euler(0f, -90f, -90f);

            var text = textObject.AddComponent<TextMeshPro>();
            text.text = bData.buttonText;
            text.fontSize = 30;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = bData.textColour;
            text.enableAutoSizing = true;
            text.rectTransform.sizeDelta = new Vector2(80f, 15f);

            // 1. Calculate the perfect world scale using the CROSSED axes from a normal button
            // Normal Button Scale: X=0.03, Y=0.2, Z=0.04
            // Normal Text Scale: X=0.01, Y=0.066, Z=0.05
            float targetWorldWidth = 0.2f * 0.01f;    // Parent Y * Text X (0.002)
            float targetWorldHeight = 0.04f * 0.066f; // Parent Z * Text Y (0.00264)
            float targetWorldDepth = 0.03f * 0.05f;   // Parent X * Text Z (0.0015)

            // 2. Divide by the constant button's mapped axes
            float localX = targetWorldWidth / bData.localScale.y;
            float localY = targetWorldHeight / bData.localScale.z;
            float localZ = targetWorldDepth / bData.localScale.x;

            // 3. Apply the perfectly rotated scale!
            text.transform.localScale = new Vector3(localX, localY, localZ);

            trigger.buttonText = text;

            // Adding it to btnObjs ensures it gets destroyed safely when the menu closes
            constBtnObjs.Add(btnObj);
        }

        void DestroyAllButtons()
        {
            foreach (GameObject btnObj in btnObjs) Destroy(btnObj);
            btnObjs.Clear();
            foreach (GameObject btnObj in constBtnObjs) Destroy(btnObj);
            constBtnObjs.Clear();
        }

        private static LineRenderer laserLine;
        private static GameObject pointerSphere;

        // NEW: This locks the teleport so it only happens once per click
        private static bool isTeleportLock = false;

        void LateUpdate()
        {
            // Update colors at the top (safe because of null checks)
            if (laserLine != null)
            {
                laserLine.startColor = Switchers.tpGunCurrentColor;
                laserLine.endColor = Switchers.tpGunCurrentColor;
                laserLine.material.shader = Shader.Find("GUI/Text Shader");
            }

            if (pointerSphere != null)
            {
                pointerSphere.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                pointerSphere.GetComponent<Renderer>().material.color = Switchers.tpGunCurrentColor;
            }

            if (OP.tpGunOn)
            {
                if (GTPlayer.Instance != null && GTPlayer.Instance.RightHand.controllerTransform != null)
                {
                    Transform rightController = GTPlayer.Instance.RightHand.controllerTransform;
                    float maxTeleportDistance = 75f;

                    if (Physics.Raycast(rightController.position, rightController.forward, out RaycastHit hitInfo, maxTeleportDistance))
                    {
                        // Render code
                        if (laserLine == null)
                        {
                            GameObject lineObj = new GameObject("TeleportLaser");
                            laserLine = lineObj.AddComponent<LineRenderer>();
                            laserLine.startWidth = 0.02f;
                            laserLine.endWidth = 0.02f;
                            laserLine.material.shader = Shader.Find("GUI/Text Shader");
                        }

                        if (pointerSphere == null)
                        {
                            pointerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            pointerSphere.name = "TeleportReticle";
                            pointerSphere.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                            pointerSphere.GetComponent<Renderer>().material.shader = Shader.Find("GUI/Text Shader");
                            GameObject.Destroy(pointerSphere.GetComponent<Collider>());
                        }

                        laserLine.SetPosition(0, rightController.position);
                        laserLine.SetPosition(1, hitInfo.point);
                        pointerSphere.transform.position = hitInfo.point;

                        laserLine.enabled = true;
                        pointerSphere.SetActive(true);

                        // --- THE FIX: Input Logic with a Lock ---
                        bool triggerPulled = ControllerInputPoller.instance.rightControllerIndexFloat > 0.1f;

                        if (triggerPulled && !isTeleportLock) // Only runs if trigger is pulled AND not locked
                        {
                            Vector3 safeTeleportTarget = hitInfo.point + (Vector3.up * 0.15f);
                            GTPlayer.Instance.transform.position = safeTeleportTarget;

                            Rigidbody playerRigidbody = GTPlayer.Instance.GetComponent<Rigidbody>();
                            if (playerRigidbody != null)
                            {
                                playerRigidbody.linearVelocity = Vector3.zero;
                                playerRigidbody.angularVelocity = Vector3.zero;
                            }

                            // Lock it! It won't teleport again until you release the trigger
                            isTeleportLock = true;
                        }
                        else if (!triggerPulled)
                        {
                            // You let go of the trigger, unlock it for the next teleport
                            isTeleportLock = false;
                        }
                    }
                    else
                    {
                        // --- THE FIX: Prevent crash if pointing at the sky too early ---
                        if (laserLine != null)
                        {
                            laserLine.SetPosition(0, rightController.position);
                            laserLine.SetPosition(1, rightController.position + (rightController.forward * maxTeleportDistance));
                        }

                        if (pointerSphere != null)
                        {
                            pointerSphere.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                // Cleanup when mod is off
                if (laserLine != null && laserLine.enabled) laserLine.enabled = false;
                if (pointerSphere != null && pointerSphere.activeSelf) pointerSphere.SetActive(false);
            }
        }
    }

    public class ButtonActivation : MonoBehaviour
    {
        public static float cooldown = 0f;

        public ButtonData referenceData;
        public Renderer buttonRenderer;
        public TextMeshPro buttonText;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != 0) return;
            if (other.name != "MenuPointer") return;
            
            if (Time.time < cooldown) return;
            cooldown = Time.time + 0.3f;

            Debug.Log("The button " + referenceData.buttonText + " was pressed");

            GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
            GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(8, true, 0.4f);

            if (referenceData.isToggleable)
            {
                if (!referenceData.isToggled && referenceData.offMethod != null)
                {
                    referenceData.offMethod.Invoke();
                }

                referenceData.isToggled = !referenceData.isToggled;
                buttonRenderer.material.color = referenceData.isToggled ? Color32Lib.menuButtonRed : Color32Lib.menuButtonWhite;

                if (referenceData.buttonColour != null)
                {
                    buttonRenderer.material.color = referenceData.buttonColour.Value;
                }

                if (buttonRenderer.material.color == Settings.black || buttonRenderer.material.color == Settings.purple || buttonRenderer.material.color == Settings.blue)
                {
                    referenceData.textColour = Settings.white;
                }
                else
                {
                    referenceData.textColour = Settings.black;
                }

                if (buttonText != null) buttonText.color = referenceData.textColour;
            }
            else
            {
                if (!string.IsNullOrEmpty(referenceData.audioFilePath))
                {
                    VirtualMicPlayer.PlaySound(referenceData.audioFilePath);
                }

                referenceData.method?.Invoke();

                if (referenceData.buttonText.Contains("Obj Grav"))
                {
                    // Check what the integer was changed to, and update the text string
                    switch (Movement.globalGravInt)
                    {
                        case 0: referenceData.buttonText = "Obj Grav: Normal"; break;
                        case 1: referenceData.buttonText = "Obj Grav: Low"; break;
                        case 2: referenceData.buttonText = "Obj Grav: High"; break;
                        case 3: referenceData.buttonText = "Obj Grav: Zero"; break;
                    }

                    // Apply the new text string to the actual 3D TextMeshPro object
                    if (buttonText != null)
                    {
                        buttonText.text = referenceData.buttonText;
                    }
                }

                if (referenceData.buttonText.Contains("P Grav"))
                {
                    // Check what the integer was changed to, and update the text string
                    switch (Movement.playerGravInt)
                    {
                        case 0: referenceData.buttonText = "P Grav: Normal"; break;
                        case 1: referenceData.buttonText = "P Grav: Low"; break;
                        case 2: referenceData.buttonText = "P Grav: High"; break;
                        case 3: referenceData.buttonText = "P Grav: Zero"; break;
                    }

                    // Apply the new text string to the actual 3D TextMeshPro object
                    if (buttonText != null)
                    {
                        buttonText.text = referenceData.buttonText;
                    }
                }

                if (referenceData.buttonColour != null)
                {
                    buttonRenderer.material.color = referenceData.buttonColour.Value;
                }

                if (referenceData.buttonText == "Plat Colour")
                {
                    buttonRenderer.material.color = Switchers.platCurrentColor;
                }

                if (referenceData.buttonText == "TPGun Colour")
                {
                    buttonRenderer.material.color = Switchers.tpGunCurrentColor;
                }

                if (buttonRenderer.material.color == Settings.black || buttonRenderer.material.color == Settings.purple || buttonRenderer.material.color == Settings.blue)
                {
                    referenceData.textColour = Settings.white;
                }
                else
                {
                    referenceData.textColour = Settings.black;
                }

                if (buttonText != null) buttonText.color = referenceData.textColour;
            }
        }
    }

    /*public class FollowMenu : MonoBehaviour
    {
        public Transform target;
        public Vector3 position;
        public Quaternion rotation;

        void LateUpdate()
        {
            transform.position = target.TransformPoint(position);
            transform.rotation = target.rotation * rotation;
        }
    }*/
}