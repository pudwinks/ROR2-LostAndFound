using BepInEx;
using BepInEx.Configuration;
using Rewired;
using RoR2;
using UnityEngine;

namespace src
{
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class LostAndFound : BaseUnityPlugin
    {
        public abstract class Interactable
        {
            public Vector3 position;
        }

        public class PurchaseInteractable : Interactable
        {
            public PurchaseInteraction pi;

            public PurchaseInteractable(Vector3 position, PurchaseInteraction pi)
            {
                this.position = position;
                this.pi = pi;
            }
        }

        public class GenericPickupControllerInteractable : Interactable
        {
            public GenericPickupController gpc;

            public GenericPickupControllerInteractable(Vector3 position, GenericPickupController gpc)
            {
                this.position = position;
                this.gpc = gpc;
            }
        }
        public class ScrapperInteractable : Interactable
        {
            public ScrapperController sc;

            public ScrapperInteractable(Vector3 position, ScrapperController sc)
            {
                this.position = position;
                this.sc = sc;
            }
        }
        public class PotentialInteractable : Interactable
        {
            public PickupPickerController ppc;

            public PotentialInteractable(Vector3 position, PickupPickerController ppc)
            {
                this.position = position;
                this.ppc = ppc;
            }
        }
        public class BarrelInteractable : Interactable
        {
            public BarrelInteraction bi;

            public BarrelInteractable(Vector3 position, BarrelInteraction bi)
            {
                this.position = position;
                this.bi = bi;
            }
        }

        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Pudwinks";
        public const string PluginName = "LostAndFound";
        public const string PluginVersion = "1.0.0";

        public static CameraRigController camera;

        public static bool recapRunning;
        int index = -1;

        public ListWithEvents<Interactable> interactables = new();
        public ListWithEvents<Interactable> remainingInteractables = new();

        ConfigEntry<bool> cnfgShowNewtAltars;
        ConfigEntry<bool> cnfgShowBarrels;
        ConfigEntry<bool> cnfgShowItemsOnGround;
        ConfigEntry<bool> cnfgShowDrones;
        ConfigEntry<bool> cnfgShow3DPrinters;

        ConfigEntry<KeyboardShortcut> kbGoNext;
        ConfigEntry<KeyboardShortcut> kbGoPrevious;
        ConfigEntry<KeyboardShortcut> kbFinishRecap;
        public static ConfigEntry<KeyboardShortcut> kbZoomIn;
        public static ConfigEntry<KeyboardShortcut> kbZoomOut;

        private void InitializeRiskOfOptions()
        {
            {
                // Show newt altars
                ConfigDescription cdesc = new ConfigDescription("Set to true if you want to see newt altars in the recap. Default is false to avoid potential spoilers.");
                ConfigDefinition cdef = new ConfigDefinition("General", "Show newt altars");
                cnfgShowNewtAltars = Config.Bind(cdef, false, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(cnfgShowNewtAltars);
            }
            {
                // Show 3D printers
                ConfigDescription cdesc = new ConfigDescription("Set to true if you want to see white and green 3D printers in the recap. Setting this to false will only hide white and green printers, since they are the most common. Red and boss printers remain visible regardless of this setting.");
                ConfigDefinition cdef = new ConfigDefinition("General", "Show 3D printers");
                cnfgShow3DPrinters = Config.Bind(cdef, true, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(cnfgShow3DPrinters);
            }

            {
                // Show drones
                ConfigDescription cdesc = new ConfigDescription("Set to true if you want to see drones in the recap. TC-280 drone always shows up regardless of this setting. Because it's a cool drone.");
                ConfigDefinition cdef = new ConfigDefinition("General", "Show drones");
                cnfgShowDrones = Config.Bind(cdef, true, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(cnfgShowDrones);
            }

            {
                // Show barrels
                ConfigDescription cdesc = new ConfigDescription("Set to true if you want to see barrels in the recap.");
                ConfigDefinition cdef = new ConfigDefinition("General", "Show barrels");
                cnfgShowBarrels = Config.Bind(cdef, false, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(cnfgShowBarrels);
            }

            {
                // Show items on ground
                ConfigDescription cdesc = new ConfigDescription("Set to true if you want to see items on the ground in the recap. If you forgot to pick something up, it'll show up.");
                ConfigDefinition cdef = new ConfigDefinition("General", "Show items on ground");
                cnfgShowItemsOnGround = Config.Bind(cdef, true, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(cnfgShowItemsOnGround);
            }

            // Keybinds

            {
                // Go next keybind
                ConfigDescription cdesc = new ConfigDescription("Which key to use to show the next interactable in the recap.");
                ConfigDefinition cdef = new ConfigDefinition("Keybinds", "Next interactable");
                KeyboardShortcut defaultKey = new KeyboardShortcut(KeyCode.Mouse1);
                kbGoNext = Config.Bind(cdef, defaultKey, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(kbGoNext);
            }

            {
                // Go previous keybind
                ConfigDescription cdesc = new ConfigDescription("Which key to use to show the previous interactable in the recap.");
                ConfigDefinition cdef = new ConfigDefinition("Keybinds", "Previous interactable");
                KeyboardShortcut defaultKey = new KeyboardShortcut(KeyCode.Mouse2);
                kbGoPrevious = Config.Bind(cdef, defaultKey, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(kbGoPrevious);
            }

            {
                // Zoom out keybind
                ConfigDescription cdesc = new ConfigDescription("Which key to use to zoom out the camera in the recap.");
                ConfigDefinition cdef = new ConfigDefinition("Keybinds", "Zoom out");
                KeyboardShortcut defaultKey = new KeyboardShortcut(KeyCode.A);
                kbZoomOut = Config.Bind(cdef, defaultKey, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(kbZoomOut);
            }

            {
                // Zoom in keybind
                ConfigDescription cdesc = new ConfigDescription("Which key to use to zoom in the camera in the recap.");
                ConfigDefinition cdef = new ConfigDefinition("Keybinds", "Zoom in");
                KeyboardShortcut defaultKey = new KeyboardShortcut(KeyCode.D);
                kbZoomIn = Config.Bind(cdef, defaultKey, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(kbZoomIn);
            }

            {
                // Finish recap keybind
                ConfigDescription cdesc = new ConfigDescription("Which key to use to finish the recap once it's started.");
                ConfigDefinition cdef = new ConfigDefinition("Keybinds", "Finish recap");
                KeyboardShortcut defaultKey = new KeyboardShortcut(KeyCode.W);
                kbFinishRecap = Config.Bind(cdef, defaultKey, cdesc);

                if (RiskOfOptionsStuff.enabled)
                    RiskOfOptionsStuff.AddOption(kbFinishRecap);
            }

        }

        public void Awake()
        {
            InitializeRiskOfOptions();

            On.RoR2.BarrelInteraction.OnEnable += (e, a) =>
            {
                e(a);

                if (!cnfgShowBarrels.Value)
                    return;

                BarrelInteractable bi = new BarrelInteractable(a.transform.position, a);

                interactables.Add(bi);

            };

            On.RoR2.ScrapperController.Start += (e, a) =>
            {
                e(a);

                ScrapperInteractable si = new ScrapperInteractable(a.transform.position, a);

                interactables.Add(si);
            };

            On.RoR2.PurchaseInteraction.Start += (e, a) =>
            {
                e(a);

                if (!cnfgShow3DPrinters.Value) // Will still show red and boss printers, since they are more rare.
                {
                    if (a.displayNameToken == "DUPLICATOR_NAME")
                        return;
                }

                if (a.displayNameToken == "SHRINE_COMBAT_NAME")
                    return;
                if (a.displayNameToken == "SHRINE_BLOOD_NAME")
                    return;
                if (a.displayNameToken == "SHRINE_HEALING_NAME")
                    return;

                if (a.displayNameToken == "FAN_NAME")
                    return;

                if (a.displayNameToken == "NEWT_STATUE_NAME" && !cnfgShowNewtAltars.Value)
                    return;

                if (!cnfgShowDrones.Value) // Will still show TC-280 because it's cool.
                {
                    if (a.displayNameToken == "DRONE_GUNNER_INTERACTABLE_NAME")
                        return;
                    if (a.displayNameToken == "DRONE_HEALING_INTERACTABLE_NAME")
                        return;
                    if (a.displayNameToken == "DRONE_MISSILE_INTERACTABLE_NAME")
                        return;
                    if (a.displayNameToken == "EQUIPMENTDRONE_INTERACTABLE_NAME")
                        return;
                    if (a.displayNameToken == "FLAMEDRONE_INTERACTABLE_NAME")
                        return;
                    if (a.displayNameToken == "EMERGENCYDRONE_INTERACTABLE_NAME")
                        return;
                    if (a.displayNameToken == "TURRET1_INTERACTABLE_NAME")
                        return;
                }

                PurchaseInteractable pi = new PurchaseInteractable(a.transform.position, a);

                interactables.Add(pi);
            };


            On.RoR2.PickupPickerController.OnEnable += (e, a) =>
            {
                e(a);

                if (!cnfgShowItemsOnGround.Value)
                    return;

                if (a.name.Contains("FragmentPotentialPickup") || a.name.Contains("OptionPickup"))
                {
                    PotentialInteractable pi = new PotentialInteractable(a.transform.position, a);

                    interactables.Add(pi);
                }
            };

            On.RoR2.GenericPickupController.Start += (e, a) =>
            {
                e(a);

                if (!cnfgShowItemsOnGround.Value)
                    return;

                GenericPickupControllerInteractable gpci = new GenericPickupControllerInteractable(a.transform.position, a);

                interactables.Add(gpci);
            };

            On.RoR2.Run.OnDestroy += (e, a) =>
            {
                e(a);

                EndMissingInteractableFinder();
            };

            On.RoR2.CameraRigController.OnEnable += (e, a) =>
            {
                e(a);

                camera = a;
            };

            On.RoR2.SceneExitController.Begin += (e, a) =>
            {
                e(a);

                if (!RoR2Application.isInSinglePlayer)
                    return;

                if (SceneCatalog.currentSceneDef.baseSceneName != "bazaar")
                    StartMissingInteractableFinder();
            };
        }

        private void EndMissingInteractableFinder()
        {
            Time.timeScale = 1f;
            recapRunning = false;
            cama.End();
            ClearLists();
        }

        private void StartMissingInteractableFinder()
        {
            GetRemainingInteractables();

            if (remainingInteractables.Count == 0)
                return;

            Time.timeScale = 0f;
            StartCamera();
            recapRunning = true;
        }

        private void ClearLists()
        {
            interactables.Clear();
            remainingInteractables.Clear();
        }

        private void GetRemainingInteractables()
        {
            remainingInteractables.Clear();

            for (int i = 0; i < interactables.Count; i++)
            {
                switch (interactables[i])
                {
                    case PurchaseInteractable pi:
                        if (pi.pi == null)
                            break;
                        if (pi.pi.available)
                            remainingInteractables.Add(interactables[i]);
                        break;

                    case ScrapperInteractable si:
                        if (si.sc == null)
                            break;
                        remainingInteractables.Add(interactables[i]);
                        break;

                    case GenericPickupControllerInteractable gpci:
                        if (gpci.gpc == null)
                            break;
                        if (!gpci.gpc.consumed)
                            remainingInteractables.Add(interactables[i]);
                        break;

                    case PotentialInteractable pi2:
                        if (pi2.ppc == null)
                            break;
                        remainingInteractables.Add(interactables[i]);
                        break;

                    case BarrelInteractable bi:
                        if (bi.bi == null)
                            break;
                        if (!bi.bi.opened)
                            remainingInteractables.Add(interactables[i]);
                        break;
                }
            }
        }

        private void IncrementCameraPosition()
        {
            index++;

            if (remainingInteractables.Count == 0)
                return;

            if (camera == null)
            {
                Logger.LogDebug("Camera was null.");
                return;
            }

            if (index < 0)
                index = 0;

            if (index >= remainingInteractables.Count)
                index = remainingInteractables.Count - 1;

            cama.SetCamPosition(remainingInteractables[index].position + new Vector3(0, 0.5f, 0));
        }
        private void DecrementCameraPosition()
        {
            index--;

            if (remainingInteractables.Count == 0)
                return;

            if (camera == null)
            {
                Logger.LogDebug("Camera was null.");
                return;
            }

            if (index < 0)
                index = 0;

            if (index >= remainingInteractables.Count)
                index = remainingInteractables.Count - 1;

            cama.SetCamPosition(remainingInteractables[index].position + new Vector3(0, 0.5f, 0));
        }


        private void StartCamera()
        {
            if (cam != null)
                Destroy(cam);

            cam = new GameObject("MissingInteractables_Camera");
            cama = cam.AddComponent<Camera>();

            cama.Start();

            index = -1;

            IncrementCameraPosition();
        }

        Camera cama;
        GameObject cam;

        private void Update()
        {
            if (!recapRunning || RoR2.PauseManager.isPaused)
                return;

            if (Input.GetKeyDown(kbGoNext.Value.MainKey))
            {
                IncrementCameraPosition();
            }

            if (Input.GetKeyDown(kbGoPrevious.Value.MainKey))
            {
                DecrementCameraPosition();
            }

            if (Input.GetKeyDown(kbFinishRecap.Value.MainKey))
            {
                EndMissingInteractableFinder();
            }
        }
    }

    public class Camera : MonoBehaviour, ICameraStateProvider
    {
        CameraRigController cam => LostAndFound.camera;

        internal class PhotoModeCameraState
        {
            internal float pitch;

            internal float yaw;

            internal float roll;

            internal Vector3 position;

            public Vector3 target;
            public float distance = 16;

            internal float fov;

            internal Quaternion Rotation
            {
                get
                {
                    return Quaternion.Euler(pitch, yaw, roll);
                }
                set
                {
                    Vector3 eulerAngles = value.eulerAngles;
                    pitch = eulerAngles.x;
                    yaw = eulerAngles.y;
                    roll = eulerAngles.z;
                }
            }
        }

        private PhotoModeCameraState cameraState = new();

        public void SetCamPosition(Vector3 pos)
        {
            cameraState.target = pos;
        }

        public void Start()
        {
            cam.SetOverrideCam(this, 0f);

            cameraState.fov = cam.baseFov;
        }

        public void End()
        {
            cam.SetOverrideCam(null, 0f);
        }

        private void Update()
        {
            if (!LostAndFound.recapRunning || RoR2.PauseManager.isPaused)
                return;

            UserProfile profile = cam.localUserViewer.userProfile;
            Player inputPlayer = cam.localUserViewer.inputPlayer;

            float axis = inputPlayer.GetAxis(23);
            float axis2 = inputPlayer.GetAxis(24);

            float mouseLookSensitivity = profile.mouseLookSensitivity;
            float mouseLookScaleX = profile.mouseLookScaleX;
            float mouseLookScaleY = profile.mouseLookScaleY;

            float value = mouseLookScaleX * mouseLookSensitivity * Time.unscaledDeltaTime * axis;
            float value2 = mouseLookScaleY * mouseLookSensitivity * Time.unscaledDeltaTime * axis2;

            float num2 = cameraState.roll * (3.1415926f / 180f);
            cameraState.yaw += cameraState.fov * (value * Mathf.Cos(num2) - value2 * Mathf.Sin(num2));
            cameraState.pitch += cameraState.fov * ((0f - value2) * Mathf.Cos(num2) - value * Mathf.Sin(num2));
            cameraState.pitch = Mathf.Clamp(cameraState.pitch, -89f, 89f);

            cameraState.position = cameraState.target - (cameraState.Rotation * Vector3.forward * cameraState.distance);

            cam.transform.position = cameraState.position;
            cam.transform.rotation = cameraState.Rotation;

            if (Input.GetKey(LostAndFound.kbZoomOut.Value.MainKey))
            {
                cameraState.distance += 35 * Time.unscaledDeltaTime;
            }

            if (Input.GetKey(LostAndFound.kbZoomIn.Value.MainKey))
            {
                cameraState.distance -= 35 * Time.unscaledDeltaTime;

                if (cameraState.distance <= 1)
                    cameraState.distance = 1;
            }


        }

        public void GetCameraState(CameraRigController cameraRigController, ref CameraState _cameraState)
        {
        }

        public bool IsHudAllowed(CameraRigController cameraRigController)
        {
            return false;
        }

        public bool IsUserControlAllowed(CameraRigController cameraRigController)
        {
            return false;
        }

        public bool IsUserLookAllowed(CameraRigController cameraRigController)
        {
            return false;
        }
    }

}
