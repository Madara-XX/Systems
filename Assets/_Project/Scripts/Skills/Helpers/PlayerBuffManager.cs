using UnityEngine;
using System.Collections.Generic;

namespace RoombaRampage.Skills.Helpers
{
    /// <summary>
    /// Manages active buffs on the player.
    /// Attach to the player GameObject.
    /// Applies buffs to PlayerStats and tracks durations.
    /// </summary>
    public class PlayerBuffManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("PlayerStats asset to modify")]
        [SerializeField] private Player.PlayerStats baseStats;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;

        private List<ActivePlayerBuff> activeBuffs = new List<ActivePlayerBuff>();
        private Player.PlayerStats modifiedStats;
        private Player.PlayerController playerController;

        private void Awake()
        {
            playerController = GetComponent<Player.PlayerController>();

            // Try to find PlayerStats if not assigned
            if (baseStats == null)
            {
                // Try to get from PlayerController
                if (playerController != null)
                {
                    // PlayerController doesn't expose stats directly, so we'll need to set this manually
                    Debug.LogWarning("[PlayerBuffManager] PlayerStats not assigned! Assign in Inspector.");
                }
            }

            // Create a runtime copy of stats
            if (baseStats != null)
            {
                modifiedStats = baseStats.CreateCopy();
            }
        }

        private void Update()
        {
            UpdateBuffs();
        }

        /// <summary>
        /// Applies a buff to the player.
        /// </summary>
        public void ApplyBuff(PlayerBuffData buffData)
        {
            // Check if buff of same type and application already exists
            ActivePlayerBuff existing = activeBuffs.Find(b =>
                b.data.statType == buffData.statType &&
                b.data.applicationType == buffData.applicationType);

            if (existing != null)
            {
                // Refresh duration for temporary buffs
                if (!existing.isPermanent)
                {
                    existing.Refresh();

                    if (showDebugInfo)
                        Debug.Log($"[PlayerBuff] Refreshed {buffData.statType} buff");
                }
            }
            else
            {
                // Add new buff
                ActivePlayerBuff newBuff = new ActivePlayerBuff(buffData);
                activeBuffs.Add(newBuff);

                if (showDebugInfo)
                {
                    string permanentText = newBuff.isPermanent ? " (permanent)" : $" for {buffData.duration}s";
                    Debug.Log($"[PlayerBuff] Applied {buffData.GetDescription()}");
                }
            }

            // Recalculate stats
            RecalculateStats();
        }

        /// <summary>
        /// Updates all active buffs and removes expired ones.
        /// </summary>
        private void UpdateBuffs()
        {
            if (activeBuffs.Count == 0)
                return;

            bool needsRecalculation = false;

            // Update each buff
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                ActivePlayerBuff buff = activeBuffs[i];
                bool shouldRemove = buff.Update();

                if (shouldRemove)
                {
                    if (showDebugInfo)
                        Debug.Log($"[PlayerBuff] {buff.data.statType} buff expired");

                    activeBuffs.RemoveAt(i);
                    needsRecalculation = true;
                }
            }

            // Recalculate stats if any buff was removed
            if (needsRecalculation)
            {
                RecalculateStats();
            }
        }

        /// <summary>
        /// Recalculates player stats based on all active buffs.
        /// </summary>
        private void RecalculateStats()
        {
            if (baseStats == null || modifiedStats == null)
            {
                Debug.LogWarning("[PlayerBuffManager] Cannot recalculate stats - baseStats or modifiedStats is null");
                return;
            }

            // Reset to base stats
            CopyStats(baseStats, modifiedStats);

            // Apply all active buffs
            foreach (ActivePlayerBuff buff in activeBuffs)
            {
                ApplyBuffToStats(buff.data, modifiedStats);
            }

            // Update PlayerController with new stats (if applicable)
            // Note: PlayerController might not have a public method to update stats at runtime
            // This would need to be added to PlayerController

            if (showDebugInfo)
            {
                Debug.Log($"[PlayerBuffManager] Recalculated stats with {activeBuffs.Count} active buff(s)");
            }
        }

        /// <summary>
        /// Applies a single buff to the stats object.
        /// </summary>
        private void ApplyBuffToStats(PlayerBuffData buff, Player.PlayerStats stats)
        {
            switch (buff.statType)
            {
                case BuffStatType.MaxSpeed:
                    stats.maxSpeed = ApplyValue(stats.maxSpeed, buff.value, buff.applicationType);
                    break;
                case BuffStatType.Acceleration:
                    stats.acceleration = ApplyValue(stats.acceleration, buff.value, buff.applicationType);
                    break;
                case BuffStatType.RotationSpeed:
                    stats.rotationSpeed = ApplyValue(stats.rotationSpeed, buff.value, buff.applicationType);
                    break;
                case BuffStatType.Damage:
                    stats.damage = Mathf.RoundToInt(ApplyValue(stats.damage, buff.value, buff.applicationType));
                    break;
                case BuffStatType.FireRate:
                    stats.fireRate = ApplyValue(stats.fireRate, buff.value, buff.applicationType);
                    break;
                case BuffStatType.ProjectileSpeed:
                    stats.projectileSpeed = ApplyValue(stats.projectileSpeed, buff.value, buff.applicationType);
                    break;
                case BuffStatType.MaxHealth:
                    stats.maxHealth = Mathf.RoundToInt(ApplyValue(stats.maxHealth, buff.value, buff.applicationType));
                    break;
                case BuffStatType.HealthRegenRate:
                    stats.healthRegenRate = ApplyValue(stats.healthRegenRate, buff.value, buff.applicationType);
                    break;
                case BuffStatType.TurboSpeedMultiplier:
                    stats.turboSpeedMultiplier = ApplyValue(stats.turboSpeedMultiplier, buff.value, buff.applicationType);
                    break;
                case BuffStatType.TurboRegenRate:
                    stats.turboRegenRate = ApplyValue(stats.turboRegenRate, buff.value, buff.applicationType);
                    break;
                case BuffStatType.MaxTurboEnergy:
                    stats.maxTurboEnergy = ApplyValue(stats.maxTurboEnergy, buff.value, buff.applicationType);
                    break;
                case BuffStatType.Mass:
                    stats.mass = ApplyValue(stats.mass, buff.value, buff.applicationType);
                    break;
                case BuffStatType.Drag:
                    stats.drag = ApplyValue(stats.drag, buff.value, buff.applicationType);
                    break;
            }
        }

        /// <summary>
        /// Applies a buff value based on application type.
        /// </summary>
        private float ApplyValue(float baseValue, float buffValue, BuffApplicationType applicationType)
        {
            switch (applicationType)
            {
                case BuffApplicationType.Add:
                    return baseValue + buffValue;
                case BuffApplicationType.Multiply:
                    return baseValue * buffValue;
                case BuffApplicationType.Override:
                    return buffValue;
                default:
                    return baseValue;
            }
        }

        /// <summary>
        /// Copies all stat values from source to destination.
        /// </summary>
        private void CopyStats(Player.PlayerStats source, Player.PlayerStats destination)
        {
            destination.acceleration = source.acceleration;
            destination.maxSpeed = source.maxSpeed;
            destination.rotationSpeed = source.rotationSpeed;
            destination.useSnappedRotation = source.useSnappedRotation;
            destination.rotationSnapAngle = source.rotationSnapAngle;
            destination.driftFactor = source.driftFactor;
            destination.brakingForce = source.brakingForce;

            destination.maxTurboEnergy = source.maxTurboEnergy;
            destination.turboSpeedMultiplier = source.turboSpeedMultiplier;
            destination.turboConsumptionRate = source.turboConsumptionRate;
            destination.turboRegenRate = source.turboRegenRate;
            destination.turboRegenDelay = source.turboRegenDelay;

            destination.mass = source.mass;
            destination.drag = source.drag;
            destination.angularDrag = source.angularDrag;

            destination.maxHealth = source.maxHealth;
            destination.invulnerabilityDuration = source.invulnerabilityDuration;
            destination.healthRegenRate = source.healthRegenRate;

            destination.fireRate = source.fireRate;
            destination.damage = source.damage;
            destination.projectileSpeed = source.projectileSpeed;
            destination.maxProjectiles = source.maxProjectiles;
        }

        /// <summary>
        /// Gets the current modified stats.
        /// </summary>
        public Player.PlayerStats GetModifiedStats()
        {
            return modifiedStats;
        }

        /// <summary>
        /// Removes all temporary buffs.
        /// </summary>
        public void ClearTemporaryBuffs()
        {
            activeBuffs.RemoveAll(b => !b.isPermanent);
            RecalculateStats();

            if (showDebugInfo)
                Debug.Log("[PlayerBuffManager] Cleared all temporary buffs");
        }

        /// <summary>
        /// Removes all buffs (including permanent ones).
        /// </summary>
        public void ClearAllBuffs()
        {
            activeBuffs.Clear();
            RecalculateStats();

            if (showDebugInfo)
                Debug.Log("[PlayerBuffManager] Cleared all buffs");
        }

        /// <summary>
        /// Removes a specific buff type.
        /// </summary>
        public void RemoveBuff(BuffStatType statType)
        {
            int removed = activeBuffs.RemoveAll(b => b.data.statType == statType);

            if (removed > 0)
            {
                RecalculateStats();

                if (showDebugInfo)
                    Debug.Log($"[PlayerBuffManager] Removed {removed} {statType} buff(s)");
            }
        }

        /// <summary>
        /// Checks if a specific buff type is active.
        /// </summary>
        public bool HasBuff(BuffStatType statType)
        {
            return activeBuffs.Exists(b => b.data.statType == statType);
        }

        /// <summary>
        /// Gets the remaining duration of a specific buff type.
        /// </summary>
        public float GetBuffDuration(BuffStatType statType)
        {
            ActivePlayerBuff buff = activeBuffs.Find(b => b.data.statType == statType);
            return buff != null ? buff.remainingDuration : 0f;
        }

        #region Debug

        private void OnGUI()
        {
            if (!showDebugInfo || activeBuffs.Count == 0)
                return;

            GUILayout.BeginArea(new Rect(10, 550, 400, 200));
            GUILayout.Label("<b>Active Buffs</b>");

            foreach (ActivePlayerBuff buff in activeBuffs)
            {
                string durationText = buff.isPermanent ? "Permanent" : $"{buff.remainingDuration:F1}s";
                GUILayout.Label($"{buff.data.statType} ({buff.data.applicationType} {buff.data.value:F2}) - {durationText}");
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
