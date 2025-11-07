using UnityEngine;
using System.Collections.Generic;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Buff skill that enhances player stats either permanently or temporarily.
    /// Can buff multiple stats at once and scales with level.
    /// </summary>
    [CreateAssetMenu(fileName = "BuffSkill", menuName = "RoombaRampage/Skills/Buff Skill", order = 3)]
    public class BuffSkillData : SkillData
    {
        [Header("Buff Configuration")]
        [Tooltip("List of buffs to apply")]
        public List<Helpers.PlayerBuffData> buffs = new List<Helpers.PlayerBuffData>();

        [Tooltip("Should buffs scale with skill level?")]
        public bool scaleWithLevel = true;

        [Tooltip("Multiplier applied to buff values per level (1.0 = no scaling)")]
        [Range(1f, 2f)]
        public float levelScaling = 1.1f;

        [Tooltip("Visual effect prefab (optional, for buff activation)")]
        public GameObject buffEffectPrefab;

        [Tooltip("Duration the visual effect stays active")]
        [Range(0.1f, 3f)]
        public float effectDuration = 1f;

        [Header("Audio (Future Use)")]
        [Tooltip("Sound effect to play when buff is applied")]
        public AudioClip buffSound;

        /// <summary>
        /// Activates the buff skill, applying all configured buffs to the player.
        /// </summary>
        public override void Activate(GameObject caster, int level)
        {
            if (caster == null)
            {
                Debug.LogWarning($"[BuffSkill] Cannot activate - caster is null");
                return;
            }

            // Get or add PlayerBuffManager
            Helpers.PlayerBuffManager buffManager = caster.GetComponent<Helpers.PlayerBuffManager>();

            if (buffManager == null)
            {
                buffManager = caster.AddComponent<Helpers.PlayerBuffManager>();
                Debug.Log($"[BuffSkill] Added PlayerBuffManager to {caster.name}");
            }

            // Apply all buffs
            foreach (Helpers.PlayerBuffData buffData in buffs)
            {
                // Create a copy to avoid modifying the original data
                Helpers.PlayerBuffData scaledBuff = CreateScaledBuff(buffData, level);
                buffManager.ApplyBuff(scaledBuff);
            }

            // Spawn visual effect if configured
            if (buffEffectPrefab != null)
            {
                Vector3 spawnPosition = caster.transform.position;
                GameObject effectObj = Instantiate(buffEffectPrefab, spawnPosition, Quaternion.identity, caster.transform);

                // Auto-destroy effect after duration
                Destroy(effectObj, effectDuration);
            }

            // Play sound effect (future implementation)
            if (buffSound != null)
            {
                // AudioSource.PlayClipAtPoint(buffSound, caster.transform.position);
            }
        }

        /// <summary>
        /// Creates a scaled version of a buff based on skill level.
        /// </summary>
        private Helpers.PlayerBuffData CreateScaledBuff(Helpers.PlayerBuffData baseBuff, int level)
        {
            // Ensure level is at least 1
            level = Mathf.Max(1, level);

            // Create a copy
            Helpers.PlayerBuffData scaledBuff = new Helpers.PlayerBuffData(
                baseBuff.statType,
                baseBuff.applicationType,
                baseBuff.value,
                baseBuff.duration
            );

            scaledBuff.effectColor = baseBuff.effectColor;

            // Apply level scaling if enabled
            if (scaleWithLevel && level > 1)
            {
                float scalingMultiplier = Mathf.Pow(levelScaling, level - 1);

                switch (baseBuff.applicationType)
                {
                    case Helpers.BuffApplicationType.Add:
                        // Scale additive values linearly
                        scaledBuff.value = baseBuff.value * scalingMultiplier;
                        break;

                    case Helpers.BuffApplicationType.Multiply:
                        // Scale multiplicative values (e.g., 1.5 -> 1.6 -> 1.7)
                        float multiplierIncrease = (baseBuff.value - 1f) * scalingMultiplier;
                        scaledBuff.value = 1f + multiplierIncrease;
                        break;

                    case Helpers.BuffApplicationType.Override:
                        // Override values scale linearly
                        scaledBuff.value = baseBuff.value * scalingMultiplier;
                        break;
                }

                // Optional: Scale duration too (permanent buffs stay permanent)
                if (baseBuff.duration > 0f)
                {
                    // Increase duration by 20% per level
                    scaledBuff.duration = baseBuff.duration * (1f + (0.2f * (level - 1)));
                }
            }

            return scaledBuff;
        }

        public override string GetLevelDescription(int level)
        {
            string buffDescriptions = "";

            foreach (Helpers.PlayerBuffData buff in buffs)
            {
                Helpers.PlayerBuffData scaledBuff = CreateScaledBuff(buff, level);
                buffDescriptions += $"\n• {scaledBuff.GetDescription()}";
            }

            return $"{description}{buffDescriptions}";
        }

        #region Validation

        protected override void OnValidate()
        {
            base.OnValidate();

            levelScaling = Mathf.Max(1f, levelScaling);
            effectDuration = Mathf.Max(0.1f, effectDuration);

            // Ensure at least one buff is configured
            if (buffs.Count == 0)
            {
                Debug.LogWarning($"[BuffSkill] {name} has no buffs configured! Add at least one buff.");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Adds a buff to this skill (useful for creating skills via code).
        /// </summary>
        public void AddBuff(Helpers.PlayerBuffData buff)
        {
            buffs.Add(buff);
        }

        /// <summary>
        /// Removes all buffs from this skill.
        /// </summary>
        public void ClearBuffs()
        {
            buffs.Clear();
        }

        /// <summary>
        /// Gets the total number of buffs this skill applies.
        /// </summary>
        public int GetBuffCount()
        {
            return buffs.Count;
        }

        #endregion
    }
}
