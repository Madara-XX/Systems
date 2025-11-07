using UnityEngine;

namespace RoombaRampage.Skills.Helpers
{
    /// <summary>
    /// Type of stat that can be buffed.
    /// </summary>
    public enum BuffStatType
    {
        // Movement
        MaxSpeed,
        Acceleration,
        RotationSpeed,

        // Combat
        Damage,
        FireRate,
        ProjectileSpeed,

        // Health
        MaxHealth,
        HealthRegenRate,

        // Turbo
        TurboSpeedMultiplier,
        TurboRegenRate,
        MaxTurboEnergy,

        // Physics
        Mass,
        Drag
    }

    /// <summary>
    /// How the buff is applied to the base stat.
    /// </summary>
    public enum BuffApplicationType
    {
        Add,           // Adds flat value (e.g., +10 speed)
        Multiply,      // Multiplies by value (e.g., ×1.5 speed)
        Override       // Replaces value entirely
    }

    /// <summary>
    /// Configuration for a player stat buff.
    /// Can be used by skills to enhance player stats.
    /// </summary>
    [System.Serializable]
    public class PlayerBuffData
    {
        [Tooltip("Which stat to buff")]
        public BuffStatType statType = BuffStatType.MaxSpeed;

        [Tooltip("How to apply the buff")]
        public BuffApplicationType applicationType = BuffApplicationType.Multiply;

        [Tooltip("Buff value (meaning depends on application type)")]
        public float value = 1.5f;

        [Tooltip("Duration of buff in seconds (0 = permanent)")]
        [Range(0f, 60f)]
        public float duration = 0f;

        [Tooltip("Visual color tint for the buff effect")]
        public Color effectColor = Color.green;

        /// <summary>
        /// Creates a default buff (speed boost).
        /// </summary>
        public PlayerBuffData()
        {
            statType = BuffStatType.MaxSpeed;
            applicationType = BuffApplicationType.Multiply;
            value = 1.5f;
            duration = 0f;
            effectColor = Color.green;
        }

        /// <summary>
        /// Creates a buff with specified parameters.
        /// </summary>
        public PlayerBuffData(BuffStatType stat, BuffApplicationType application, float val, float dur = 0f)
        {
            statType = stat;
            applicationType = application;
            value = val;
            duration = dur;

            // Set default colors based on stat type
            switch (stat)
            {
                case BuffStatType.MaxSpeed:
                case BuffStatType.Acceleration:
                    effectColor = Color.cyan; // Speed = cyan
                    break;
                case BuffStatType.Damage:
                case BuffStatType.FireRate:
                    effectColor = Color.red; // Combat = red
                    break;
                case BuffStatType.MaxHealth:
                case BuffStatType.HealthRegenRate:
                    effectColor = Color.green; // Health = green
                    break;
                case BuffStatType.TurboSpeedMultiplier:
                case BuffStatType.TurboRegenRate:
                    effectColor = Color.yellow; // Turbo = yellow
                    break;
                default:
                    effectColor = Color.white;
                    break;
            }
        }

        /// <summary>
        /// Gets a human-readable description of this buff.
        /// </summary>
        public string GetDescription()
        {
            string durationText = duration > 0f ? $" for {duration}s" : " (permanent)";

            switch (applicationType)
            {
                case BuffApplicationType.Add:
                    return $"+{value} {statType}{durationText}";
                case BuffApplicationType.Multiply:
                    return $"×{value} {statType}{durationText}";
                case BuffApplicationType.Override:
                    return $"{statType} = {value}{durationText}";
                default:
                    return $"{statType} buff{durationText}";
            }
        }
    }

    /// <summary>
    /// Active buff instance on the player.
    /// Tracks duration and handles expiration.
    /// </summary>
    public class ActivePlayerBuff
    {
        public PlayerBuffData data;
        public float remainingDuration;
        public bool isPermanent;

        public ActivePlayerBuff(PlayerBuffData buffData)
        {
            data = buffData;
            remainingDuration = buffData.duration;
            isPermanent = buffData.duration <= 0f;
        }

        /// <summary>
        /// Updates the buff. Returns true if buff should be removed.
        /// </summary>
        public bool Update()
        {
            if (isPermanent)
                return false; // Never remove permanent buffs

            remainingDuration -= Time.deltaTime;
            return remainingDuration <= 0f;
        }

        /// <summary>
        /// Refreshes the buff duration.
        /// </summary>
        public void Refresh()
        {
            if (!isPermanent)
                remainingDuration = data.duration;
        }
    }
}
