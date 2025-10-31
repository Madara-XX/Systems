using UnityEngine;
using UnityEngine.InputSystem;

namespace RoombaRampage.RoombaCamera
{
    /// <summary>
    /// Example script demonstrating how to use the CameraController.
    /// This script shows common camera operations and can be used as a reference.
    /// Attach to any GameObject to test camera functionality.
    /// </summary>
    public class CameraControllerExample : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the camera controller")]
        [SerializeField] private CameraController cameraController;

        [Header("Example Settings")]
        [Tooltip("Should this example script run?")]
        [SerializeField] private bool enableExample = false;

        [Header("Test Inputs - Using New Input System")]
        [Tooltip("Uses number keys 1-5 for preset switching")]
     
        [Header("Zoom Settings")]
       
        [Range(0.1f, 0.5f)]
        [SerializeField] private float zoomStep = 0.1f;

        private void Awake()
        {
            // Find camera controller if not assigned
            if (cameraController == null)
            {
                cameraController = FindFirstObjectByType<CameraController>();

                if (cameraController == null)
                {
                    Debug.LogError("[CameraControllerExample] No CameraController found in scene!");
                    enabled = false;
                }
            }
        }

        private void Update()
        {
            if (!enableExample || cameraController == null)
                return;

            HandlePresetSwitching();
            HandleZoomControls();
            HandleDebugOutput();
        }

        /// <summary>
        /// Handles preset switching via number keys (1-5)
        /// </summary>
        private void HandlePresetSwitching()
        {
            if (Keyboard.current == null) return;

            // Direct preset switching with number keys
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                cameraController.SetPreset(0);
                Debug.Log("[Camera Example] Switched to Top View");
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                cameraController.SetPreset(1);
                Debug.Log("[Camera Example] Switched to Top View Follow");
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                cameraController.SetPreset(2);
                Debug.Log("[Camera Example] Switched to Third Person");
            }
            else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                cameraController.SetPreset(3);
                Debug.Log("[Camera Example] Switched to Isometric");
            }
            else if (Keyboard.current.digit5Key.wasPressedThisFrame)
            {
                cameraController.SetPreset(4);
                Debug.Log("[Camera Example] Switched to Dynamic");
            }
        }

        /// <summary>
        /// Handles manual zoom controls (+/- and 0 keys)
        /// </summary>
        private void HandleZoomControls()
        {
            if (Keyboard.current == null) return;

            // Zoom in (+ or = key)
            if (Keyboard.current.equalsKey.wasPressedThisFrame)
            {
                float currentZoom = cameraController.GetZoom();
                cameraController.SetZoom(currentZoom - zoomStep);
                Debug.Log($"[Camera Example] Zoom In: {cameraController.GetZoom():F2}");
            }

            // Zoom out (- key)
            if (Keyboard.current.minusKey.wasPressedThisFrame)
            {
                float currentZoom = cameraController.GetZoom();
                cameraController.SetZoom(currentZoom + zoomStep);
                Debug.Log($"[Camera Example] Zoom Out: {cameraController.GetZoom():F2}");
            }

            // Reset zoom (0 key)
            if (Keyboard.current.digit0Key.wasPressedThisFrame)
            {
                cameraController.SetZoom(1f);
                Debug.Log("[Camera Example] Zoom Reset: 1.00");
            }
        }

        /// <summary>
        /// Outputs debug information (press I key)
        /// </summary>
        private void HandleDebugOutput()
        {
            if (Keyboard.current == null) return;

            // Press 'I' for info
            if (Keyboard.current.iKey.wasPressedThisFrame)
            {
                Debug.Log($"[Camera Example] Current Preset: {cameraController.GetCurrentPresetName()} (Index: {cameraController.GetCurrentPresetIndex()})");
                Debug.Log($"[Camera Example] Current Zoom: {cameraController.GetZoom():F2}");
            }
        }

        /// <summary>
        /// Example: Switch camera based on game state
        /// </summary>
        public void OnGameStateChanged(string newState)
        {
            switch (newState)
            {
                case "Menu":
                    cameraController.SetPreset("Isometric");
                    cameraController.SetPresetSwitchingEnabled(false);
                    break;

                case "Gameplay":
                    cameraController.SetPreset("Top View Follow");
                    cameraController.SetPresetSwitchingEnabled(true);
                    break;

                case "BossMode":
                    cameraController.SetPreset("Dynamic");
                    cameraController.SetZoom(1.2f); // Slight zoom out
                    break;

                case "Cutscene":
                    cameraController.SetPreset("Third Person");
                    cameraController.SetPresetSwitchingEnabled(false);
                    cameraController.SetZoomInputEnabled(false);
                    break;

                default:
                    Debug.LogWarning($"[Camera Example] Unknown game state: {newState}");
                    break;
            }
        }

        /// <summary>
        /// Example: Change camera target dynamically
        /// </summary>
        public void OnPlayerRespawned(Transform newPlayerTransform)
        {
            if (cameraController != null && newPlayerTransform != null)
            {
                cameraController.SetPlayer(newPlayerTransform);
                cameraController.SnapToPlayer(); // Instant position update
                Debug.Log("[Camera Example] Camera target updated to new player");
            }
        }

        /// <summary>
        /// Example: Temporarily disable camera controls during cutscene
        /// </summary>
        public void StartCutscene()
        {
            if (cameraController != null)
            {
                cameraController.SetPresetSwitchingEnabled(false);
                cameraController.SetZoomInputEnabled(false);
                Debug.Log("[Camera Example] Camera controls disabled for cutscene");
            }
        }

        /// <summary>
        /// Example: Re-enable camera controls after cutscene
        /// </summary>
        public void EndCutscene()
        {
            if (cameraController != null)
            {
                cameraController.SetPresetSwitchingEnabled(true);
                cameraController.SetZoomInputEnabled(true);
                Debug.Log("[Camera Example] Camera controls re-enabled");
            }
        }

        /// <summary>
        /// Example: Set camera for specific gameplay scenario
        /// </summary>
        public void ConfigureCameraForBulletHell()
        {
            // For bullet hell, we want maximum visibility
            cameraController.SetPreset("Top View");
            cameraController.SetZoom(1.2f); // Slight zoom out to see more
        }

        /// <summary>
        /// Example: Set camera for racing mode
        /// </summary>
        public void ConfigureCameraForRacing()
        {
            // For racing, we want immersive follow
            cameraController.SetPreset("Third Person");
            cameraController.SetZoom(1.0f); // Normal zoom
        }

        private void OnGUI()
        {
            if (!enableExample)
                return;

            // Display controls on screen
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.Label("=== Camera Example Controls ===");
            GUILayout.Label($"Current: {cameraController.GetCurrentPresetName()}");
            GUILayout.Label($"Zoom: {cameraController.GetZoom():F2}");
            GUILayout.Space(10);
            GUILayout.Label("Number Keys 1-5: Switch presets");
            GUILayout.Label("C: Cycle presets");
            GUILayout.Label("Mouse Scroll: Zoom");
            GUILayout.Label("+/-: Manual zoom");
            GUILayout.Label("0: Reset zoom");
            GUILayout.Label("I: Print info");
            GUILayout.EndArea();
        }
    }
}
