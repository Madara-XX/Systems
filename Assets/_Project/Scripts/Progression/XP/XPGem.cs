using UnityEngine;

namespace RoombaRampage.Progression
{
    /// <summary>
    /// XP Gem pickup that flies toward player when in range.
    /// Spawned by XPGemPool when enemies die.
    ///
    /// Features:
    /// - Magnetic attraction to player (smooth lerp with acceleration)
    /// - Auto-collection on reaching player
    /// - Visual rotation and effects
    /// - Poolable (returns to pool after collection)
    /// - Configurable lifetime (optional despawn)
    ///
    /// Attach to XP gem prefab with:
    /// - MeshRenderer or SpriteRenderer for visuals
    /// - Collider (trigger) for player detection
    /// - Optional: Particle system for sparkle effects
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class XPGem : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")]
        [Tooltip("XP value awarded when collected")]
        [SerializeField] private int xpValue = 25;

        [Header("Visual Effects")]
        [Tooltip("Rotation speed (degrees/second)")]
        [SerializeField] private float rotationSpeed = 180f;

        [Tooltip("Scale pulse animation")]
        [SerializeField] private bool enablePulseAnimation = true;

        [Tooltip("Pulse scale range (min-max)")]
        [SerializeField] private Vector2 pulseScaleRange = new Vector2(0.9f, 1.1f);

        [Tooltip("Pulse speed")]
        [SerializeField] private float pulseSpeed = 2f;

        [Header("References")]
        [Tooltip("Optional: Particle system for sparkle effects")]
        [SerializeField] private ParticleSystem sparkleParticles;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        #endregion

        #region Private Fields

        // Movement state
        private bool isBeingAttracted = false;
        private Transform playerTransform;
        private Vector3 velocity = Vector3.zero;
        private float attractionProgress = 0f;

        // Lifetime tracking
        private float spawnTime;
        private float lifetime;

        // Visual state
        private Vector3 originalScale;
        private float pulseTimer = 0f;

        // References
        private XPSettings xpSettings;
        private Collider gemCollider;

        #endregion

        #region Public Properties

        /// <summary>
        /// XP value of this gem.
        /// </summary>
        public int XPValue => xpValue;

        /// <summary>
        /// Is gem currently flying toward player?
        /// </summary>
        public bool IsBeingAttracted => isBeingAttracted;

        #endregion

        #region Initialization

        private void Awake()
        {
            // Get components
            gemCollider = GetComponent<Collider>();
            if (gemCollider != null)
            {
                gemCollider.isTrigger = true;
            }

            // Store original scale
            originalScale = transform.localScale;

            // Get XP settings
            if (XPManager.Instance != null)
            {
                xpSettings = XPManager.Instance.GetComponent<XPManager>().GetXPSettings();
            }
        }

        /// <summary>
        /// Initializes gem when spawned from pool.
        /// </summary>
        /// <param name="position">Spawn position</param>
        /// <param name="value">XP value</param>
        /// <param name="settings">XP settings reference</param>
        public void Initialize(Vector3 position, int value, XPSettings settings)
        {
            // Set position and value
            transform.position = position;
            xpValue = value;
            xpSettings = settings;

            // Reset state
            isBeingAttracted = false;
            attractionProgress = 0f;
            velocity = Vector3.zero;
            spawnTime = Time.time;
            lifetime = settings != null ? settings.gemLifetime : 30f;

            // Reset visuals
            transform.localScale = originalScale;
            pulseTimer = Random.Range(0f, Mathf.PI * 2f); // Random pulse offset

            // Enable collider
            if (gemCollider != null)
            {
                gemCollider.enabled = true;
            }

            // Start particle effect
            if (sparkleParticles != null)
            {
                sparkleParticles.Play();
            }

            // Find player
            FindPlayer();

            if (showDebugInfo)
            {
                Debug.Log($"[XPGem] Spawned at {position} with {xpValue} XP");
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            // Check lifetime
            if (lifetime > 0f && Time.time - spawnTime >= lifetime)
            {
                // Auto-collect after lifetime expires
                CollectGem();
                return;
            }

            // Find player if not found
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            // Check if player is in magnet range
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (xpSettings != null && distanceToPlayer <= xpSettings.magnetRange)
            {
                isBeingAttracted = true;
            }

            // Move toward player if attracted
            if (isBeingAttracted)
            {
                MoveTowardPlayer(distanceToPlayer);
            }

            // Visual effects
            UpdateVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if player collected gem
            if (other.CompareTag("Player"))
            {
                CollectGem();
            }
        }

        #endregion

        #region Movement Logic

        /// <summary>
        /// Finds player transform in scene.
        /// </summary>
        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        /// <summary>
        /// Moves gem toward player with magnetic attraction.
        /// Uses acceleration curve for snappy feel.
        /// </summary>
        private void MoveTowardPlayer(float distance)
        {
            if (playerTransform == null || xpSettings == null) return;

            // Update attraction progress (0-1)
            float maxDistance = xpSettings.magnetRange;
            attractionProgress = 1f - Mathf.Clamp01(distance / maxDistance);

            // Get acceleration multiplier from curve
            float accelerationMultiplier = xpSettings.magnetAccelerationCurve.Evaluate(attractionProgress);

            // Calculate direction to player
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;

            // Move on XZ plane (top-down game)
            directionToPlayer.y = 0f;

            // Calculate speed based on distance (faster as closer)
            float speed = xpSettings.magnetSpeed * accelerationMultiplier;

            // Move toward player
            transform.position = Vector3.MoveTowards(
                transform.position,
                playerTransform.position,
                speed * Time.deltaTime
            );

            // Alternate: Smooth lerp movement
            // transform.position = Vector3.Lerp(
            //     transform.position,
            //     playerTransform.position,
            //     Time.deltaTime * speed
            // );
        }

        #endregion

        #region Visual Effects

        /// <summary>
        /// Updates visual effects (rotation, pulse).
        /// </summary>
        private void UpdateVisuals()
        {
            // Rotate gem
            if (rotationSpeed > 0f)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }

            // Pulse scale animation
            if (enablePulseAnimation)
            {
                pulseTimer += Time.deltaTime * pulseSpeed;
                float pulseFactor = Mathf.Lerp(pulseScaleRange.x, pulseScaleRange.y, (Mathf.Sin(pulseTimer) + 1f) / 2f);
                transform.localScale = originalScale * pulseFactor;
            }
        }

        #endregion

        #region Collection

        /// <summary>
        /// Collects gem and awards XP to player.
        /// Returns gem to pool.
        /// </summary>
        private void CollectGem()
        {
            if (showDebugInfo)
            {
                Debug.Log($"[XPGem] Collected! Awarding {xpValue} XP");
            }

            // Award XP
            if (XPManager.Instance != null)
            {
                XPManager.Instance.AddXP(xpValue);
            }

            // Stop particles
            if (sparkleParticles != null)
            {
                sparkleParticles.Stop();
            }

            // Return to pool
            ReturnToPool();
        }

        /// <summary>
        /// Returns gem to pool for reuse.
        /// </summary>
        private void ReturnToPool()
        {
            if (XPGemPool.Instance != null)
            {
                XPGemPool.Instance.ReturnGem(this);
            }
            else
            {
                // No pool available - destroy
                Destroy(gameObject);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets XP value (useful for different enemy types).
        /// </summary>
        public void SetXPValue(int value)
        {
            xpValue = Mathf.Max(1, value);
        }

        /// <summary>
        /// Forces immediate collection (useful for vacuum effects).
        /// </summary>
        public void ForceCollect()
        {
            CollectGem();
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            // Draw magnet range
            if (xpSettings != null)
            {
                Gizmos.color = isBeingAttracted ? Color.green : Color.yellow;
                Gizmos.DrawWireSphere(transform.position, 0.5f);

                // Draw line to player if attracted
                if (isBeingAttracted && playerTransform != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(transform.position, playerTransform.position);
                }
            }
        }

        #endregion
    }

    #region Extension Methods

    /// <summary>
    /// Extension to access XPSettings from XPManager (helper).
    /// </summary>
    public static class XPManagerExtensions
    {
        private static System.Reflection.FieldInfo xpSettingsField;

        public static XPSettings GetXPSettings(this XPManager manager)
        {
            if (xpSettingsField == null)
            {
                xpSettingsField = typeof(XPManager).GetField("xpSettings",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            }

            return xpSettingsField?.GetValue(manager) as XPSettings;
        }
    }

    #endregion
}
