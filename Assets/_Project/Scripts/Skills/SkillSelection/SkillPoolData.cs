using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Configuration for rarity-based skill selection weights.
    /// </summary>
    [System.Serializable]
    public class RarityWeights
    {
        [Tooltip("Weight for Common rarity")]
        [Range(1f, 100f)]
        public float common = 60f;

        [Tooltip("Weight for Uncommon rarity")]
        [Range(1f, 100f)]
        public float uncommon = 25f;

        [Tooltip("Weight for Rare rarity")]
        [Range(1f, 100f)]
        public float rare = 12f;

        [Tooltip("Weight for Epic rarity")]
        [Range(1f, 100f)]
        public float epic = 3f;

        [Tooltip("Weight for Legendary rarity")]
        [Range(1f, 100f)]
        public float legendary = 1f;

        /// <summary>
        /// Gets the weight for a specific rarity.
        /// </summary>
        public float GetWeight(SkillRarity rarity)
        {
            switch (rarity)
            {
                case SkillRarity.Common: return common;
                case SkillRarity.Uncommon: return uncommon;
                case SkillRarity.Rare: return rare;
                case SkillRarity.Epic: return epic;
                case SkillRarity.Legendary: return legendary;
                default: return 1f;
            }
        }
    }

    /// <summary>
    /// Central skill pool configuration.
    /// Manages all available skills and generates weighted random skill offers.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillPool", menuName = "RoombaRampage/Skill Pool", order = 10)]
    public class SkillPoolData : ScriptableObject
    {
        [Header("Skill Pool")]
        [Tooltip("All skills available in the game for selection")]
        public List<SkillData> availableSkills = new List<SkillData>();

        [Header("Selection Settings")]
        [Tooltip("Number of skills offered per level-up")]
        [Range(2, 5)]
        public int skillOffersPerLevelUp = 3;

        [Tooltip("Prioritize upgrades over new skills?")]
        public bool favorUpgrades = true;

        [Tooltip("Weight multiplier for upgrades (higher = upgrades appear more often)")]
        [Range(1f, 5f)]
        public float upgradeWeightMultiplier = 2f;

        [Header("Rarity Weights")]
        [Tooltip("Selection weights by rarity (higher = more common)")]
        public RarityWeights rarityWeights = new RarityWeights();

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        /// <summary>
        /// Generates a list of skill offers for level-up selection.
        /// </summary>
        /// <param name="playerSkills">The player's SkillManager</param>
        /// <param name="count">Number of offers to generate</param>
        /// <returns>List of skill offers</returns>
        public List<SkillOffer> GenerateSkillOffers(SkillManager playerSkills, int count)
        {
            List<SkillOffer> offers = new List<SkillOffer>();
            List<SkillData> eligibleSkills = GetEligibleSkills(playerSkills);

            if (eligibleSkills.Count == 0)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[SkillPool] No eligible skills available! All skills may be maxed.");
                return offers;
            }

            // Generate offers ensuring no duplicates
            HashSet<SkillData> usedSkills = new HashSet<SkillData>();

            for (int i = 0; i < count && eligibleSkills.Count > 0; i++)
            {
                SkillOffer offer = SelectWeightedRandomSkill(eligibleSkills, playerSkills, usedSkills);

                if (offer != null)
                {
                    offers.Add(offer);
                    usedSkills.Add(offer.skillData);

                    if (showDebugLogs)
                    {
                        string type = offer.isNewSkill ? "NEW" : "UPGRADE";
                        Debug.Log($"[SkillPool] Offer {i + 1}: {offer.skillData.skillName} ({type}, {offer.rarity})");
                    }
                }
            }

            return offers;
        }

        /// <summary>
        /// Selects a weighted random skill from eligible pool.
        /// </summary>
        private SkillOffer SelectWeightedRandomSkill(List<SkillData> eligibleSkills, SkillManager playerSkills, HashSet<SkillData> usedSkills)
        {
            // Build weighted list
            List<WeightedSkillEntry> weightedList = new List<WeightedSkillEntry>();
            float totalWeight = 0f;

            foreach (SkillData skill in eligibleSkills)
            {
                // Skip if already used in this selection round
                if (usedSkills.Contains(skill))
                    continue;

                int currentLevel = playerSkills.GetSkillLevel(skill);
                bool isUpgrade = currentLevel > 0;
                SkillRarity rarity = isUpgrade ? skill.upgradeRarity : skill.baseRarity;

                // Calculate weight
                float baseWeight = rarityWeights.GetWeight(rarity);
                float finalWeight = isUpgrade && favorUpgrades ? baseWeight * upgradeWeightMultiplier : baseWeight;

                weightedList.Add(new WeightedSkillEntry
                {
                    skill = skill,
                    weight = finalWeight,
                    currentLevel = currentLevel,
                    rarity = rarity
                });

                totalWeight += finalWeight;
            }

            if (weightedList.Count == 0)
                return null;

            // Select weighted random
            float randomValue = Random.Range(0f, totalWeight);
            float cumulativeWeight = 0f;

            foreach (WeightedSkillEntry entry in weightedList)
            {
                cumulativeWeight += entry.weight;

                if (randomValue <= cumulativeWeight)
                {
                    int targetLevel = entry.currentLevel + 1;
                    return new SkillOffer(entry.skill, entry.currentLevel, targetLevel, entry.rarity);
                }
            }

            // Fallback: return last entry
            WeightedSkillEntry fallback = weightedList[weightedList.Count - 1];
            return new SkillOffer(fallback.skill, fallback.currentLevel, fallback.currentLevel + 1, fallback.rarity);
        }

        /// <summary>
        /// Gets list of skills eligible for selection.
        /// </summary>
        private List<SkillData> GetEligibleSkills(SkillManager playerSkills)
        {
            List<SkillData> eligible = new List<SkillData>();

            foreach (SkillData skill in availableSkills)
            {
                if (skill == null || !skill.canBeOffered)
                    continue;

                int currentLevel = playerSkills.GetSkillLevel(skill);

                // Include if player doesn't have it, or if it can be upgraded
                if (currentLevel == 0 || currentLevel < skill.maxLevel)
                {
                    eligible.Add(skill);
                }
            }

            return eligible;
        }

        /// <summary>
        /// Internal struct for weighted selection.
        /// </summary>
        private struct WeightedSkillEntry
        {
            public SkillData skill;
            public float weight;
            public int currentLevel;
            public SkillRarity rarity;
        }

        #region Validation

        private void OnValidate()
        {
            skillOffersPerLevelUp = Mathf.Max(1, skillOffersPerLevelUp);
            upgradeWeightMultiplier = Mathf.Max(1f, upgradeWeightMultiplier);
        }

        #endregion
    }
}
