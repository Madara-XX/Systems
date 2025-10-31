using UnityEngine;
using UnityEngine.InputSystem;

namespace RoombaRampage.RoombaCamera
{
    /// <summary>
    /// Main camera controller for RoombaRampage.
    /// Handles smooth camera following, multiple view modes, and zoom functionality.
    /// Attach to your Main Camera GameObject.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The player transform to follow")]
        [SerializeField] private Transform playerTransform;

        [Tooltip("Camera settings ScriptableObject with presets")]
        [SerializeField] private CameraSettings settings;

        [Header("Runtime Settings")]
        [Tooltip("Current camera preset index")]
        [SerializeField] private int currentPresetIndex = 0;

        [Tooltip("Allow runtime preset switching via input")]
        [SerializeField] private bool allowPresetSwitching = true;

        [Tooltip("Allow zoom input")]
        [SerializeField] private bool allowZoomInput = true;

        // Cached components
        private UnityEngine.Camera cameraComponent;

        // Current state
        private CameraPreset currentPreset;
        private CameraPreset targetPreset;
        private float transitionProgress = 1f;

        // Position tracking
        private Vector3 targetPosition;
        private Vector3 positionVelocity;

        // Rotation tracking
        private Quaternion targetRotation;
        private Quaternion rotationVelocity;

        // Zoom tracking
        private float currentZoom = 1f;
        private float zoomVelocity = 0f;

        // Debug
        private bool showDebug = true;

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache components
            cameraComponent = GetComponent<UnityEngine.Camera>();

            // Validate references
            if (playerTransform == null)
            {
                Debug.LogError("[CameraController] Player transform not assigned! Camera will not function.");
                enabled = false;
                return;
            }

            if (settings == null)
            {
                Debug.LogError("[CameraController] Camera settings not assigned! Camera will not function.");
                enabled = false;
                return;
            }

            // Initialize camera with default preset
            InitializeCamera();
        }

        private void LateUpdate()
        {
            if (playerTransform == null || settings == null)
                return;

            // Handle input
            HandleInput();

            // Update transition between presets
            UpdatePresetTransition();

            // Calculate and apply camera transform
            UpdateCameraTransform();

            // Apply camera settings
            ApplyCameraSettings();
        }

        private void OnDrawGizmos()
        {
            if (!showDebug || settings == null || !settings.showDebugGizmos)
                return;

            if (playerTransform != null && currentPreset != null)
            {
                // Draw target position
                Gizmos.color = settings.gizmoColor;
                Gizmos.DrawWireSphere(targetPosition, 0.5f);

                // Draw line from camera to target
                Gizmos.DrawLine(transform.position, targetPosition);

                // Draw look-at target
                Vector3 lookAtTarget = playerTransform.position + currentPreset.lookAtOffset;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(lookAtTarget, 0.3f);

                // Draw boundaries if enabled
                if (currentPreset.useBoundaries)
                {
                    Gizmos.color = Color.red;
                    Vector3 min = new Vector3(currentPreset.minBoundary.x, playerTransform.position.y, currentPreset.minBoundary.y);
                    Vector3 max = new Vector3(currentPreset.maxBoundary.x, playerTransform.position.y, currentPreset.maxBoundary.y);

                    // Draw boundary box
                    Vector3 size = max - min;
                    Gizmos.DrawWireCube(min + size * 0.5f, new Vector3(size.x, 0.1f, size.z));
                }
            }
        }

        #endregion

        #region Initialization

        private void InitializeCamera()
        {
            // Load default preset
            currentPresetIndex = Mathf.Clamp(settings.defaultPresetIndex, 0, settings.GetPresetCount() - 1);
            currentPreset = settings.GetPreset(currentPresetIndex).Clone();
            targetPreset = currentPreset.Clone();
            transitionProgress = 1f;

            // Initialize zoom
            currentZoom = 1f;

            // Set initial position and rotation
            UpdateCameraTransform();
            ApplyCameraSettings();

            // Force immediate positioning
            transform.position = targetPosition;
            transform.rotation = targetRotation;

            Debug.Log($"[CameraController] Initialized with preset: {currentPreset.presetName}");
        }

        #endregion

        #region Input Handling

        private void HandleInput()
        {
            // Cycle camera presets
            if (allowPresetSwitching && Keyboard.current != null)
            {
                if (Keyboard.current.cKey.wasPressedThisFrame)
                {
                    CycleToNextPreset();
                }
            }

            // Handle zoom input
            if (allowZoomInput && currentPreset.allowZoom && Mouse.current != null)
            {
                float scrollInput = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scrollInput) > 0.01f)
                {
                    float zoomChange = scrollInput * settings.scrollSensitivity * 0.01f;
                    currentZoom = Mathf.Clamp(currentZoom - zoomChange, currentPreset.minZoom, currentPreset.maxZoom);
                }
            }
        }

        #endregion

        #region Preset Management

        /// <summary>
        /// Cycles to the next camera preset
        /// </summary>
        public void CycleToNextPreset()
        {
            int nextIndex = (currentPresetIndex + 1) % settings.GetPresetCount();
            SetPreset(nextIndex);
        }

        /// <summary>
        /// Sets the camera to a specific preset by index
        /// </summary>
        public void SetPreset(int presetIndex)
        {
            if (presetIndex < 0 || presetIndex >= settings.GetPresetCount())
            {
                Debug.LogWarning($"[CameraController] Invalid preset index: {presetIndex}");
                return;
            }

            currentPresetIndex = presetIndex;
            targetPreset = settings.GetPreset(presetIndex).Clone();
            transitionProgress = 0f;

            // Reset zoom for new preset
            currentZoom = 1f;

            Debug.Log($"[CameraController] Switching to preset: {targetPreset.presetName}");
        }

        /// <summary>
        /// Sets the camera to a specific preset by name
        /// </summary>
        public void SetPreset(string presetName)
        {
            for (int i = 0; i < settings.GetPresetCount(); i++)
            {
                CameraPreset preset = settings.GetPreset(i);
                if (preset.presetName == presetName)
                {
                    SetPreset(i);
                    return;
                }
            }

            Debug.LogWarning($"[CameraController] Preset '{presetName}' not found.");
        }

        /// <summary>
        /// Gets the current preset index
        /// </summary>
        public int GetCurrentPresetIndex()
        {
            return currentPresetIndex;
        }

        /// <summary>
        /// Gets the current preset name
        /// </summary>
        public string GetCurrentPresetName()
        {
            return currentPreset != null ? currentPreset.presetName : "None";
        }

        #endregion

        #region Preset Transition

        private void UpdatePresetTransition()
        {
            if (transitionProgress >= 1f)
                return;

            // Update transition progress
            transitionProgress += Time.deltaTime * settings.transitionSpeed;
            transitionProgress = Mathf.Clamp01(transitionProgress);

            // Smoothly interpolate all preset values
            float t = Mathf.SmoothStep(0f, 1f, transitionProgress);

            currentPreset.offset = Vector3.Lerp(currentPreset.offset, targetPreset.offset, t);
            currentPreset.rotation = Vector3.Lerp(currentPreset.rotation, targetPreset.rotation, t);
            currentPreset.lookAtOffset = Vector3.Lerp(currentPreset.lookAtOffset, targetPreset.lookAtOffset, t);
            currentPreset.positionFollowSpeed = Mathf.Lerp(currentPreset.positionFollowSpeed, targetPreset.positionFollowSpeed, t);
            currentPreset.rotationFollowSpeed = Mathf.Lerp(currentPreset.rotationFollowSpeed, targetPreset.rotationFollowSpeed, t);
            currentPreset.fieldOfView = Mathf.Lerp(currentPreset.fieldOfView, targetPreset.fieldOfView, t);
            currentPreset.orthographicSize = Mathf.Lerp(currentPreset.orthographicSize, targetPreset.orthographicSize, t);

            // Boolean values switch at 50% transition
            if (t >= 0.5f)
            {
                currentPreset.followPlayerRotation = targetPreset.followPlayerRotation;
                currentPreset.isOrthographic = targetPreset.isOrthographic;
                currentPreset.useSmoothDamping = targetPreset.useSmoothDamping;
                currentPreset.allowZoom = targetPreset.allowZoom;
            }
        }

        #endregion

        #region Camera Transform Update

        private void UpdateCameraTransform()
        {
            // Calculate target position with zoom
            Vector3 zoomedOffset = currentPreset.offset * currentZoom;
            Vector3 desiredPosition;

            if (currentPreset.followPlayerRotation)
            {
                // Rotate offset by player's rotation
                desiredPosition = playerTransform.position + playerTransform.TransformDirection(zoomedOffset);
            }
            else
            {
                // Fixed offset
                desiredPosition = playerTransform.position + zoomedOffset;
            }

            // Apply boundary constraints
            if (currentPreset.useBoundaries)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, currentPreset.minBoundary.x, currentPreset.maxBoundary.x);
                desiredPosition.z = Mathf.Clamp(desiredPosition.z, currentPreset.minBoundary.y, currentPreset.maxBoundary.y);
            }

            // Smooth position following
            if (currentPreset.useSmoothDamping)
            {
                targetPosition = Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref positionVelocity,
                    1f / currentPreset.positionFollowSpeed
                );
            }
            else
            {
                targetPosition = Vector3.Lerp(
                    transform.position,
                    desiredPosition,
                    Time.deltaTime * currentPreset.positionFollowSpeed
                );
            }

            // Apply position
            transform.position = targetPosition;

            // Calculate target rotation
            Quaternion desiredRotation;

            if (currentPreset.followPlayerRotation)
            {
                // Combine player rotation with camera rotation offset
                Quaternion playerRotation = Quaternion.Euler(0f, playerTransform.eulerAngles.y, 0f);
                Quaternion offsetRotation = Quaternion.Euler(currentPreset.rotation);
                desiredRotation = playerRotation * offsetRotation;
            }
            else
            {
                // Fixed rotation
                desiredRotation = Quaternion.Euler(currentPreset.rotation);
            }

            // Smooth rotation following
            targetRotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                Time.deltaTime * currentPreset.rotationFollowSpeed
            );

            // Apply rotation (with look-at override if needed)
            if (currentPreset.lookAtOffset.sqrMagnitude > 0.01f)
            {
                Vector3 lookAtTarget = playerTransform.position + currentPreset.lookAtOffset;
                transform.LookAt(lookAtTarget);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }

        #endregion

        #region Camera Settings

        private void ApplyCameraSettings()
        {
            if (cameraComponent == null)
                return;

            // Set orthographic or perspective
            cameraComponent.orthographic = currentPreset.isOrthographic;

            if (currentPreset.isOrthographic)
            {
                // Apply orthographic size with zoom
                cameraComponent.orthographicSize = currentPreset.orthographicSize * currentZoom;
            }
            else
            {
                // Apply field of view
                cameraComponent.fieldOfView = currentPreset.fieldOfView;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets a new player target to follow
        /// </summary>
        public void SetPlayer(Transform newPlayer)
        {
            playerTransform = newPlayer;
            Debug.Log($"[CameraController] Player target set to: {newPlayer.name}");
        }

        /// <summary>
        /// Gets the current zoom level
        /// </summary>
        public float GetZoom()
        {
            return currentZoom;
        }

        /// <summary>
        /// Sets the zoom level directly
        /// </summary>
        public void SetZoom(float zoom)
        {
            currentZoom = Mathf.Clamp(zoom, currentPreset.minZoom, currentPreset.maxZoom);
        }

        /// <summary>
        /// Enables or disables preset switching via input
        /// </summary>
        public void SetPresetSwitchingEnabled(bool enabled)
        {
            allowPresetSwitching = enabled;
        }

        /// <summary>
        /// Enables or disables zoom input
        /// </summary>
        public void SetZoomInputEnabled(bool enabled)
        {
            allowZoomInput = enabled;
        }

        /// <summary>
        /// Forces immediate camera position update (no smoothing)
        /// </summary>
        public void SnapToPlayer()
        {
            if (playerTransform == null)
                return;

            Vector3 zoomedOffset = currentPreset.offset * currentZoom;
            Vector3 desiredPosition;

            if (currentPreset.followPlayerRotation)
            {
                desiredPosition = playerTransform.position + playerTransform.TransformDirection(zoomedOffset);
            }
            else
            {
                desiredPosition = playerTransform.position + zoomedOffset;
            }

            transform.position = desiredPosition;

            // Reset velocities
            positionVelocity = Vector3.zero;
            zoomVelocity = 0f;
        }

        #endregion
    }
}
