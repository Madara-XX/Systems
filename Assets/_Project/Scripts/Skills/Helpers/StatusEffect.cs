using UnityEngine;

namespace RoombaRampage.Skills.Helpers
{
    /// <summary>
    /// Type of status effect to apply to enemies.
    /// </summary>
    public enum StatusEffectType
    {
        None,       // No effect
        Burn,       // Damage over time
        Slow,       // Movement speed reduction
        Stun,       // Cannot move or act
        Poison,     // Damage over time (different visuals than burn)
        Freeze      // Stops movement completely
    }

    /// <summary>
    /// Configuration for a status effect.
    /// Can be used by any skill to apply effects to enemies.
    /// </summary>
    [System.Serializable]
    public class StatusEffectData
    {
        [Tooltip("Type of status effect")]
        public StatusEffectType effectType = StatusEffectType.None;

        [Tooltip("Duration of the effect in seconds")]
        [Range(0.1f, 10f)]
        public float duration = 3f;

        [Tooltip("Damage per second (for Burn/Poison)")]
        [Range(0f, 50f)]
        public float damagePerSecond = 5f;

        [Tooltip("Movement speed multiplier (for Slow) - 0.5 = 50% speed")]
        [Range(0.1f, 1f)]
        public float movementSpeedMultiplier = 0.5f;

        [Tooltip("Visual color tint for the effect")]
        public Color effectColor = Color.red;

        /// <summary>
        /// Creates a default status effect (no effect).
        /// </summary>
        public StatusEffectData()
        {
            effectType = StatusEffectType.None;
            duration = 3f;
            damagePerSecond = 5f;
            movementSpeedMultiplier = 0.5f;
            effectColor = Color.red;
        }

        /// <summary>
        /// Creates a status effect with specified type.
        /// </summary>
        public StatusEffectData(StatusEffectType type, float duration, float damagePerSecond = 5f, float speedMultiplier = 0.5f)
        {
            this.effectType = type;
            this.duration = duration;
            this.damagePerSecond = damagePerSecond;
            this.movementSpeedMultiplier = speedMultiplier;

            // Set default colors based on type
            switch (type)
            {
                case StatusEffectType.Burn:
                    effectColor = new Color(1f, 0.5f, 0f); // Orange
                    break;
                case StatusEffectType.Slow:
                    effectColor = new Color(0.5f, 0.5f, 1f); // Light blue
                    break;
                case StatusEffectType.Stun:
                    effectColor = Color.yellow;
                    break;
                case StatusEffectType.Poison:
                    effectColor = Color.green;
                    break;
                case StatusEffectType.Freeze:
                    effectColor = Color.cyan;
                    break;
                default:
                    effectColor = Color.white;
                    break;
            }
        }
    }

    /// <summary>
    /// Active status effect instance on an enemy.
    /// Tracks duration and handles tick logic.
    /// </summary>
    public class ActiveStatusEffect
    {
        public StatusEffectData data;
        public float remainingDuration;
        private float lastTickTime;

        public ActiveStatusEffect(StatusEffectData effectData)
        {
            data = effectData;
            remainingDuration = effectData.duration;
            lastTickTime = Time.time;
        }

        /// <summary>
        /// Updates the status effect. Returns true if effect should be removed.
        /// </summary>
        public bool Update(Enemies.EnemyHealth enemyHealth, Enemies.EnemyAI enemyAI)
        {
            remainingDuration -= Time.deltaTime;

            // Handle tick damage (for Burn/Poison)
            if (data.effectType == StatusEffectType.Burn || data.effectType == StatusEffectType.Poison)
            {
                if (Time.time - lastTickTime >= 1f) // Tick every second
                {
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(data.damagePerSecond);
                    }
                    lastTickTime = Time.time;
                }
            }

            // Handle movement modifiers (applied in EnemyAI)
            // Handled externally by checking GetMovementSpeedMultiplier()

            return remainingDuration <= 0f;
        }

        /// <summary>
        /// Gets the movement speed multiplier for this effect.
        /// </summary>
        public float GetMovementSpeedMultiplier()
        {
            switch (data.effectType)
            {
                case StatusEffectType.Slow:
                    return data.movementSpeedMultiplier;
                case StatusEffectType.Stun:
                case StatusEffectType.Freeze:
                    return 0f; // Cannot move
                default:
                    return 1f; // Normal speed
            }
        }

        /// <summary>
        /// Checks if enemy can act (for Stun).
        /// </summary>
        public bool CanAct()
        {
            return data.effectType != StatusEffectType.Stun;
        }
    }
}
