using UnityEngine;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Represents a single skill offer during level-up selection.
    /// Can be a new skill or an upgrade to an existing skill.
    /// </summary>
    [System.Serializable]
    public class SkillOffer
    {
        /// <summary>
        /// The skill being offered.
        /// </summary>
        public SkillData skillData;

        /// <summary>
        /// Current level of the skill (0 = new skill, player doesn't have it).
        /// </summary>
        public int currentLevel;

        /// <summary>
        /// Level the skill will be after selection.
        /// </summary>
        public int targetLevel;

        /// <summary>
        /// Rarity of this offer (affects selection weight and visuals).
        /// </summary>
        public SkillRarity rarity;

        /// <summary>
        /// Is this an upgrade to an existing skill?
        /// </summary>
        public bool isUpgrade => currentLevel > 0;

        /// <summary>
        /// Is this a new skill the player doesn't have yet?
        /// </summary>
        public bool isNewSkill => currentLevel == 0;

        /// <summary>
        /// Creates a new skill offer.
        /// </summary>
        public SkillOffer(SkillData skill, int current, int target, SkillRarity rarity)
        {
            this.skillData = skill;
            this.currentLevel = current;
            this.targetLevel = target;
            this.rarity = rarity;
        }

        /// <summary>
        /// Gets a formatted string showing the level progression.
        /// </summary>
        public string GetLevelText()
        {
            if (isNewSkill)
            {
                return "NEW!";
            }
            else
            {
                return $"Level {currentLevel} → {targetLevel}";
            }
        }

        /// <summary>
        /// Gets the skill description for the target level.
        /// </summary>
        public string GetDescription()
        {
            return skillData.GetLevelDescription(targetLevel);
        }
    }
}
