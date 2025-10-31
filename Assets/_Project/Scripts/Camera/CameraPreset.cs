using UnityEngine;

namespace RoombaRampage.RoombaCamera
{
    /// <summary>
    /// Defines a camera configuration preset for different viewing modes.
    /// Used by CameraSettings ScriptableObject to store preset configurations.
    /// </summary>
    [System.Serializable]
    public class CameraPreset
    {
        [Header("Preset Info")]
        [Tooltip("Name of this camera preset (e.g., 'Top View', 'Third Person')")]
        public string presetName = "Default";

        [Tooltip("Description of this camera mode")]
        [TextArea(2, 3)]
        public string description = "Camera preset description";

        [Header("Position Settings")]
        [Tooltip("Offset from the player position")]
        public Vector3 offset = new Vector3(0f, 10f, -5f);

        [Tooltip("Should the camera follow player rotation?")]
        public bool followPlayerRotation = false;

        [Tooltip("Additional rotation to apply to the camera (Euler angles)")]
        public Vector3 rotation = new Vector3(45f, 0f, 0f);

        [Tooltip("Offset for the LookAt target relative to player")]
        public Vector3 lookAtOffset = Vector3.zero;

        [Header("Follow Settings")]
        [Tooltip("How quickly the camera follows position (higher = faster)")]
        [Range(0.1f, 50f)]
        public float positionFollowSpeed = 5f;

        [Tooltip("How quickly the camera rotates (higher = faster)")]
        [Range(0.1f, 50f)]
        public float rotationFollowSpeed = 5f;

        [Tooltip("Use smooth damping instead of lerp for more natural movement")]
        public bool useSmoothDamping = true;

        [Header("Camera Settings")]
        [Tooltip("Is this camera orthographic or perspective?")]
        public bool isOrthographic = false;

        [Tooltip("Field of view for perspective camera")]
        [Range(20f, 120f)]
        public float fieldOfView = 60f;

        [Tooltip("Orthographic size (height in units)")]
        [Range(1f, 50f)]
        public float orthographicSize = 10f;

        [Header("Zoom Settings")]
        [Tooltip("Can the player zoom this camera?")]
        public bool allowZoom = true;

        [Tooltip("Minimum zoom distance multiplier")]
        [Range(0.1f, 1f)]
        public float minZoom = 0.5f;

        [Tooltip("Maximum zoom distance multiplier")]
        [Range(1f, 3f)]
        public float maxZoom = 2f;

        [Tooltip("How fast the zoom responds")]
        [Range(1f, 20f)]
        public float zoomSpeed = 10f;

        [Header("Boundary Settings")]
        [Tooltip("Should the camera be constrained to arena boundaries?")]
        public bool useBoundaries = false;

        [Tooltip("Minimum XZ position for camera")]
        public Vector2 minBoundary = new Vector2(-50f, -50f);

        [Tooltip("Maximum XZ position for camera")]
        public Vector2 maxBoundary = new Vector2(50f, 50f);

        /// <summary>
        /// Creates a deep copy of this preset
        /// </summary>
        public CameraPreset Clone()
        {
            return new CameraPreset
            {
                presetName = presetName,
                description = description,
                offset = offset,
                followPlayerRotation = followPlayerRotation,
                rotation = rotation,
                lookAtOffset = lookAtOffset,
                positionFollowSpeed = positionFollowSpeed,
                rotationFollowSpeed = rotationFollowSpeed,
                useSmoothDamping = useSmoothDamping,
                isOrthographic = isOrthographic,
                fieldOfView = fieldOfView,
                orthographicSize = orthographicSize,
                allowZoom = allowZoom,
                minZoom = minZoom,
                maxZoom = maxZoom,
                zoomSpeed = zoomSpeed,
                useBoundaries = useBoundaries,
                minBoundary = minBoundary,
                maxBoundary = maxBoundary
            };
        }
    }
}
