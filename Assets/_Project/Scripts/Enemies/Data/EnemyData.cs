using UnityEngine;

namespace RoombaRampage.Enemies
{
    /// <summary>
    /// ScriptableObject containing all data for an enemy type.
    /// Create instances via Assets > Create > RoombaRampage > Enemy Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "RoombaRampage/Enemy Data", order = 2)]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Display name of the enemy")]
        public string enemyName = "New Enemy";

        [Tooltip("Description of the enemy")]
        [TextArea(2, 4)]
        public string description = "An enemy description.";

        [Header("Combat Stats")]
        [Tooltip("Maximum health of the enemy")]
        [Min(1f)]
        public float maxHealth = 50f;

        [Tooltip("Damage dealt to player on collision")]
        [Min(0f)]
        public float collisionDamage = 10f;

        [Tooltip("Cooldown between collision damage (seconds)")]
        [Min(0f)]
        public float damageCooldown = 1f;

        [Header("Movement")]
        [Tooltip("Movement speed on XZ plane")]
        [Min(0f)]
        public float moveSpeed = 3f;

        [Tooltip("Rotation speed when turning toward player (degrees/sec)")]
        [Min(0f)]
        public float rotationSpeed = 180f;

        [Tooltip("Distance to stop from player (0 = move right up to player)")]
        [Min(0f)]
        public float stoppingDistance = 0f;

        [Header("AI Behavior")]
        [Tooltip("How often AI updates target (seconds) - lower = more responsive, higher = better performance")]
        [Min(0.1f)]
        public float aiUpdateInterval = 0.2f;

        [Tooltip("Detection range for finding player (0 = infinite)")]
        [Min(0f)]
        public float detectionRange = 0f; // 0 = infinite range

        [Header("Rewards")]
        [Tooltip("Score awarded when enemy dies")]
        [Min(0)]
        public int scoreValue = 10;

        [Tooltip("XP awarded when enemy dies")]
        [Min(0)]
        public int xpValue = 25;

        [Header("Visuals (Optional - Not Required)")]
        [Tooltip("Optional prefab reference - NOT REQUIRED. The spawner uses prefabs directly, not this field.")]
        public GameObject prefab;

        [Header("Audio/VFX References")]
        [Tooltip("Sound when enemy takes damage (not implemented yet)")]
        public AudioClip hurtSound;

        [Tooltip("Sound when enemy dies (not implemented yet)")]
        public AudioClip deathSound;

        [Tooltip("Particle effect when enemy spawns (not implemented yet)")]
        public GameObject spawnEffectPrefab;

        [Tooltip("Particle effect when enemy dies (not implemented yet)")]
        public GameObject deathEffectPrefab;

        #region Runtime Helpers

        /// <summary>
        /// Checks if enemy has infinite detection range.
        /// </summary>
        public bool HasInfiniteRange() => detectionRange <= 0f;

        /// <summary>
        /// Checks if player is in detection range.
        /// </summary>
        /// <param name="enemyPosition">Enemy position</param>
        /// <param name="playerPosition">Player position</param>
        /// <returns>True if player is in range</returns>
        public bool IsPlayerInRange(Vector3 enemyPosition, Vector3 playerPosition)
        {
            if (HasInfiniteRange()) return true;

            float distanceSqr = (playerPosition - enemyPosition).sqrMagnitude;
            float rangeSqr = detectionRange * detectionRange;
            return distanceSqr <= rangeSqr;
        }

        /// <summary>
        /// Checks if enemy should stop moving (reached stopping distance).
        /// </summary>
        /// <param name="distanceToPlayer">Current distance to player</param>
        /// <returns>True if should stop</returns>
        public bool ShouldStop(float distanceToPlayer)
        {
            return distanceToPlayer <= stoppingDistance;
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure sensible defaults
            if (maxHealth < 1f) maxHealth = 1f;
            if (collisionDamage < 0f) collisionDamage = 0f;
            if (damageCooldown < 0f) damageCooldown = 0f;
            if (moveSpeed < 0f) moveSpeed = 0f;
            if (rotationSpeed < 0f) rotationSpeed = 0f;
            if (stoppingDistance < 0f) stoppingDistance = 0f;
            if (aiUpdateInterval < 0.1f) aiUpdateInterval = 0.1f;
            if (detectionRange < 0f) detectionRange = 0f;
            if (scoreValue < 0) scoreValue = 0;
            if (xpValue < 0) xpValue = 0;
        }

        #endregion
    }
}
