using UnityEngine;

namespace RoombaRampage.Skills
{
    /// <summary>
    /// Base ScriptableObject for all skill data.
    /// Skills are passive abilities that autofire on cooldown.
    /// </summary>
    public abstract class SkillData : ScriptableObject
    {
        [Header("Skill Info")]
        [Tooltip("Name of the skill")]
        public string skillName = "New Skill";

        [Tooltip("Description of what the skill does")]
        [TextArea(2, 4)]
        public string description = "";

        [Tooltip("Icon sprite for UI display")]
        public Sprite icon;

        [Header("Cooldown Settings")]
        [Tooltip("Cooldown between activations (seconds)")]
        [Range(0.1f, 10f)]
        public float cooldown = 1f;

        [Tooltip("Should skill autofire when off cooldown?")]
        public bool autoFire = true;

        [Header("Level Scaling")]
        [Tooltip("Starting level of the skill")]
        [Range(1, 10)]
        public int startingLevel = 1;

        [Tooltip("Maximum level the skill can reach")]
        [Range(1, 10)]
        public int maxLevel = 5;

        /// <summary>
        /// Activates the skill. Override in derived classes.
        /// </summary>
        /// <param name="caster">The GameObject that casts the skill (usually player)</param>
        /// <param name="level">Current level of the skill</param>
        public abstract void Activate(GameObject caster, int level);

        /// <summary>
        /// Gets the cooldown for this skill at a specific level.
        /// Can be overridden to implement level-based cooldown reduction.
        /// </summary>
        public virtual float GetCooldown(int level)
        {
            return cooldown;
        }

        /// <summary>
        /// Gets a description with level-specific values.
        /// </summary>
        public virtual string GetLevelDescription(int level)
        {
            return description;
        }

        #region Validation

        protected virtual void OnValidate()
        {
            startingLevel = Mathf.Max(1, startingLevel);
            maxLevel = Mathf.Max(startingLevel, maxLevel);
            cooldown = Mathf.Max(0.1f, cooldown);
        }

        #endregion
    }
}
