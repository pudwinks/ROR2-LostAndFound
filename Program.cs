using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace LostAndFound
{
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class LostAndFoundPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Pudwinks";
        public const string PluginName = "LostAndFound";
        public const string PluginVersion = "1.2.5";

        public static readonly List<Interactable> interactables = new List<Interactable>();

        private static ConfigEntry<bool> showNewtAltars;
        private static ConfigEntry<bool> showLunarPods;
        private static ConfigEntry<bool> showPrinters;
        private static ConfigEntry<bool> showDrones;
        private static ConfigEntry<bool> showDroneScrappers;
        private static ConfigEntry<bool> showItemsOnGround;
        private static ConfigEntry<bool> showBarrels;
        private static ConfigEntry<bool> showContents;
        public static ConfigEntry<bool> showVoidKeyContents;
        public static ConfigEntry<bool> showSecretNtormatPages;
        private static ConfigEntry<bool> showText;
        private static ConfigEntry<int> textSize;
        private static ConfigEntry<int> textBottomOffset;

        internal static GameObject effectPrefab;

        private static GameObject textCanvas;
        private static HGTextMeshProUGUI recapText;

        public static bool didScrap;

        private static ConfigEntry<KeyboardShortcut> nextKey;
        private static ConfigEntry<KeyboardShortcut> previousKey;
        private static ConfigEntry<KeyboardShortcut> zoomOutKey;
        private static ConfigEntry<KeyboardShortcut> zoomInKey;
        private static ConfigEntry<KeyboardShortcut> finishKey;

        private static SceneExitController heldExit;
        private static bool releasing;

        private static int currentIndex = -1;

        private const float defaultOrbitYaw = 0f;
        private const float defaultOrbitPitch = 15.0f;
        private const float defaultOrbitDistance = 23f;
        private static float orbitYaw = defaultOrbitYaw;
        private static float orbitPitch = defaultOrbitPitch;
        private static float orbitDistance = defaultOrbitDistance;
        private const float orbitPitchLimit = 89f;

        private const float zoomSpeed = 35f;
        private const float minOrbitDistance = 1f;

        private void Awake()
        {
            showNewtAltars = Config.Bind("General", "Show newt altars", false,
                "Set to true if you want to see newt altars in the recap. Default is false to avoid potential spoilers.");
            RiskOfOptionsStuff.AddOption(showNewtAltars);

            showLunarPods = Config.Bind("General", "Show lunar pods", false,
                "Set to true if you want to see lunar pods in the recap.");
            RiskOfOptionsStuff.AddOption(showLunarPods);

            showPrinters = Config.Bind("General", "Show white/green 3D printers", true,
                "Set to true if you want to see white and green 3D printers in the recap. Setting this to false will only hide white and green printers, since they are the most common. Red and boss printers remain visible regardless of this setting.");
            RiskOfOptionsStuff.AddOption(showPrinters);

            showDrones = Config.Bind("General", "Show drones", true,
                "Set to true if you want to see drones in the recap. TC-280 drone always shows up regardless of this setting. Because it's a cool drone.");
            RiskOfOptionsStuff.AddOption(showDrones);

            showDroneScrappers = Config.Bind("General", "Show drone scrappers", false,
                "Set to true if you want to see drone scrappers in the recap.");
            RiskOfOptionsStuff.AddOption(showDroneScrappers);

            showItemsOnGround = Config.Bind("General", "Show items on ground", true,
                "Set to true if you want to see items on the ground in the recap. If you forgot to pick something up, it'll show up. Does not show void items.");
            RiskOfOptionsStuff.AddOption(showItemsOnGround);

            showBarrels = Config.Bind("General", "Show barrels", false,
                "Set to true if you want to see barrels in the recap.");
            RiskOfOptionsStuff.AddOption(showBarrels);

            showContents = Config.Bind("General", "Show container contents", true,
                "Set to true if you want to see container contents in the recap.");
            RiskOfOptionsStuff.AddOption(showContents);

            showVoidKeyContents = Config.Bind("General", "Show Encrusted Cache contents", true,
                "Set to true if you want to see the contents of Encrusted Caches (corrupted form of the Rusted Key lockbox). Irrelevant if \"Show container contents\" is false.");
            RiskOfOptionsStuff.AddOption(showVoidKeyContents);

            showSecretNtormatPages = Config.Bind("General", "Show secret Ntormat pages", true,
               "Set to true if you want to see the secret Ntormat pages on the ground in the recap.");
            RiskOfOptionsStuff.AddOption(showSecretNtormatPages);

            showText = Config.Bind("General", "Show recap text", true,
                "Set to true if you want to see the recap counter text (e.g. 3/17) at the bottom of the screen.");
            RiskOfOptionsStuff.AddOption(showText);

            textSize = Config.Bind("General", "Recap text size", 28,
                new ConfigDescription("Font size of the recap counter text.",
                    new AcceptableValueRange<int>(12, 200)));
            RiskOfOptionsStuff.AddOption(textSize);

            textBottomOffset = Config.Bind("General", "Recap text bottom offset", 200,
                new ConfigDescription("Distance in pixels between the bottom of the screen and the recap counter text.",
                    new AcceptableValueRange<int>(0, 1000)));
            RiskOfOptionsStuff.AddOption(textBottomOffset);

            nextKey = Config.Bind("Keybinds", "Next interactable", new KeyboardShortcut(KeyCode.Mouse0),
                "Which key to use to show the next interactable in the recap.");
            RiskOfOptionsStuff.AddOption(nextKey);

            previousKey = Config.Bind("Keybinds", "Previous interactable", new KeyboardShortcut(KeyCode.Mouse1),
                "Which key to use to show the previous interactable in the recap.");
            RiskOfOptionsStuff.AddOption(previousKey);

            zoomOutKey = Config.Bind("Keybinds", "Zoom out", new KeyboardShortcut(KeyCode.A),
                "Which key to use to zoom out the camera in the recap.");
            RiskOfOptionsStuff.AddOption(zoomOutKey);

            zoomInKey = Config.Bind("Keybinds", "Zoom in", new KeyboardShortcut(KeyCode.D),
                "Which key to use to zoom in the camera in the recap.");
            RiskOfOptionsStuff.AddOption(zoomInKey);

            finishKey = Config.Bind("Keybinds", "Finish recap", new KeyboardShortcut(KeyCode.S),
                "Which key to use to finish the recap once it's started.");
            RiskOfOptionsStuff.AddOption(finishKey);

            effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC3/MealPrep/PickupTakenOrbEffect.prefab").WaitForCompletion();

            On.RoR2.PurchaseInteraction.Start += PurchaseInteraction_Start;
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ScrapperController.PreStartClient += ScrapperController_PreStartClient;
            On.RoR2.ScrapperController.BeginScrapping_UniquePickup += ScrapperController_BeginScrapping_UniquePickup;
            On.RoR2.MultiShopController.Start += MultiShopController_Start;
            On.RoR2.DroneVendorMultiShopController.Start += DroneVendorMultiShopController_Start;
            On.RoR2.GenericPickupController.Start += GenericPickupController_Start;
            On.RoR2.PickupPickerController.OnEnable += PickupPickerController_OnEnable;
            On.RoR2.BarrelInteraction.OnEnable += BarrelInteraction_OnEnable;
            On.RoR2.SceneExitController.SetState += SceneExitController_SetState;
            On.RoR2.Run.OnDestroy += Run_OnDestroy;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDestroy()
        {
            On.RoR2.PurchaseInteraction.Start -= PurchaseInteraction_Start;
            On.RoR2.PurchaseInteraction.OnInteractionBegin -= PurchaseInteraction_OnInteractionBegin;
            On.RoR2.ScrapperController.PreStartClient -= ScrapperController_PreStartClient;
            On.RoR2.ScrapperController.BeginScrapping_UniquePickup -= ScrapperController_BeginScrapping_UniquePickup;
            On.RoR2.MultiShopController.Start -= MultiShopController_Start;
            On.RoR2.DroneVendorMultiShopController.Start -= DroneVendorMultiShopController_Start;
            On.RoR2.GenericPickupController.Start -= GenericPickupController_Start;
            On.RoR2.PickupPickerController.OnEnable -= PickupPickerController_OnEnable;
            On.RoR2.BarrelInteraction.OnEnable -= BarrelInteraction_OnEnable;
            On.RoR2.SceneExitController.SetState -= SceneExitController_SetState;
            On.RoR2.Run.OnDestroy -= Run_OnDestroy;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;

            if (textCanvas)
                Destroy(textCanvas);
        }

        private void Update()
        {
            if (heldExit == null)
                return;

            if (PauseManager.PauseScreenIsOpen())
                return;

            if (Input.GetKeyDown(nextKey.Value.MainKey))
                MoveTo(currentIndex + 1);

            if (Input.GetKeyDown(previousKey.Value.MainKey))
                MoveTo(currentIndex - 1);

            if (Input.GetKeyDown(finishKey.Value.MainKey))
            {
                Resume();
                return;
            }

            if (currentIndex < 0)
                return;

            if (Input.GetKey(zoomOutKey.Value.MainKey))
                orbitDistance += zoomSpeed * Time.unscaledDeltaTime;

            if (Input.GetKey(zoomInKey.Value.MainKey))
                orbitDistance -= zoomSpeed * Time.unscaledDeltaTime;

            if (orbitDistance < minOrbitDistance)
                orbitDistance = minOrbitDistance;

            Vector2 look = ReadMouseLook();
            orbitYaw += look.x;
            orbitPitch = Mathf.Clamp(orbitPitch - look.y, -orbitPitchLimit, orbitPitchLimit);
            ApplyCamera();
        }

        private void LateUpdate()
        {
            bool visible = heldExit != null && currentIndex >= 0 && showText.Value && !PauseManager.PauseScreenIsOpen();

            if (!visible)
            {
                if (textCanvas)
                    textCanvas.SetActive(false);
                return;
            }

            // Created lazily so HGTextMeshProUGUI picks up the language font, which isn't loaded yet in Awake
            if (!textCanvas)
                CreateRecapText();

            textCanvas.SetActive(true);
            recapText.fontSize = textSize.Value;
            recapText.rectTransform.anchoredPosition = new Vector2(0f, textBottomOffset.Value);
            recapText.text = $"{currentIndex + 1}/{interactables.Count}";
        }

        private static void CreateRecapText()
        {
            textCanvas = new GameObject("LostAndFoundRecapCanvas");
            DontDestroyOnLoad(textCanvas);

            Canvas canvas = textCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            GameObject textObject = new GameObject("RecapText", typeof(RectTransform));
            textObject.transform.SetParent(textCanvas.transform, false);

            recapText = textObject.AddComponent<HGTextMeshProUGUI>();
            recapText.alignment = TextAlignmentOptions.Bottom;
            recapText.enableWordWrapping = false;
            recapText.raycastTarget = false;

            RectTransform rect = recapText.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(2000f, 400f);
        }

        private static Vector2 ReadMouseLook()
        {
            LocalUser user = LocalUserManager.GetFirstLocalUser();
            UserProfile profile = user?.userProfile;

            if (user?.inputPlayer == null || profile == null)
                return Vector2.zero;

            Vector2 look = new Vector2(
                user.inputPlayer.GetAxisRaw(RewiredConsts.Action.AimHorizontalMouse),
                user.inputPlayer.GetAxisRaw(RewiredConsts.Action.AimVerticalMouse));

            if (profile.mouseLookInvertX)
                look.x = -look.x;
            if (profile.mouseLookInvertY)
                look.y = -look.y;

            return look * new Vector2(profile.mouseLookScaleX, profile.mouseLookScaleY) * profile.mouseLookSensitivity;
        }

        private void MoveTo(int index)
        {
            if (interactables.Count == 0)
                return;

            currentIndex = Mathf.Clamp(index, 0, interactables.Count - 1);
        }

        private void ApplyCamera()
        {
            CameraRigController rig = LocalUserManager.GetFirstLocalUser()?.cameraRigController;

            if (!rig)
                return;

            Vector3 offset = new Vector3(0, 0.5f, 0);
            Vector3 targetPosition = interactables[currentIndex].GetPosition() + offset;
            Vector3 cameraPosition = targetPosition + Quaternion.Euler(orbitPitch, orbitYaw, 0f) * new Vector3(0f, 0f, -orbitDistance);
            rig.SetCameraState(new CameraState
            {
                position = cameraPosition,
                rotation = Quaternion.LookRotation(targetPosition - cameraPosition),
                fov = rig.currentCameraState.fov
            });
        }

        private void PurchaseInteraction_Start(On.RoR2.PurchaseInteraction.orig_Start orig, PurchaseInteraction self)
        {
            orig(self);

            if (Blacklists.blacklistedNames.Contains(self.displayNameToken))
                return;

            if (Blacklists.droneNames.Contains(self.displayNameToken) && !showDrones.Value)
                return;

            if (self.displayNameToken == Blacklists.newtAltarName && !showNewtAltars.Value)
                return;

            if (self.displayNameToken == Blacklists.lunarPodName && !showLunarPods.Value)
                return;

            if (self.displayNameToken == Blacklists.printerName && !showPrinters.Value)
                return;

            if (self.displayNameToken == Blacklists.droneScrapperName && !showDroneScrappers.Value)
                return;

            if (self.name == "PortalDialer")
                return;

            // Hallowed Concepts stuff
            if ((self.GetGameObject()?.name ?? "").ToUpper().Contains("MIMIC")) // mimics
                return;

            if (Printer3DInteractable.printerNames.Contains(self.displayNameToken))
                interactables.Add(new Printer3DInteractable(self));
            else if (self.displayNameToken == HollowChestInteractable.hollowChestName)
                interactables.Add(new HollowChestInteractable(self));
            else
                interactables.Add(new PurchaseInteractable(self));
        }

        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            orig(self, activator);

            if (!Printer3DInteractable.printerNames.Contains(self.displayNameToken) && self.displayNameToken != HollowChestInteractable.hollowChestName)
                return;

            foreach (Interactable interactable in interactables)
            {
                if (interactable is Printer3DInteractable printer && printer.pi == self)
                    printer.usedPrinter = true;

                if (interactable is HollowChestInteractable hollowChest && hollowChest.pi == self)
                    hollowChest.purchased = true;
            }
        }

        private void ScrapperController_PreStartClient(On.RoR2.ScrapperController.orig_PreStartClient orig, ScrapperController self)
        {
            orig(self);

            interactables.Add(new ScrapperInteractable(self));
        }

        private void ScrapperController_BeginScrapping_UniquePickup(On.RoR2.ScrapperController.orig_BeginScrapping_UniquePickup orig, ScrapperController self, UniquePickup pickupToTake)
        {
            orig(self, pickupToTake);

            didScrap = true;
        }

        private void MultiShopController_Start(On.RoR2.MultiShopController.orig_Start orig, MultiShopController self)
        {
            orig(self);

            interactables.Add(new MultiShopInteractable(self));
        }

        private void DroneVendorMultiShopController_Start(On.RoR2.DroneVendorMultiShopController.orig_Start orig, DroneVendorMultiShopController self)
        {
            orig(self);

            interactables.Add(new DroneMultiShopInteractable(self));
        }

        private void GenericPickupController_Start(On.RoR2.GenericPickupController.orig_Start orig, GenericPickupController self)
        {
            orig(self);

            if (!showItemsOnGround.Value)
                return;

            // Hallowed Concepts secret N'tormat pages
            // has to be done here, because it doesn't have a pickupDef here
            if (self.GetComponent<PagePotentialPicker>() && showSecretNtormatPages.Value)
            {
                interactables.Add(new NtormatPageInteractable(self));
                return;
            }

            PickupDef pickupDef = self.pickup.pickupIndex.pickupDef;

            if (pickupDef == null)
                return;

            if (pickupDef.nameToken == Blacklists.powerPyramidName)
                return;

            if (pickupDef.equipmentIndex != EquipmentIndex.None)
                return;

            if (pickupDef.isLunar)
                return;

            if (self.pickup.isTempItem)
                return;

            ItemTier tier = pickupDef.itemTier;
            if (tier == ItemTier.VoidTier1 || tier == ItemTier.VoidTier2 || tier == ItemTier.VoidTier3)
                return;

            interactables.Add(new GenericPickupControllerInteractable(self));
        }

        private void PickupPickerController_OnEnable(On.RoR2.PickupPickerController.orig_OnEnable orig, PickupPickerController self)
        {
            orig(self);

            if (!showItemsOnGround.Value)
                return;

            if (self.name.Contains("FragmentPotentialPickup") || self.name.Contains("OptionPickup"))
                interactables.Add(new PotentialInteractable(self));
        }

        private void BarrelInteraction_OnEnable(On.RoR2.BarrelInteraction.orig_OnEnable orig, BarrelInteraction self)
        {
            orig(self);

            if (!showBarrels.Value)
                return;

            interactables.Add(new BarrelInteractable(self));
        }

        private void SceneExitController_SetState(On.RoR2.SceneExitController.orig_SetState orig, SceneExitController self, SceneExitController.ExitState newState)
        {
            if (!RoR2Application.isInSinglePlayer)
            {
                orig(self, newState);
                return;
            }

            try
            {
                if (newState == SceneExitController.ExitState.Finished && !releasing)
                {
                    // Skip recap on these stages
                    string stageName = SceneCatalog.currentSceneDef?.baseSceneName;
                    if (stageName == "bazaar" || stageName == "solutionalhaunt" || stageName == "solusweb" || stageName == "computationalexchange" || stageName == "moon2")
                    {
                        orig(self, newState);
                        return;
                    }

                    if (heldExit == null)
                    {
                        interactables.RemoveAll(interactable => !interactable.IsAvailable);

                        if (interactables.Count == 0)
                        {
                            orig(self, newState);
                            return;
                        }

                        heldExit = self;
                        currentIndex = -1;

                        if (showContents.Value)
                        {
                            foreach (Interactable interactable in interactables)
                            {
                                try
                                {
                                    interactable.SimulateDrop();
                                }
                                catch (System.Exception ex)
                                {
                                    Debug.LogError(ex);
                                }
                            }
                        }

                        Time.timeScale = 0f;
                        MoveTo(0);
                    }
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                heldExit = null;
                Time.timeScale = 1.0f;
                orig(self, newState);
                return;
            }

            orig(self, newState);
        }

        private void Resume()
        {
            SceneExitController exit = heldExit;
            heldExit = null;
            Time.timeScale = 1.0f;

            if (exit)
            {
                releasing = true;
                try
                {
                    exit.SetState(SceneExitController.ExitState.Finished);
                }
                finally
                {
                    releasing = false;
                }
            }
        }

        private void Run_OnDestroy(On.RoR2.Run.orig_OnDestroy orig, Run self)
        {
            orig(self);

            EndRecap();
        }

        private static void EndRecap()
        {
            Time.timeScale = 1f;
            heldExit = null;
            releasing = false;
            currentIndex = -1;
            orbitYaw = defaultOrbitYaw;
            orbitPitch = defaultOrbitPitch;
            orbitDistance = defaultOrbitDistance;
            didScrap = false;
            interactables.Clear();
        }

        private void OnActiveSceneChanged(Scene from, Scene to)
        {
            interactables.Clear();
            didScrap = false;
        }
    }
}
