using UnityEngine;

namespace RoombaRampage.RoombaCamera
{
    /// <summary>
    /// ScriptableObject that holds all camera preset configurations.
    /// Create via: Assets > Create > RoombaRampage > Camera Settings
    /// </summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "RoombaRampage/Camera Settings", order = 1)]
    public class CameraSettings : ScriptableObject
    {
        [Header("Camera Presets")]
        [Tooltip("All available camera presets")]
        public CameraPreset[] presets = new CameraPreset[5];

        [Header("Default Settings")]
        [Tooltip("Which preset to use on startup (index into presets array)")]
        [Range(0, 4)]
        public int defaultPresetIndex = 0;

        [Tooltip("How quickly to transition between presets")]
        [Range(0.1f, 5f)]
        public float transitionSpeed = 1f;

        [Header("Input Settings")]
        [Tooltip("Mouse scroll sensitivity for zooming")]
        [Range(0.1f, 2f)]
        public float scrollSensitivity = 1f;

        [Tooltip("Key to cycle through camera presets")]
        public KeyCode cycleCameraKey = KeyCode.C;

        [Header("Debug Settings")]
        [Tooltip("Show debug gizmos in Scene view")]
        public bool showDebugGizmos = true;

        [Tooltip("Color for debug gizmos")]
        public Color gizmoColor = Color.cyan;

        private void OnValidate()
        {
            // Ensure we have the right number of presets
            if (presets == null || presets.Length != 5)
            {
                System.Array.Resize(ref presets, 5);
            }

            // Initialize default presets if they're null
            InitializeDefaultPresets();
        }

        /// <summary>
        /// Initializes default presets with sensible values
        /// </summary>
        private void InitializeDefaultPresets()
        {
            // Top View
            if (presets[0] == null || presets[0].presetName == "Default")
            {
                presets[0] = new CameraPreset
                {
                    presetName = "Top View",
                    description = "Straight down view from above",
                    offset = new Vector3(0f, 20f, 0f),
                    rotation = new Vector3(90f, 0f, 0f),
                    followPlayerRotation = false,
                    positionFollowSpeed = 8f,
                    isOrthographic = false,
                    fieldOfView = 60f,
                    allowZoom = true,
                    minZoom = 0.5f,
                    maxZoom = 2f,
                    zoomSpeed = 10f
                };
            }

            // Top View Follow
            if (presets[1] == null || presets[1].presetName == "Default")
            {
                presets[1] = new CameraPreset
                {
                    presetName = "Top View Follow",
                    description = "Top view with slight angle for depth perception",
                    offset = new Vector3(0f, 18f, -3f),
                    rotation = new Vector3(75f, 0f, 0f),
                    followPlayerRotation = false,
                    positionFollowSpeed = 8f,
                    isOrthographic = false,
                    fieldOfView = 60f,
                    allowZoom = true,
                    minZoom = 0.5f,
                    maxZoom = 2f,
                    zoomSpeed = 10f
                };
            }

            // Third Person
            if (presets[2] == null || presets[2].presetName == "Default")
            {
                presets[2] = new CameraPreset
                {
                    presetName = "Third Person",
                    description = "Behind the player, angled down",
                    offset = new Vector3(0f, 8f, -12f),
                    rotation = new Vector3(35f, 0f, 0f),
                    followPlayerRotation = true,
                    positionFollowSpeed = 6f,
                    rotationFollowSpeed = 8f,
                    isOrthographic = false,
                    fieldOfView = 70f,
                    allowZoom = true,
                    minZoom = 0.7f,
                    maxZoom = 1.5f,
                    zoomSpeed = 8f
                };
            }

            // Isometric
            if (presets[3] == null || presets[3].presetName == "Default")
            {
                presets[3] = new CameraPreset
                {
                    presetName = "Isometric",
                    description = "Classic 45° isometric view",
                    offset = new Vector3(10f, 10f, -10f),
                    rotation = new Vector3(45f, 45f, 0f),
                    followPlayerRotation = false,
                    positionFollowSpeed = 7f,
                    isOrthographic = true,
                    orthographicSize = 10f,
                    allowZoom = true,
                    minZoom = 0.6f,
                    maxZoom = 1.8f,
                    zoomSpeed = 12f
                };
            }

            // Dynamic
            if (presets[4] == null || presets[4].presetName == "Default")
            {
                presets[4] = new CameraPreset
                {
                    presetName = "Dynamic",
                    description = "Action camera that follows player rotation",
                    offset = new Vector3(0f, 12f, -8f),
                    rotation = new Vector3(55f, 0f, 0f),
                    followPlayerRotation = true,
                    positionFollowSpeed = 10f,
                    rotationFollowSpeed = 12f,
                    useSmoothDamping = true,
                    isOrthographic = false,
                    fieldOfView = 75f,
                    allowZoom = true,
                    minZoom = 0.5f,
                    maxZoom = 2f,
                    zoomSpeed = 10f
                };
            }
        }

        /// <summary>
        /// Gets a preset by index with bounds checking
        /// </summary>
        public CameraPreset GetPreset(int index)
        {
            if (presets == null || index < 0 || index >= presets.Length)
            {
                Debug.LogWarning($"Invalid preset index: {index}. Returning default preset.");
                return presets[defaultPresetIndex];
            }

            return presets[index];
        }

        /// <summary>
        /// Gets a preset by name
        /// </summary>
        public CameraPreset GetPresetByName(string name)
        {
            foreach (var preset in presets)
            {
                if (preset != null && preset.presetName == name)
                {
                    return preset;
                }
            }

            Debug.LogWarning($"Preset with name '{name}' not found. Returning default preset.");
            return presets[defaultPresetIndex];
        }

        /// <summary>
        /// Gets the number of available presets
        /// </summary>
        public int GetPresetCount()
        {
            return presets != null ? presets.Length : 0;
        }
    }
}
