using UnityEngine;

namespace RoombaRampage.Player
{
    /// <summary>
    /// ScriptableObject containing all player configuration data.
    /// Allows easy balancing and multiple stat profiles (difficulty variants, character types).
    /// Create instances via: Right-click > Create > RoombaRampage > Player Stats
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "RoombaRampage/Player Stats", order = 1)]
    public class PlayerStats : ScriptableObject
    {
        #region Movement Configuration

        [Header("Movement")]
        [Tooltip("Acceleration force applied when moving (higher = faster acceleration)")]
        [Range(5f, 50f)]
        public float acceleration = 15f;

        [Tooltip("Maximum speed the player can achieve")]
        [Range(5f, 25f)]
        public float maxSpeed = 10f;

        [Tooltip("Rotation speed in degrees per second (higher = faster turning)")]
        [Range(90f, 360f)]
        public float rotationSpeed = 150f;

        [Tooltip("Use snapped rotation (8-directional) instead of smooth rotation?")]
        public bool useSnappedRotation = false;

        [Tooltip("Rotation snap angle in degrees (e.g., 45 for 8-directional, 90 for 4-directional)")]
        [Range(15f, 90f)]
        public float rotationSnapAngle = 45f;

        [Tooltip("Drift factor: 1.0 = no drift (full grip), 0.5 = heavy drift")]
        [Range(0.5f, 1f)]
        public float driftFactor = 0.92f;

        [Tooltip("Braking force applied when pressing opposite direction")]
        [Range(10f, 50f)]
        public float brakingForce = 20f;

        #endregion

        #region Physics Configuration

        [Header("Physics")]
        [Tooltip("Rigidbody mass (affects acceleration and collision response)")]
        [Range(0.5f, 5f)]
        public float mass = 1f;

        [Tooltip("Linear drag (friction): higher = faster slowdown when not moving")]
        [Range(0f, 5f)]
        public float drag = 1.5f;

        [Tooltip("Angular drag (rotation friction): higher = faster rotation slowdown")]
        [Range(0f, 10f)]
        public float angularDrag = 3f;

        #endregion

        #region Arena Boundaries

        [Header("Boundaries")]
        [Tooltip("Arena width in world units")]
        [Range(10f, 100f)]
        public float arenaWidth = 20f;

        [Tooltip("Arena height in world units")]
        [Range(10f, 100f)]
        public float arenaHeight = 15f;

        #endregion

        #region Health Configuration

        [Header("Health")]
        [Tooltip("Maximum health points")]
        [Range(10, 500)]
        public int maxHealth = 100;

        [Tooltip("Duration of invulnerability after taking damage (in seconds)")]
        [Range(0f, 5f)]
        public float invulnerabilityDuration = 1.5f;

        [Tooltip("Health regeneration per second (0 = no regen)")]
        [Range(0f, 10f)]
        public float healthRegenRate = 0f;

        #endregion

        #region Combat Configuration (Future)

        [Header("Combat - Future Use")]
        [Tooltip("Fire rate: time between shots (in seconds)")]
        [Range(0.05f, 2f)]
        public float fireRate = 0.2f;

        [Tooltip("Base damage per shot")]
        [Range(1, 100)]
        public int damage = 10;

        [Tooltip("Projectile speed")]
        [Range(5f, 50f)]
        public float projectileSpeed = 20f;

        [Tooltip("Maximum number of projectiles on screen")]
        [Range(10, 200)]
        public int maxProjectiles = 50;

        #endregion

        #region Upgrade Configuration (Future)

        [Header("Upgrades - Future Use")]
        [Tooltip("Speed boost multiplier cap")]
        [Range(1f, 5f)]
        public float maxSpeedMultiplier = 2.5f;

        [Tooltip("Fire rate multiplier cap")]
        [Range(1f, 5f)]
        public float maxFireRateMultiplier = 3f;

        [Tooltip("Damage multiplier cap")]
        [Range(1f, 10f)]
        public float maxDamageMultiplier = 5f;

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure values stay within reasonable ranges
            acceleration = Mathf.Max(1f, acceleration);
            maxSpeed = Mathf.Max(1f, maxSpeed);
            rotationSpeed = Mathf.Max(10f, rotationSpeed);
            driftFactor = Mathf.Clamp01(driftFactor);

            mass = Mathf.Max(0.1f, mass);
            drag = Mathf.Max(0f, drag);
            angularDrag = Mathf.Max(0f, angularDrag);

            arenaWidth = Mathf.Max(5f, arenaWidth);
            arenaHeight = Mathf.Max(5f, arenaHeight);

            maxHealth = Mathf.Max(1, maxHealth);
            invulnerabilityDuration = Mathf.Max(0f, invulnerabilityDuration);
            healthRegenRate = Mathf.Max(0f, healthRegenRate);

            fireRate = Mathf.Max(0.01f, fireRate);
            damage = Mathf.Max(1, damage);
            projectileSpeed = Mathf.Max(1f, projectileSpeed);
            maxProjectiles = Mathf.Max(1, maxProjectiles);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a copy of this PlayerStats with modified values.
        /// Useful for temporary stat changes without modifying the original asset.
        /// </summary>
        public PlayerStats CreateCopy()
        {
            PlayerStats copy = CreateInstance<PlayerStats>();

            // Copy all values
            copy.acceleration = acceleration;
            copy.maxSpeed = maxSpeed;
            copy.rotationSpeed = rotationSpeed;
            copy.useSnappedRotation = useSnappedRotation;
            copy.rotationSnapAngle = rotationSnapAngle;
            copy.driftFactor = driftFactor;
            copy.brakingForce = brakingForce;

            copy.mass = mass;
            copy.drag = drag;
            copy.angularDrag = angularDrag;

            copy.arenaWidth = arenaWidth;
            copy.arenaHeight = arenaHeight;

            copy.maxHealth = maxHealth;
            copy.invulnerabilityDuration = invulnerabilityDuration;
            copy.healthRegenRate = healthRegenRate;

            copy.fireRate = fireRate;
            copy.damage = damage;
            copy.projectileSpeed = projectileSpeed;
            copy.maxProjectiles = maxProjectiles;

            copy.maxSpeedMultiplier = maxSpeedMultiplier;
            copy.maxFireRateMultiplier = maxFireRateMultiplier;
            copy.maxDamageMultiplier = maxDamageMultiplier;

            return copy;
        }

        /// <summary>
        /// Applies a multiplier to movement-related stats.
        /// </summary>
        public void ApplyMovementMultiplier(float multiplier)
        {
            acceleration *= multiplier;
            maxSpeed *= multiplier;
            rotationSpeed *= multiplier;
        }

        /// <summary>
        /// Applies a multiplier to combat-related stats.
        /// </summary>
        public void ApplyCombatMultiplier(float multiplier)
        {
            damage = Mathf.RoundToInt(damage * multiplier);
            fireRate /= multiplier; // Lower fire rate = faster shooting
        }

        #endregion

        #region Preset Configurations

        /// <summary>
        /// Applies "Easy Mode" preset values.
        /// </summary>
        [ContextMenu("Apply Easy Mode Preset")]
        public void ApplyEasyPreset()
        {
            acceleration = 20f;
            maxSpeed = 12f;
            rotationSpeed = 180f;
            maxHealth = 150;
            invulnerabilityDuration = 2f;
            damage = 15;
            Debug.Log("Applied Easy Mode preset to PlayerStats.");
        }

        /// <summary>
        /// Applies "Normal Mode" preset values (default).
        /// </summary>
        [ContextMenu("Apply Normal Mode Preset")]
        public void ApplyNormalPreset()
        {
            acceleration = 15f;
            maxSpeed = 10f;
            rotationSpeed = 150f;
            maxHealth = 100;
            invulnerabilityDuration = 1.5f;
            damage = 10;
            Debug.Log("Applied Normal Mode preset to PlayerStats.");
        }

        /// <summary>
        /// Applies "Hard Mode" preset values.
        /// </summary>
        [ContextMenu("Apply Hard Mode Preset")]
        public void ApplyHardPreset()
        {
            acceleration = 12f;
            maxSpeed = 8f;
            rotationSpeed = 120f;
            maxHealth = 75;
            invulnerabilityDuration = 1f;
            damage = 8;
            Debug.Log("Applied Hard Mode preset to PlayerStats.");
        }

        #endregion
    }
}
