using UnityEngine;
using UnityEngine.InputSystem;

namespace RoombaRampage.Player
{
    /// <summary>
    /// Handles New Input System integration for the player.
    /// Captures and exposes input values to other player components.
    /// Uses InputSystem_Actions (generated from .inputactions file).
    /// </summary>
    public class PlayerInput : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [Tooltip("Optional: Event channel for broadcasting input events")]
        [SerializeField] private PlayerEvents playerEvents;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        // Input actions reference
        private InputSystem_Actions inputActions;

        // Input state
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool attackPressed;
        private bool attackHeld;
        private bool interactPressed;
        private bool interactHeld;

        // Input enabled state
        private bool inputEnabled = true;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current movement input (WASD/Left Stick) - normalized Vector2.
        /// </summary>
        public Vector2 MoveInput => inputEnabled ? moveInput : Vector2.zero;

        /// <summary>
        /// Current look input (Mouse/Right Stick) - screen position or direction.
        /// </summary>
        public Vector2 LookInput => inputEnabled ? lookInput : Vector2.zero;

        /// <summary>
        /// Was attack button pressed this frame?
        /// </summary>
        public bool AttackPressed => inputEnabled && attackPressed;

        /// <summary>
        /// Is attack button currently held down?
        /// </summary>
        public bool AttackHeld => inputEnabled && attackHeld;

        /// <summary>
        /// Was interact button pressed this frame?
        /// </summary>
        public bool InteractPressed => inputEnabled && interactPressed;

        /// <summary>
        /// Is interact button currently held down?
        /// </summary>
        public bool InteractHeld => inputEnabled && interactHeld;

        /// <summary>
        /// Is input currently enabled?
        /// </summary>
        public bool IsInputEnabled => inputEnabled;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Initialize Input System Actions
            inputActions = new InputSystem_Actions();

            // Subscribe to input events
            SubscribeToInputEvents();
        }

        private void OnEnable()
        {
            // Enable input actions
            if (inputActions != null)
            {
                inputActions.Player.Enable();
            }
        }

        private void OnDisable()
        {
            // Disable input actions
            if (inputActions != null)
            {
                inputActions.Player.Disable();
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from input events
            UnsubscribeFromInputEvents();

            // Dispose of input actions
            inputActions?.Dispose();
        }

        private void LateUpdate()
        {
            // Reset per-frame input states
            attackPressed = false;
            interactPressed = false;

            // Debug display
            if (showDebugInfo)
            {
                DisplayDebugInfo();
            }
        }

        #endregion

        #region Input Event Handlers

        /// <summary>
        /// Subscribes to all input action events.
        /// </summary>
        private void SubscribeToInputEvents()
        {
            if (inputActions == null) return;

            inputActions.Player.Move.performed += OnMove;
            inputActions.Player.Move.canceled += OnMove;

            inputActions.Player.Look.performed += OnLook;
            inputActions.Player.Look.canceled += OnLook;

            inputActions.Player.Attack.started += OnAttackStarted;
            inputActions.Player.Attack.performed += OnAttackPerformed;
            inputActions.Player.Attack.canceled += OnAttackCanceled;

            inputActions.Player.Interact.started += OnInteractStarted;
            inputActions.Player.Interact.performed += OnInteractPerformed;
            inputActions.Player.Interact.canceled += OnInteractCanceled;
        }

        /// <summary>
        /// Unsubscribes from all input action events.
        /// </summary>
        private void UnsubscribeFromInputEvents()
        {
            if (inputActions == null) return;

            inputActions.Player.Move.performed -= OnMove;
            inputActions.Player.Move.canceled -= OnMove;

            inputActions.Player.Look.performed -= OnLook;
            inputActions.Player.Look.canceled -= OnLook;

            inputActions.Player.Attack.started -= OnAttackStarted;
            inputActions.Player.Attack.performed -= OnAttackPerformed;
            inputActions.Player.Attack.canceled -= OnAttackCanceled;

            inputActions.Player.Interact.started -= OnInteractStarted;
            inputActions.Player.Interact.performed -= OnInteractPerformed;
            inputActions.Player.Interact.canceled -= OnInteractCanceled;
        }

        /// <summary>
        /// Handles Move input (WASD/Left Stick).
        /// </summary>
        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Handles Look input (Mouse/Right Stick).
        /// </summary>
        private void OnLook(InputAction.CallbackContext context)
        {
            lookInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Handles Attack input start (button pressed).
        /// </summary>
        private void OnAttackStarted(InputAction.CallbackContext context)
        {
            attackPressed = true;
            attackHeld = true;
        }

        /// <summary>
        /// Handles Attack input performed (continued hold).
        /// </summary>
        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            attackHeld = true;
        }

        /// <summary>
        /// Handles Attack input canceled (button released).
        /// </summary>
        private void OnAttackCanceled(InputAction.CallbackContext context)
        {
            attackHeld = false;
        }

        /// <summary>
        /// Handles Interact input start (button pressed).
        /// </summary>
        private void OnInteractStarted(InputAction.CallbackContext context)
        {
            interactPressed = true;
            interactHeld = true;
        }

        /// <summary>
        /// Handles Interact input performed (continued hold).
        /// </summary>
        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            interactHeld = true;
        }

        /// <summary>
        /// Handles Interact input canceled (button released).
        /// </summary>
        private void OnInteractCanceled(InputAction.CallbackContext context)
        {
            interactHeld = false;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Enables player input.
        /// </summary>
        public void EnableInput()
        {
            inputEnabled = true;

            if (inputActions != null)
            {
                inputActions.Player.Enable();
            }
        }

        /// <summary>
        /// Disables player input.
        /// Also clears all current input states.
        /// </summary>
        public void DisableInput()
        {
            inputEnabled = false;

            // Clear all input states
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            attackPressed = false;
            attackHeld = false;
            interactPressed = false;
            interactHeld = false;

            if (inputActions != null)
            {
                inputActions.Player.Disable();
            }
        }

        /// <summary>
        /// Gets the world position that the player is looking at (for mouse input).
        /// Converts screen position to world position on XZ plane.
        /// </summary>
        /// <returns>World position of look target (on XZ plane)</returns>
        public Vector3 GetLookWorldPosition()
        {
            // Get main camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("[PlayerInput] Main camera not found. Cannot convert look position.");
                return transform.position;
            }

            // Create a ray from camera through mouse position
            Ray ray = mainCamera.ScreenPointToRay(lookInput);

            // Create a plane at player Y height (XZ plane)
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

            // Raycast to find intersection point
            if (groundPlane.Raycast(ray, out float enter))
            {
                return ray.GetPoint(enter);
            }

            return transform.position;
        }

        /// <summary>
        /// Gets the direction vector from player to look target.
        /// Useful for aiming weapons.
        /// Returns direction on XZ plane (Y component is zero).
        /// </summary>
        /// <returns>Normalized direction vector (XZ plane)</returns>
        public Vector3 GetLookDirection()
        {
            Vector3 lookWorldPos = GetLookWorldPosition();
            Vector3 direction = lookWorldPos - transform.position;
            direction.y = 0f; // Flatten to XZ plane
            return direction.normalized;
        }

        /// <summary>
        /// Gets the angle in degrees from player to look target.
        /// Useful for rotating 3D models/turrets around Y axis.
        /// </summary>
        /// <returns>Angle in degrees (rotation around Y axis)</returns>
        public float GetLookAngle()
        {
            Vector3 direction = GetLookDirection();
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            return angle;
        }

        #endregion

        #region Debug Methods

        /// <summary>
        /// Displays debug information in Scene view.
        /// </summary>
        private void DisplayDebugInfo()
        {
            // Display in console (throttled)
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[PlayerInput] Move: {moveInput}, Look: {lookInput}, Attack: {attackHeld}, Interact: {interactHeld}");
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;

            // Display input state on screen
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Input Enabled: {inputEnabled}");
            GUILayout.Label($"Move Input: {moveInput}");
            GUILayout.Label($"Look Input: {lookInput}");
            GUILayout.Label($"Attack Pressed: {attackPressed}");
            GUILayout.Label($"Attack Held: {attackHeld}");
            GUILayout.Label($"Interact Pressed: {interactPressed}");
            GUILayout.Label($"Interact Held: {interactHeld}");
            GUILayout.EndArea();
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            // Draw move input direction on XZ plane
            if (moveInput.sqrMagnitude > 0.01f)
            {
                Gizmos.color = Color.green;
                Vector3 moveDir3D = new Vector3(moveInput.x, 0f, moveInput.y); // X=horizontal, Z=forward
                Gizmos.DrawRay(transform.position, moveDir3D * 2f);
            }

            // Draw look direction on XZ plane
            Vector3 lookDir = GetLookDirection();
            if (lookDir.sqrMagnitude > 0.01f)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, lookDir * 3f);
            }
        }

        #endregion
    }
}
