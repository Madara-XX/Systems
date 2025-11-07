using UnityEngine;
using System.Collections;

namespace RoombaRampage.Player
{
    /// <summary>
    /// Core physics-based movement controller for the player's roomba character.
    /// Handles acceleration, rotation, speed limiting, and boundary constraints.
    /// Uses Rigidbody (3D) for natural physics-based movement and collision on XZ plane.
    /// Top-down 3D game with movement on XZ plane (Y is up).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [SerializeField] private PlayerStats stats;
        [Tooltip("Optional: Event channel for broadcasting movement events")]
        [SerializeField] private PlayerEvents playerEvents;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        #endregion

        #region Private Fields

        // Cached component references
        private Rigidbody rb;
        private PlayerInput playerInput;

        // Movement state
        private Vector3 moveDirection;
        private float currentSpeed;
        private bool isMoving;
        private bool isEnabled = true;

        // Speed boost state
        private Coroutine speedBoostCoroutine;
        private float speedMultiplier = 1f;

        // Turbo boost state
        private float currentTurboEnergy;
        private bool isTurboActive;
        private float timeSinceTurboUse;

        // Constants
        private const float MinSpeedThreshold = 0.1f;
        private const float MinRotationSpeed = 0.5f;

        #endregion

        #region Public Properties

        /// <summary>
        /// Current velocity of the player (read-only).
        /// </summary>
        public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;

        /// <summary>
        /// Current speed magnitude of the player (read-only).
        /// </summary>
        public float CurrentSpeed => currentSpeed;

        /// <summary>
        /// Is the player currently moving (read-only).
        /// </summary>
        public bool IsMoving => isMoving;

        /// <summary>
        /// Is the controller currently enabled (read-only).
        /// </summary>
        public bool IsEnabled => isEnabled;

        /// <summary>
        /// Current turbo energy (read-only).
        /// </summary>
        public float CurrentTurboEnergy => currentTurboEnergy;

        /// <summary>
        /// Maximum turbo energy (read-only).
        /// </summary>
        public float MaxTurboEnergy => stats != null ? stats.maxTurboEnergy : 100f;

        /// <summary>
        /// Is turbo boost currently active (read-only).
        /// </summary>
        public bool IsTurboActive => isTurboActive;

        /// <summary>
        /// Turbo energy percentage (0-1) (read-only).
        /// </summary>
        public float TurboEnergyPercent => stats != null ? currentTurboEnergy / stats.maxTurboEnergy : 0f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Cache component references (best practice: do in Awake)
            rb = GetComponent<Rigidbody>();
            playerInput = GetComponent<PlayerInput>();

            // Validate configuration
            if (stats == null)
            {
                Debug.LogError($"[PlayerController] PlayerStats not assigned on {gameObject.name}. Please assign in Inspector.");
                enabled = false;
                return;
            }

            // Configure Rigidbody from stats
            ConfigureRigidbody();

            // Initialize turbo energy to max
            currentTurboEnergy = stats.maxTurboEnergy;
        }

        private void FixedUpdate()
        {
            if (!isEnabled) return;

            // Cache input values (convert 2D input to 3D XZ plane, unless slope movement is enabled)
            Vector2 moveInput2D = playerInput.MoveInput;
            Vector3 moveInput = new Vector3(moveInput2D.x, 0f, moveInput2D.y); // X=horizontal, Z=forward, Y=0

            // Update turbo boost state
            bool sprintInput = playerInput.SprintHeld;
            UpdateTurboBoost(sprintInput);

            // Update movement state
            UpdateMovementState(moveInput);

            // Apply physics-based movement
            ApplyMovement(moveInput);
            ApplyRotation(moveInput);
            ApplyFriction();

            // Enforce speed limits
            ClampSpeed();

            // Keep player within arena boundaries (only if not on slopes)
            if (!stats.allowSlopeMovement)
            {
                ClampToBoundaries();
            }

            // Broadcast movement events
            BroadcastMovementEvents();
        }

        #endregion

        #region Movement Methods

        /// <summary>
        /// Updates movement state variables based on input.
        /// </summary>
        private void UpdateMovementState(Vector3 moveInput)
        {
            // Get velocity on XZ plane (ignore Y)
            Vector3 velocityXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            currentSpeed = velocityXZ.magnitude;
            isMoving = moveInput.sqrMagnitude > 0.01f;
        }

        /// <summary>
        /// Applies force-based acceleration to the Rigidbody (3D).
        /// Creates a vehicle-like acceleration feel rather than instant movement.
        /// Movement on XZ plane only (Y is locked).
        /// </summary>
        private void ApplyMovement(Vector3 moveInput)
        {
            if (moveInput.sqrMagnitude < 0.01f) return;

            // Calculate combined multiplier (speed boost + turbo)
            float combinedMultiplier = speedMultiplier;
            if (isTurboActive)
            {
                combinedMultiplier *= stats.turboSpeedMultiplier;
            }

            // Calculate acceleration force on XZ plane (apply turbo to acceleration too)
            Vector3 moveForce = moveInput.normalized * stats.acceleration * combinedMultiplier * rb.mass;
            moveForce.y = 0f; // Ensure no vertical force

            // Apply force to Rigidbody
            rb.AddForce(moveForce, ForceMode.Force);
        }

        /// <summary>
        /// Applies rotation toward movement direction with smooth or snapped interpolation.
        /// Creates a steering feel rather than instant snapping.
        /// Rotates around Y axis for top-down 3D.
        /// </summary>
        private void ApplyRotation(Vector3 moveInput)
        {
            // Only rotate if moving above minimum threshold
            if (currentSpeed < MinRotationSpeed) return;
            if (moveInput.sqrMagnitude < 0.01f) return;

            // Calculate target rotation from input direction (XZ plane)
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);

            // Apply rotation snapping if enabled
            if (stats.useSnappedRotation)
            {
                targetRotation = SnapRotationToAngle(targetRotation, stats.rotationSnapAngle);
            }

            // Smooth rotation interpolation (rotationSpeed is in degrees/second)
            float maxDegreesDelta = stats.rotationSpeed * Time.fixedDeltaTime;
            Quaternion newRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, maxDegreesDelta);
            rb.MoveRotation(newRotation);

            // Apply drift effect if enabled
            if (stats.driftFactor < 1f)
            {
                ApplyDrift();
            }
        }

        /// <summary>
        /// Applies drift effect by lerping velocity toward forward direction.
        /// Lower drift factor = more drift. Higher = less drift (more grip).
        /// </summary>
        private void ApplyDrift()
        {
            // Get forward direction on XZ plane (3D forward is Z axis)
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();

            // Get velocity on XZ plane
            Vector3 velocityXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Calculate how aligned velocity is with forward direction
            float forwardDot = Vector3.Dot(forward, velocityXZ.normalized);

            // Only apply drift if not moving perfectly forward
            if (forwardDot < 0.95f)
            {
                // Lerp velocity toward forward direction
                float driftAmount = (1f - stats.driftFactor) * Time.fixedDeltaTime;
                Vector3 targetVelocity = forward * velocityXZ.magnitude;
                Vector3 newVelocity = Vector3.Lerp(velocityXZ, targetVelocity, driftAmount);

                // Apply new velocity (preserve Y component)
                rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
            }
        }

        /// <summary>
        /// Snaps a rotation to the nearest multiple of the given angle.
        /// Used for 8-directional (45°) or 4-directional (90°) movement.
        /// </summary>
        private Quaternion SnapRotationToAngle(Quaternion rotation, float snapAngle)
        {
            // Get the euler angle (Y rotation for top-down)
            float yAngle = rotation.eulerAngles.y;

            // Snap to nearest multiple of snapAngle
            float snappedAngle = Mathf.Round(yAngle / snapAngle) * snapAngle;

            // Return snapped rotation
            return Quaternion.Euler(0f, snappedAngle, 0f);
        }

        /// <summary>
        /// Applies friction when no input is provided.
        /// Creates a gradual slowdown rather than instant stop.
        /// Only affects XZ plane movement.
        /// </summary>
        private void ApplyFriction()
        {
            // Only apply friction when not providing input
            if (isMoving) return;

            // Only apply if moving above threshold
            if (currentSpeed < MinSpeedThreshold)
            {
                // Stop XZ movement, preserve Y
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                return;
            }

            // Apply gradual slowdown on XZ plane
            float frictionAmount = stats.drag * Time.fixedDeltaTime;
            Vector3 velocityXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 newVelocityXZ = Vector3.Lerp(velocityXZ, Vector3.zero, frictionAmount);

            // Apply new velocity (preserve Y component)
            rb.linearVelocity = new Vector3(newVelocityXZ.x, rb.linearVelocity.y, newVelocityXZ.z);
        }

        /// <summary>
        /// Clamps velocity to maximum speed limit.
        /// Prevents player from exceeding configured max speed.
        /// Only affects XZ plane movement.
        /// </summary>
        private void ClampSpeed()
        {
            // Combine both speed boost and turbo boost multipliers
            float combinedMultiplier = speedMultiplier;
            if (isTurboActive)
            {
                combinedMultiplier *= stats.turboSpeedMultiplier;
            }

            float maxSpeed = stats.maxSpeed * combinedMultiplier;

            // Get velocity on XZ plane (or full 3D if slope movement is enabled)
            Vector3 velocity = rb.linearVelocity;
            Vector3 velocityXZ = stats.allowSlopeMovement
                ? velocity
                : new Vector3(velocity.x, 0f, velocity.z);

            if (velocityXZ.magnitude > maxSpeed)
            {
                Vector3 clampedVelocity = velocityXZ.normalized * maxSpeed;
                // Apply clamped velocity (preserve Y component if not on slopes)
                if (stats.allowSlopeMovement)
                {
                    rb.linearVelocity = clampedVelocity;
                }
                else
                {
                    rb.linearVelocity = new Vector3(clampedVelocity.x, rb.linearVelocity.y, clampedVelocity.z);
                }
            }
        }

        /// <summary>
        /// Keeps player within arena boundaries.
        /// Stops movement when hitting boundary edge.
        /// Works on XZ plane for 3D top-down game.
        /// </summary>
        private void ClampToBoundaries()
        {
            Vector3 position = transform.position;
            Vector3 clampedPosition = position;

            // Calculate boundary limits (arena on XZ plane)
            float halfWidth = stats.arenaWidth * 0.5f;
            float halfHeight = stats.arenaHeight * 0.5f;

            // Clamp position to boundaries (X and Z axes)
            clampedPosition.x = Mathf.Clamp(position.x, -halfWidth, halfWidth);
            clampedPosition.z = Mathf.Clamp(position.z, -halfHeight, halfHeight);

            // If position was clamped, stop velocity in that direction
            if (clampedPosition != position)
            {
                transform.position = clampedPosition;

                // Stop velocity perpendicular to boundary
                Vector3 velocity = rb.linearVelocity;
                if (Mathf.Abs(clampedPosition.x - position.x) > 0.01f)
                {
                    velocity.x = 0f;
                }
                if (Mathf.Abs(clampedPosition.z - position.z) > 0.01f)
                {
                    velocity.z = 0f;
                }
                rb.linearVelocity = velocity;
            }
        }

        /// <summary>
        /// Updates turbo boost state, consuming and regenerating energy.
        /// </summary>
        private void UpdateTurboBoost(bool sprintInput)
        {
            // Track time since last turbo use
            if (!sprintInput || currentTurboEnergy <= 0f)
            {
                timeSinceTurboUse += Time.fixedDeltaTime;
            }
            else
            {
                timeSinceTurboUse = 0f;
            }

            // Update turbo active state
            bool wasTurboActive = isTurboActive;
            isTurboActive = sprintInput && currentTurboEnergy > 0f && isMoving;

            // Consume turbo energy when active
            if (isTurboActive)
            {
                currentTurboEnergy -= stats.turboConsumptionRate * Time.fixedDeltaTime;
                currentTurboEnergy = Mathf.Max(0f, currentTurboEnergy);
            }
            // Regenerate turbo energy when not active (with delay)
            else if (timeSinceTurboUse >= stats.turboRegenDelay)
            {
                currentTurboEnergy += stats.turboRegenRate * Time.fixedDeltaTime;
                currentTurboEnergy = Mathf.Min(stats.maxTurboEnergy, currentTurboEnergy);
            }

            // Broadcast turbo state change events
            if (wasTurboActive != isTurboActive && playerEvents != null)
            {
                // You can add turbo events here if needed
            }
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Broadcasts movement-related events to PlayerEvents ScriptableObject.
        /// </summary>
        private void BroadcastMovementEvents()
        {
            if (playerEvents == null) return;

            // Broadcast speed changes (throttled to avoid spam)
            playerEvents.RaiseSpeedChanged(currentSpeed);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies a temporary speed boost multiplier.
        /// </summary>
        /// <param name="multiplier">Speed multiplier (e.g., 1.5 = 150% speed)</param>
        /// <param name="duration">Duration in seconds</param>
        public void ApplySpeedBoost(float multiplier, float duration)
        {
            // Cancel existing speed boost if any
            if (speedBoostCoroutine != null)
            {
                StopCoroutine(speedBoostCoroutine);
            }

            // Start new speed boost
            speedBoostCoroutine = StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
        }

        /// <summary>
        /// Resets player to spawn position with zero velocity.
        /// </summary>
        /// <param name="position">Spawn position</param>
        public void ResetToSpawn(Vector3 position)
        {
            transform.position = position;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            currentSpeed = 0f;
            isMoving = false;

            // Reset turbo energy
            currentTurboEnergy = stats.maxTurboEnergy;
            isTurboActive = false;
            timeSinceTurboUse = 0f;
        }

        /// <summary>
        /// Enables or disables the controller.
        /// When disabled, no movement is processed.
        /// </summary>
        /// <param name="enabled">Enable state</param>
        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;

            if (!enabled)
            {
                // Stop all movement when disabled
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// Applies an instant force impulse to the player.
        /// Useful for knockback, explosions, etc.
        /// </summary>
        /// <param name="force">Force vector to apply (on XZ plane)</param>
        public void ApplyImpulse(Vector3 force)
        {
            force.y = 0f; // Ensure no vertical impulse
            rb.AddForce(force, ForceMode.Impulse);
        }

        #endregion

        #region Coroutines

        /// <summary>
        /// Coroutine for temporary speed boost.
        /// </summary>
        private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
        {
            speedMultiplier = multiplier;

            yield return new WaitForSeconds(duration);

            speedMultiplier = 1f;
            speedBoostCoroutine = null;
        }

        #endregion

        #region Configuration Methods

        /// <summary>
        /// Configures Rigidbody (3D) properties from PlayerStats.
        /// </summary>
        private void ConfigureRigidbody()
        {
            if (rb == null) return;

            rb.mass = stats.mass;
            rb.linearDamping = stats.drag;
            rb.angularDamping = stats.angularDrag;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Configure gravity and constraints based on slope movement setting
            if (stats.allowSlopeMovement)
            {
                // Enable gravity and allow full 3D movement
                rb.useGravity = true;
                // Only freeze X/Z rotation for slope movement
                rb.constraints = RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationZ;
            }
            else
            {
                // Top-down game on flat XZ plane, no gravity
                rb.useGravity = false;
                // Freeze Y position and X/Z rotation (top-down 3D on XZ plane)
                rb.constraints = RigidbodyConstraints.FreezePositionY |
                                RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationZ;
            }
        }

        #endregion

        #region Debug Gizmos

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || stats == null) return;

            // Draw arena boundaries on XZ plane
            Gizmos.color = Color.yellow;
            Vector3 size = new Vector3(stats.arenaWidth, 0.1f, stats.arenaHeight);
            Gizmos.DrawWireCube(Vector3.zero, size);

            // Draw velocity vector
            if (Application.isPlaying && rb != null)
            {
                Gizmos.color = Color.green;
                Vector3 velocityXZ = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Gizmos.DrawRay(transform.position, velocityXZ);

                // Draw forward direction (Z axis in 3D)
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(transform.position, transform.forward * 2f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;

            // Draw max speed circle on XZ plane
            if (stats != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                DrawCircle(transform.position, stats.maxSpeed, 32);
            }
        }

        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                // Draw circle on XZ plane (Y = 0)
                Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, newPoint);
                prevPoint = newPoint;
            }
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure PlayerStats is assigned
            if (stats == null)
            {
                Debug.LogWarning($"[PlayerController] PlayerStats not assigned on {gameObject.name}.");
            }
        }

        #endregion
    }
}
